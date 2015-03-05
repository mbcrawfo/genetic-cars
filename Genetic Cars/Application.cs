using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using FarseerPhysics.Dynamics;
using Genetic_Cars.Car;
using Genetic_Cars.Properties;
using log4net;
using Microsoft.Xna.Framework;
using NLua;
using SFML.Graphics;
using SFML.Window;
using View = SFML.Graphics.View;

// ReSharper disable RedundantDefaultMemberInitializer

namespace Genetic_Cars
{
  sealed class Application : PhysicsManager, IDisposable
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    // cap framerate to 65 fps
    private const float DrawingTickInterval = 1f / 65f;
    // logic updates at 30 fps, time in s
    private const float LogicTickInterval = 1f / 30f;
    // physics updates at 60 fps, Farseer uses time in seconds
    private const float PhysicsTickInterval = 1f / 60f;
    // attempt to maintain 30 fps in ms
    private const long TargetFrameTime = (long)(1000f / 30f);
    private static readonly Vector2 Gravity = new Vector2(0f, -9.8f);
    private const float ViewBaseWidth = 20f;

    private bool m_disposed = false;
    private bool m_initialized = false;
    private bool m_newWorld = false;
    private bool m_exit = false;

    // frame state variables
    private readonly Stopwatch m_frameTime = new Stopwatch();
    private readonly Stopwatch m_drawingTime = new Stopwatch();
    private readonly Stopwatch m_physicsTime = new Stopwatch();
    private readonly Stopwatch m_logicTime = new Stopwatch();
    private float m_lastDrawingStepDelta;
    private float m_lastPhysicsStepDelta;
    private float m_lastLogicStepDelta;
    private bool m_paused = false;
    private bool m_renderEnabled = true;

    // rendering state variables
    private MainWindow m_window;
    // can't be initialized until after the window is shown
    private RenderWindow m_drawingWindow;
    private View m_drawingView;
    private float m_renderWindowBaseWidth;
    private RenderWindow m_overviewWindow;
    private View m_overviewView;

    // game data
    private Track m_track;
    private Population m_population;
    private readonly RectangleShape m_viewShape = new RectangleShape
    {
      FillColor = Color.Transparent,
      OutlineColor = Color.Black,
      OutlineThickness = 1
    };
    private Lua m_luaState;
    
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
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
      Settings.Default.Save();
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
      // gui window
      m_window = new MainWindow();
      m_window.Show();
      m_window.PauseSimulation += PauseSimulation;
      m_window.ResumeSimulation += ResumeSimulation;
      m_window.TrackSeedChanged += WindowOnTrackSeedChanged;
      m_window.PopulationSeedChanged += WindowOnPopulationSeedChanged;
      m_window.NewPopulation += WindowOnNewPopulation;
      m_window.EnableGraphics += EnableRender;
      m_window.DisableGraphics += DisableRender;
      m_window.LuaLoad += LuaLoad;
      
      // main SFML panel and view
      m_drawingWindow = new RenderWindow(
        m_window.DrawingPanelHandle,
        new ContextSettings { AntialiasingLevel = 8 }
        );
      Log.DebugFormat("RenderWindow created size {0}", m_drawingWindow.Size);
      var size = m_drawingWindow.Size;
      m_renderWindowBaseWidth = size.X;
      var ratio = (float)size.Y / size.X;
      m_drawingView = new View
      {
        Size = new Vector2f(ViewBaseWidth, ViewBaseWidth * ratio),
        Center = new Vector2f(0, -2),
        Viewport = new FloatRect(0, 0, 1, 1)
      };
      m_drawingWindow.Resized += DrawingWindowOnResized;
      m_viewShape.Size = m_drawingView.Size;
      m_viewShape.Origin = m_drawingView.Size / 2;

      // overview SFML panel
      m_overviewWindow = new RenderWindow(
        m_window.OverviewPanelHandle,
        new ContextSettings { AntialiasingLevel = 8 }
        );
      m_overviewWindow.Resized += OverviewWindowOnResized;

      var seed = DateTime.Now.ToString("F");
      Log.DebugFormat("Initial seed string: {0}", seed);
      SetTrackSeed(seed.GetHashCode());
      SetPopulationSeed(seed.GetHashCode());
      GenerateWorld();
      
      Phenotype.MutateStrategy = Phenotype.DefaultMutator;
      Phenotype.CrossoverStrategy = Phenotype.DefaultCrossOver;
      
      Properties.Settings.Default.PropertyChanged += (sender, args) =>
      {
        switch (args.PropertyName)
        {
          case "NumClones":
            m_population.NumClones = Properties.Settings.Default.NumClones;
            break;

          case "NumRandom":
            m_population.NumRandom = Properties.Settings.Default.NumRandom;
            break;

          case "MutationRate":
            m_population.MutationRate = Properties.Settings.Default.MutationRate;
            break;
        }
      };

      m_initialized = true;
    }
    
    /// <summary>
    /// Executes the program.
    /// </summary>
    public void Run()
    {
      while (m_window.Visible && !m_exit)
      {
        m_frameTime.Restart();

        if (!m_paused)
        {
          if (m_renderEnabled)
          {
            GraphicalRealtimeUpdate();
          }
          else
          {
            NonGraphicalRapidUpdate();
          }
        }
        
        System.Windows.Forms.Application.DoEvents();
        m_drawingWindow.DispatchEvents();
        m_overviewWindow.DispatchEvents();

        if (m_renderEnabled && m_frameTime.ElapsedMilliseconds < TargetFrameTime)
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

        m_overviewView.Dispose();
        m_overviewWindow.Dispose();

        m_drawingView.Dispose();
        m_drawingWindow.Dispose();
        
        m_window.Dispose();

        if (m_luaState != null)
        {
          m_luaState.Dispose();
        }
      }

      m_disposed = true;
    }

    /// <summary>
    /// Update the program using real time, draw graphics.
    /// </summary>
    private void GraphicalRealtimeUpdate()
    {
      Draw();
      PhysicsRealtime();
      LogicRealtime();
    }

    private void Draw()
    {
      m_lastDrawingStepDelta += (float) m_drawingTime.Elapsed.TotalSeconds;
      m_drawingTime.Restart();
      if (m_lastDrawingStepDelta >= DrawingTickInterval)
      {
        m_lastDrawingStepDelta = 0;

        var following = m_window.FollowingCarId;
        Vector2f viewPos;
        if (following == MainWindow.LeaderCarId)
        {
          viewPos = m_population.Leader.Position.ToVector2f().InvertY();
        }
        else
        {
          var car = m_population.GetCar(following);
          if (!car.IsAlive)
          {
            m_window.FollowingCarId = MainWindow.LeaderCarId;
          }
          viewPos = car.Position.ToVector2f().InvertY();
        }
        m_drawingView.Center = viewPos;
        m_viewShape.Position = viewPos;

        // draw main window (track and cars)
        m_drawingWindow.SetView(m_drawingView);
        m_drawingWindow.Clear(Color.White);
        m_track.Draw(m_drawingWindow);
        m_population.Draw(m_drawingWindow);
        m_drawingWindow.Display();

        DrawOverview();
      }
    }

    private void DrawOverview()
    {
      m_overviewWindow.SetView(m_overviewView);
      m_overviewWindow.Clear(Color.White);
      m_track.DrawOverview(m_overviewWindow);
      m_population.DrawOverview(m_overviewWindow);
      m_overviewWindow.Draw(m_viewShape);
      m_overviewWindow.Display();
    }

    private void PhysicsRealtime()
    {
      m_lastPhysicsStepDelta += (float)m_physicsTime.Elapsed.TotalSeconds;
      m_physicsTime.Restart();
      while (m_lastPhysicsStepDelta >= PhysicsTickInterval)
      {
        m_lastPhysicsStepDelta -= PhysicsTickInterval;
        StepWorld(PhysicsTickInterval);
      }
    }

    private void LogicRealtime()
    {
      m_lastLogicStepDelta += (float)m_logicTime.Elapsed.TotalSeconds;
      m_logicTime.Restart();
      while (m_lastLogicStepDelta >= LogicTickInterval)
      {
        m_lastLogicStepDelta -= LogicTickInterval;

        if (m_newWorld)
        {
          m_newWorld = false;
          m_window.ResetUi();
          var seed = DateTime.Now.ToString("F");
          SetTrackSeed(seed.GetHashCode());
          SetPopulationSeed(seed.GetHashCode());
          GenerateWorld();
        }

        m_population.Update(LogicTickInterval);
        
        // sync the gui text
        m_window.SetDistance(m_population.Leader.MaxForwardDistance);
        m_window.SetLiveCount(m_population.LiveCount);
        m_window.SetFollowingNumber(
          m_window.FollowingCarId == MainWindow.LeaderCarId ? 
          m_population.Leader.Id : m_window.FollowingCarId);
      }
    }

    /// <summary>
    /// Updates the physics and logic simulation by one physics tick.  Each 
    /// call performs an update regardless of the elapsed wall time.  The 
    /// overview is updated every 250 ms.
    /// </summary>
    private void NonGraphicalRapidUpdate()
    {
      m_lastDrawingStepDelta += (float)m_drawingTime.Elapsed.TotalSeconds;
      m_drawingTime.Restart();
      if (m_lastDrawingStepDelta >= 0.5f)
      {
        m_lastDrawingStepDelta = 0;
        DrawOverview();
      }

      StepWorld(PhysicsTickInterval);
      m_population.Update(PhysicsTickInterval);

      if (m_newWorld)
      {
        m_newWorld = false;
        m_window.ResetUi();
        var seed = DateTime.Now.ToString("F");
        SetTrackSeed(seed.GetHashCode());
        SetPopulationSeed(seed.GetHashCode());
        GenerateWorld();
      }
    }

    private bool LuaLoad(string path, out string error)
    {
      error = "";
      var newLua = new Lua();

      // load .net packages then clear the import function
      //newLua.LoadCLRPackage();
      //newLua.DoString(@"import('System')");
      //newLua.DoString(@"import('System.Text')");
      newLua.DoString(@"import = function () end");

      // set globals for scripts to use
      newLua["Log"] = LogManager.GetLogger("Lua");
      newLua["RNG"] = Phenotype.Random;
      newLua["GenomeLength"] = Phenotype.GenomeLength;
      
      try
      {
        newLua.DoFile(path);
      }
      catch (Exception e)
      {
        Log.ErrorFormat("Error loading file {0}", path);
        Log.Error(e);
        error = e.Message;
        return false;
      }

      var crossover = newLua.GetFunction(@"CrossOver");
      var mutate = newLua.GetFunction(@"Mutate");

      if (crossover == null || mutate == null)
      {
        if (crossover == null)
        {
          error = "Missing CrossOver function";
          Log.ErrorFormat("file {0} was missing CrossOver function",
            path);
        }
        if (mutate == null)
        {
          error += string.IsNullOrEmpty(error) ? "" : "\n";
          error += "Missing Mutate function";
          Log.ErrorFormat("file {0} was missing Mutate function", 
            path);
        }
        
        Log.Error("Mutate/Crossover functions have not been changed");
        return false;
      }

      Phenotype.CrossoverStrategy =
        (s1, s2) => (string) crossover.Call(s1, s2).First();
      Phenotype.MutateStrategy =
        genome => (string) mutate.Call(genome).First();
      Log.InfoFormat("Set crossover and mutation functions from {0}",
        path);
        
      m_luaState = newLua;
      return true;
    }

    private void SetTrackSeed(int seed)
    {
      Log.InfoFormat("Track seed set to 0x{0:X}", seed);
      var random = new Random(seed);
      Track.Random = random;
    }

    private void SetPopulationSeed(int seed)
    {
      Log.InfoFormat("Population seed set to 0x{0:X}", seed);
      var random = new Random(seed);
      Population.Random = random;
      Phenotype.Random = random;
    }

    private void GenerateWorld()
    {
      if (m_initialized)
      {
        m_track.Dispose();
        m_population.Dispose();
        m_overviewView.Dispose();
      }

      // create the world
      World = new World(Gravity);
      m_track = new Track(this);
      m_track.Generate();
      m_track.FinishLineCrossed += TrackOnFinishLineCrossed;
      Car.Car.StartPosition = new Vector2(m_track.StartingLine,
        (2 * Definition.MaxBodyPointDistance) + Definition.MaxWheelRadius);

      // rebuild the view to match the track
      var size = m_overviewWindow.Size;
      var ratio = (float) size.Y / size.X;
      m_overviewView = new View
      {
        Center = m_track.Center.ToVector2f().InvertY(),
        Size = new Vector2f(m_track.Dimensions.X, m_track.Dimensions.X * ratio),
        Viewport = new FloatRect(0, 0, 1, 1)
      };

      ResetPopulation();
    }

    private void ResetPopulation()
    {
      if (m_initialized)
      {
        m_population.Dispose();
      }

      m_population = new Population(this);
      m_population.NewGeneration += m_window.NewGeneration;
      m_population.NewChampion += m_window.AddChampion;
      m_population.Generate();
    }
    
    #region Event Handlers

    private void PauseSimulation()
    {
      m_paused = true;

      m_drawingTime.Stop();
      m_physicsTime.Stop();
      m_logicTime.Stop();
    }

    private void ResumeSimulation()
    {
      m_paused = false;

      // don't touch the timers if rendering is disabled
      if (!m_renderEnabled)
      {
        return;
      }

      m_drawingTime.Start();
      m_frameTime.Start();
      m_logicTime.Start();
    }

    /// <summary>
    /// Resizes the SFML view to avoid object distortion.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DrawingWindowOnResized(object sender, SizeEventArgs e)
    {
      var newWidth = (ViewBaseWidth / m_renderWindowBaseWidth) * e.Width;
      var ratio = (float)e.Height / e.Width;
      m_drawingView.Size = new Vector2f(newWidth, newWidth * ratio);
      m_viewShape.Size = m_drawingView.Size;
      m_viewShape.Origin = m_drawingView.Size / 2;
    }

    private void OverviewWindowOnResized(object sender, SizeEventArgs e)
    {
      var ratio = (float)e.Height / e.Width;
      m_overviewView.Size =
        new Vector2f(m_track.Dimensions.X, m_track.Dimensions.X * ratio);
    }

    private void WindowOnTrackSeedChanged(int seed)
    {
      SetTrackSeed(seed);
      SetPopulationSeed(seed);
      GenerateWorld();
    }

    private void WindowOnPopulationSeedChanged(int seed)
    {
      SetPopulationSeed(seed);
      ResetPopulation();
    }

    private void WindowOnNewPopulation()
    {
      if (m_population != null)
      {
        m_population.Generate();
      }
    }

    private void TrackOnFinishLineCrossed(int id)
    {
      PauseSimulation();

      // render a zoomed in view of the winning car
      const float width = 8;
      var ratio = m_drawingView.Size.Y / m_drawingView.Size.X;
      View winnerView = new View
      {
        Center = m_population.Leader.Position.ToVector2f().InvertY(),
        Size = new Vector2f(width, width * ratio),
        Viewport = new FloatRect(0, 0, 1, 1)
      };
      m_drawingWindow.SetView(winnerView);
      m_drawingWindow.Clear(Color.White);
      m_track.Draw(m_drawingWindow);
      m_population.Draw(m_drawingWindow);
      m_drawingWindow.Display();

      // save a screenshot
      var filename = string.Format(
        "winner_gen{0}_car{1}_{2:yyyy-MM-dd_HH-mm-ss}.png",
        m_population.Generation, id, DateTime.Now);
      m_drawingWindow.Capture().SaveToFile(filename);
      Log.InfoFormat("Screenshot of winner saved to {0}", filename);

      var message = string.Format(
        "Track defeated in generation {0} by car {1}.\n" +
        "Click Yes to generate a new world with a new seed, or No to exit.",
        m_population.Generation, id
        );
      var result = MessageBox.Show(message, "Great Success", 
        MessageBoxButtons.YesNo, MessageBoxIcon.None);
      if (result == DialogResult.Yes)
      {
        m_newWorld = true;
      }
      else
      {
        m_exit = true;
      }

      ResumeSimulation();
    }

    private void EnableRender()
    {
      m_renderEnabled = true;
      m_drawingTime.Restart();
      m_physicsTime.Restart();
      m_logicTime.Restart();
    }

    private void DisableRender()
    {
      m_renderEnabled = false;

      m_lastDrawingStepDelta = 0;
      m_lastPhysicsStepDelta = 0;
      m_lastLogicStepDelta = 0;

      m_drawingTime.Restart();
      m_physicsTime.Stop();
      m_logicTime.Stop();
    }

    #endregion
  }
}
