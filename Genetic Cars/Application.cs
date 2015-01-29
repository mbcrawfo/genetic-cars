using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;
using Color = SFML.Graphics.Color;

namespace Genetic_Cars
{
  class Application
  {
    #region Constants
    // logic updates at 30 fps
    private const long LogicTickInterval = (long)(1000f / 30f);
    // physics updates at 60 fps
    private const float PhysicsTickInterval = 1f / 60f;
    // attempt to maintain 30 fps
    private const long TargetFrameTime = (long)(1000f / 30f);
    private static readonly Vector2 Gravity = new Vector2(0f, -9.8f);
    #endregion

    // frame state variables
    private readonly Stopwatch m_frameTime = new Stopwatch();
    private readonly Stopwatch m_physicsTime = new Stopwatch();
    private long m_lastFrameTotalTime = 0;
    private long m_logicDelta = 0;
    private float m_lastPhysicsStepDelta = 0f;

    // rendering state variables
    private readonly MainWindow m_window = new MainWindow();
    // can't be initialized until after the window is shown
    private readonly SFML.Graphics.RenderWindow m_renderWindow;
    private readonly SFML.Graphics.View m_view;

    // physics state variables
    private readonly World m_world;

    // game data
    private Random m_random;
    private readonly Track m_track;
    
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      //System.Windows.Forms.Application.EnableVisualStyles();
      //System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
      new Application().Run();
    }

    /// <summary>
    /// Creates the app in its initial state with a world generated using a 
    /// seed based on the current time.
    /// </summary>
    public Application()
    {
      // initialize the rendering components
      m_window.Show();
      m_renderWindow = new RenderWindow(
        m_window.DrawingSurfaceHandle,
        new ContextSettings { AntialiasingLevel = 8 }
        );
      var size = m_renderWindow.Size;
      m_view = new SFML.Graphics.View
      {
        Size = new Vector2f(10, 7.5f),
        Center = new Vector2f(0, -2),
        Viewport = new FloatRect(0, 0, 1, 1)
      };

      // create the world
      m_random = new Random();
      m_world = new World(Gravity);
      m_track = new Track(m_world);
      m_track.Generate(m_random);
    }

    /// <summary>
    /// Executes the program.
    /// </summary>
    public void Run()
    {
      while (m_window.Visible)
      {
        m_lastFrameTotalTime = m_frameTime.ElapsedMilliseconds;
        m_frameTime.Restart();

        DoDrawing();
        DoPhysics();
        System.Windows.Forms.Application.DoEvents();
        DoLogic();

        if (m_frameTime.ElapsedMilliseconds < TargetFrameTime)
        {
          Thread.Sleep(1);
        }
      }
    }

    private void DoLogic()
    {
      m_renderWindow.DispatchEvents();
      
      m_logicDelta += m_lastFrameTotalTime;
      while (m_logicDelta >= LogicTickInterval)
      {
        m_logicDelta -= LogicTickInterval;
        
        m_view.Move(new Vector2f(0.1f, 0));
      }
    }

    private void DoDrawing()
    {
      m_renderWindow.SetView(m_view);

      m_renderWindow.Clear(Color.White);
      m_track.Draw(m_renderWindow);

      m_renderWindow.Display();
    }

    private void DoPhysics()
    {
      m_lastPhysicsStepDelta += (float)m_physicsTime.Elapsed.TotalSeconds;
      while (m_lastPhysicsStepDelta >= PhysicsTickInterval)
      {
        m_lastPhysicsStepDelta -= PhysicsTickInterval;
        m_world.Step(PhysicsTickInterval);
        m_world.ClearForces();
      }
      m_physicsTime.Restart();
    }
  }
}
