using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using SFML.Graphics;

namespace Genetic_Cars
{
  class Application
  {
    private const long LogicTickInterval = (long)(1000.0f / 30.0f);
    private const long MaxFrameTime = (long)(1000.0f / 30.0f);

    private MainWindow m_window;
    private RenderWindow m_renderWindow;

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
    /// Executes the program.
    /// </summary>
    public void Run()
    {
      Initialize();
      MainLoop();
    }

    /// <summary>
    /// Initialize everything.
    /// </summary>
    private void Initialize()
    {
      m_window = new MainWindow();
      m_window.Show();
      m_renderWindow = new RenderWindow(m_window.DrawingSurfaceHandle);
    }

    /// <summary>
    /// The program main loop.
    /// </summary>
    private void MainLoop()
    {
      Stopwatch frameTime = new Stopwatch();
      Stopwatch physicsTime = new Stopwatch();
      long logicDelta = 0;

      while (m_window.Visible)
      {
        frameTime.Restart();

        m_renderWindow.Clear(Color.Yellow);
        m_renderWindow.Display();
        // update physics
        System.Windows.Forms.Application.DoEvents();

        logicDelta += frameTime.ElapsedMilliseconds;
        while (logicDelta >= LogicTickInterval)
        {
          logicDelta -= LogicTickInterval;
          // update logic
          m_renderWindow.DispatchEvents();
        }

        if (frameTime.ElapsedMilliseconds < MaxFrameTime)
        {
          Thread.Sleep(1);
        }
      }
    }
  }
}
