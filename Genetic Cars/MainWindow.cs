using System;
using System.Reflection;
using System.Windows.Forms;
using log4net;

namespace Genetic_Cars
{
  public partial class MainWindow : Form
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    private const float AspectRatio = 4f / 3f;

    private bool m_paused = false;

    public MainWindow()
    {
      InitializeComponent();
      mutationRateTextBox.Text = 
        Properties.Settings.Default.DefaultMutationRate.ToString();
    }

    /// <summary>
    /// Handles the request for a seed change.
    /// </summary>
    /// <param name="seed">
    /// The string used for the seed.  May be null or empty.
    /// </param>
    public delegate void SeedChangedHandler(string seed);

    /// <summary>
    /// Handles a request to pause the world simulation.
    /// </summary>
    public delegate void PauseSimulationHandler();

    /// <summary>
    /// Handles a request to resume the world simulation.
    /// </summary>
    public delegate void ResumeSimulationHandler();

    /// <summary>
    /// Handles a request to change the GA mutation rate.
    /// </summary>
    /// <param name="rate">The new mutation rate [0, 1]</param>
    public delegate void MutationRateChangedHandler(float rate);

    public SeedChangedHandler SeedChanged;
    public PauseSimulationHandler PauseSimulation;
    public ResumeSimulationHandler ResumeSimulation;
    public MutationRateChangedHandler MutationRateChanged;

    /// <summary>
    /// The handle for the SFML drawing surface.
    /// </summary>
    public IntPtr DrawingSurfaceHandle
    {
      get { return drawingSurface.Handle; }
    }

    private void OnSeedChanged(string seed)
    {
      if (SeedChanged != null)
      {
        SeedChanged(seed);
      }
    }

    private void OnPauseSimulation()
    {
      if (PauseSimulation != null)
      {
        PauseSimulation();
      }
    }

    private void OnResumeSimulation()
    {
      if (ResumeSimulation != null)
      {
        ResumeSimulation();
      }
    }

    private void OnMutationRateChanged(float rate)
    {
      if (MutationRateChanged != null)
      {
        MutationRateChanged(rate);
      }
    }

    #region Event Handlers
    private void seedApplyButton_Click(object sender, EventArgs e)
    {
      OnPauseSimulation();

      var apply = true;
      var seed = seedTextBox.Text;
      if (string.IsNullOrEmpty(seed))
      {
        var answer = MessageBox.Show(
          "No seed entered.  Click OK to use the current date/time string.", 
          "", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        apply = answer == DialogResult.OK;
        if (apply)
        {
          seed = DateTime.Now.ToString("F");
        }
      }

      if (apply)
      {
        var result = MessageBox.Show(
          "Applying a new seed will reset the world.  Continue?",
          "Apply seed?", MessageBoxButtons.YesNo, MessageBoxIcon.Question
          );
        if (result == DialogResult.Yes)
        {
          OnSeedChanged(seed);
        }
      }
      
      OnResumeSimulation();
    }
    
    private void newPopulationButton_Click(object sender, EventArgs e)
    {
      OnPauseSimulation();
      MessageBox.Show("I'm not implemented yet :(", "Oops", MessageBoxButtons.OK);
      OnResumeSimulation();
    }

    private void rapidSimButton_Click(object sender, EventArgs e)
    {
      OnPauseSimulation();
      MessageBox.Show("I'm not implemented yet :(", "Oops", MessageBoxButtons.OK);
      OnResumeSimulation();
    }

    private void pauseButton_Click(object sender, EventArgs e)
    {
      if (m_paused)
      {
        pauseButton.Text = "Pause";
        OnResumeSimulation();
        m_paused = false;
      }
      else
      {
        pauseButton.Text = "Resume";
        OnPauseSimulation();
        m_paused = true;
      }
    }

    private void mutationRateApplyButton_Click(object sender, EventArgs e)
    {
      float rate = -1;
      if (float.TryParse(mutationRateTextBox.Text, out rate) && 
        (rate >= 0 || rate <= 1))
      {
        OnMutationRateChanged(rate);
      }
      else
      {
        OnPauseSimulation();
        MessageBox.Show("Invalid mutation rate value.", "Error",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
        OnResumeSimulation();
      }
    }
    #endregion
  }
}
