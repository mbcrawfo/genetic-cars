using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using FarseerPhysics.Dynamics;
using Genetic_Cars.Car;
using Genetic_Cars.Properties;
using log4net;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

// ReSharper disable RedundantDefaultMemberInitializer

namespace Genetic_Cars
{
  sealed class Application : PhysicsManager, IDisposable
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    // logic updates at 30 fps, time in ms
    //private const long LogicTickInterval = (long)(1000f / 30f);
    // physics updates at 60 fps, Farseer uses time in seconds
    private const float PhysicsTickInterval = 1f / 60f;
    // attempt to maintain 30 fps in ms
    private const long TargetFrameTime = (long)(1000f / 30f);
    private static readonly Vector2 Gravity = new Vector2(0f, -9.8f);
    private const float ViewBaseWidth = 20f;

    private bool m_disposed = false;
    private bool m_initialized = false;

    // frame state variables
    private readonly Stopwatch m_frameTime = new Stopwatch();
    private readonly Stopwatch m_physicsTime = new Stopwatch();
    private float m_lastPhysicsStepDelta;
    private bool m_paused = false;

    // rendering state variables
    private MainWindow m_window;
    // can't be initialized until after the window is shown
    private RenderWindow m_renderWindow;
    private View m_view;
    private float m_renderWindowBaseWidth;

    // game data
    private Track m_track;
    private Population m_population;
    
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      // dump settings to the log
      Log.Info("Loaded settings:");
      foreach (SettingsProperty property in Settings.Default.Properties)
      {
        Log.InfoFormat("{0}={1}", 
          property.Name, Settings.Default[property.Name]);
      }

      FarseerPhysics.Settings.UseFPECollisionCategories = true;
      FarseerPhysics.Settings.VelocityIterations = 10;
      FarseerPhysics.Settings.PositionIterations = 8;
      FarseerPhysics.Settings.MaxPolygonVertices = Definition.NumBodyPoints;

      // not sure what this does, leftover from the project generation
      System.Windows.Forms.Application.EnableVisualStyles();
      System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

      var app = new Application();
      app.Initialize();
      app.Run();
    }

    ~Application()
    {
      Dispose(false);
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
      m_window.PauseSimulation += PauseSimulation;
      m_window.ResumeSimulation += ResumeSimulation;
      m_window.SeedChanged += WindowOnSeedChanged;
      m_window.NewPopulation += WindowOnNewPopulation;
      
      m_renderWindow = new RenderWindow(
        m_window.DrawingSurfaceHandle,
        new ContextSettings { AntialiasingLevel = 8 }
        );
      Log.DebugFormat("RenderWindow created size {0}", m_renderWindow.Size);
      var size = m_renderWindow.Size;
      m_renderWindowBaseWidth = size.X;
      var ratio = (float)size.Y / size.X;
      m_view = new View
      {
        Size = new Vector2f(ViewBaseWidth, ViewBaseWidth * ratio),
        Center = new Vector2f(0, -2),
        Viewport = new FloatRect(0, 0, 1, 1)
      };
      m_renderWindow.Resized += WindowOnResized;

      var seed = DateTime.Now.ToString("F");
      Log.DebugFormat("Initial seed string: {0}", seed);
      SetSeed(seed.GetHashCode());
      GenerateWorld();
      
      m_initialized = true;
    }

    /// <summary>
    /// Executes the program.
    /// </summary>
    public void Run()
    {
      while (m_window.Visible)
      {
        m_frameTime.Restart();

        if (!m_paused)
        {
          DoDrawing();
          DoPhysics();
        }
        System.Windows.Forms.Application.DoEvents();
        m_renderWindow.DispatchEvents();

        if (m_frameTime.ElapsedMilliseconds < TargetFrameTime)
        {
          Thread.Sleep(1);
        }
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposeManaged)
    {
      if (m_disposed || !m_initialized)
      {
        return;
      }

      if (disposeManaged)
      {
        m_track.Dispose();
        m_population.Dispose();

        m_view.Dispose();
        m_renderWindow.Dispose();
        m_window.Dispose();
      }

      m_disposed = true;
    }

    private void DoDrawing()
    {
      m_view.Center = m_population.Leader.Position.ToVector2f().InvertY();
      m_renderWindow.SetView(m_view);

      m_renderWindow.Clear(Color.White);
      m_track.Draw(m_renderWindow);
      m_population.Draw(m_renderWindow);
      m_renderWindow.Display();
    }

    private void DoPhysics()
    {
      m_lastPhysicsStepDelta += (float)m_physicsTime.Elapsed.TotalSeconds;
      while (m_lastPhysicsStepDelta >= PhysicsTickInterval)
      {
        m_lastPhysicsStepDelta -= PhysicsTickInterval;
        StepWorld(PhysicsTickInterval);
      }
      m_physicsTime.Restart();
    }

    private void SetSeed(int seed)
    {
      Log.InfoFormat("RNG seed set to 0x{0:X}", seed);
      var random = new Random(seed);
      Track.Random = random;
      Population.Random = random;
      Phenotype.Random = random;
    }

    private void GenerateWorld()
    {
      if (m_initialized)
      {
        m_track.Dispose();
        m_population.Dispose();
      }

      // create the world
      World = new World(Gravity);
      m_track = new Track(this);
      m_track.Generate();
      Entity.StartPosition = new Vector2(m_track.StartingLine,
        (2 * Definition.MaxBodyPointDistance) + Definition.MaxWheelRadius);
      m_population = new Population(this);
    }

    private void PauseSimulation()
    {
      m_paused = true;
      m_frameTime.Stop();
      m_physicsTime.Stop();
    }

    private void ResumeSimulation()
    {
      m_paused = false;
      m_frameTime.Start();
      m_frameTime.Start();
    }

    #region Event Handlers
    /// <summary>
    /// Resizes the SFML view to avoid object distortion.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void WindowOnResized(object sender, SizeEventArgs e)
    {
      var newWidth = (ViewBaseWidth / m_renderWindowBaseWidth) * e.Width;
      var ratio = (float)e.Height / e.Width;
      m_view.Size = new Vector2f(newWidth, newWidth * ratio);
      Log.DebugFormat("Window resized to {0} new view size {1}",
        e, m_view.Size
        );
    }

    private void WindowOnSeedChanged(int seed)
    {
      SetSeed(seed);
      GenerateWorld();
    }

    private void WindowOnNewPopulation()
    {
      if (m_population != null)
      {
        m_population.Generate();
      }
    }
    #endregion
  }
}
