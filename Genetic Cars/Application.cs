using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using FarseerPhysics.Dynamics;
using log4net;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace Genetic_Cars
{
  class Application : IDisposable
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    // logic updates at 30 fps, time in ms
    private const long LogicTickInterval = (long)(1000f / 30f);
    // physics updates at 60 fps, Farseer uses time in seconds
    private const float PhysicsTickInterval = 1f / 60f;
    // attempt to maintain 30 fps in ms
    private const long TargetFrameTime = (long)(1000f / 30f);
    private static readonly Vector2 Gravity = new Vector2(0f, -9.8f);

    // frame state variables
    private readonly Stopwatch m_frameTime = new Stopwatch();
    private readonly Stopwatch m_physicsTime = new Stopwatch();
    private long m_lastFrameTotalTime;
    private long m_logicDelta;
    private float m_lastPhysicsStepDelta;

    // rendering state variables
    private MainWindow m_window;
    // can't be initialized until after the window is shown
    private RenderWindow m_renderWindow;
    private View m_view;

    // physics state variables
    private World m_world;

    // game data
    private Random m_random;
    private Track m_track;
    
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      System.Windows.Forms.Application.EnableVisualStyles();
      System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
      new Application().Run();
    }

    /// <summary>
    /// Creates the app in its initial state with a world generated using a 
    /// seed based on the current time.
    /// </summary>
    public void Initialize()
    {
      // initialize the rendering components
      m_window = new MainWindow();
      m_window.Show();
      m_renderWindow = new RenderWindow(
        m_window.DrawingSurfaceHandle,
        new ContextSettings { AntialiasingLevel = 8 }
        );
      Log.DebugFormat("RenderWindow created size {0}", m_renderWindow.Size);
      m_view = new View
      {
        Size = new Vector2f(10, 7.5f),
        Center = new Vector2f(0, -2),
        Viewport = new FloatRect(0, 0, 1, 1)
      };

      var seedString = DateTime.Now.ToString("F");
      m_random = new Random(seedString.GetHashCode());
      Log.InfoFormat("RNG seed string:\n{0}", seedString);
      
      // create the world
      m_world = new World(Gravity);
      m_track = new Track(m_world);
      m_track.Generate(m_random);
    }

    /// <summary>
    /// Executes the program.
    /// </summary>
    public void Run()
    {
      Initialize();

      while (m_window.Visible)
      {
        m_lastFrameTotalTime = m_frameTime.ElapsedMilliseconds;
        m_frameTime.Restart();

        DoDrawing();
        DoPhysics();
        DoLogic();

        if (m_frameTime.ElapsedMilliseconds < TargetFrameTime)
        {
          Thread.Sleep(1);
        }
      }
    }

    /// <summary>
    /// Clean up disposables, blah blah.
    /// </summary>
    public void Dispose()
    {
      if (m_renderWindow != null)
      {
        m_renderWindow.Dispose();
      }
      if (m_window != null)
      {
        m_window.Dispose();
      }
    }

    private void DoLogic()
    {
      System.Windows.Forms.Application.DoEvents();
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
