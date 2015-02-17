using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using Genetic_Cars.Properties;
using log4net;
// ReSharper disable LocalizableElement
// ReSharper disable RedundantDefaultMemberInitializer

namespace Genetic_Cars
{
  public partial class MainWindow : Form
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    private bool m_paused = false;

    public MainWindow()
    {
      InitializeComponent();

      // initialize default values
      mutationRateTextBox.Text = 
        Settings.Default.DefaultMutationRate.ToString(
        CultureInfo.CurrentCulture);

      
      clonesComboBox.Items.Clear();
      var maxClones = Settings.Default.PopulationSize / 2;
      for (var i = 0; i <= maxClones; i++)
      {
        clonesComboBox.Items.Add(i);
      }
      clonesComboBox.SelectedIndex = 2;
    }

    /// <summary>
    /// Handles the request for a seed change.
    /// </summary>
    /// <param name="seed"></param>
    public delegate void SeedChangedHandler(int seed);

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

    /// <summary>
    /// Handles a request to generate a new population.
    /// </summary>
    public delegate void NewPopulationHandler();

    /// <summary>
    /// Handles a request to changed the number of clones in each generation.
    /// </summary>
    /// <param name="num"></param>
    public delegate void NumClonesChangedHandler(int num);

    public event SeedChangedHandler SeedChanged;
    public event PauseSimulationHandler PauseSimulation;
    public event ResumeSimulationHandler ResumeSimulation;
    public event MutationRateChangedHandler MutationRateChanged;
    public event NewPopulationHandler NewPopulation;
    public event NumClonesChangedHandler NumClonesChanged;

    /// <summary>
    /// The handle for the main SFML drawing surface.
    /// </summary>
    public IntPtr DrawingPanelHandle
    {
      get { return drawingPanel.Handle; }
    }

    /// <summary>
    /// The handle for the overview SFML drawing surface.
    /// </summary>
    public IntPtr OverviewPanelHandle
    {
      get { return overviewPanel.Handle; }
    }

    /// <summary>
    /// Set the text indicating the current generation.
    /// </summary>
    /// <param name="generation"></param>
    public void SetGeneration(int generation)
    {
      generationLabel.Text = string.Format("Generation: {0}",
        generation);
    }

    /// <summary>
    /// Set the text indicating the number of live cars.
    /// </summary>
    /// <param name="count"></param>
    public void SetLiveCount(int count)
    {
      liveCountLabel.Text = string.Format("Live Cars: {0}", count);
    }

    /// <summary>
    /// Set the text indicating the distance traveled for the currently 
    /// watched car.
    /// </summary>
    /// <param name="distance"></param>
    public void SetDistance(float distance)
    {
      distanceLabel.Text = string.Format("Distance: {0:F2} m", distance);
    }

    private void OnSeedChanged(int seed)
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

    private void OnNewPopulation()
    {
      if (NewPopulation != null)
      {
        NewPopulation();
      }
    }

    private void OnNumClonesChanged(int num)
    {
      if (NumClonesChanged != null)
      {
        NumClonesChanged(num);
      }
    }

    #region Event Handlers
    private void seedApplyButton_Click(object sender, EventArgs e)
    {
      OnPauseSimulation();

      var str = seedTextBox.Text;
      var apply = true;
      int seed = 0;
      if (string.IsNullOrEmpty(str))
      {
        var answer = MessageBox.Show(
          "No seed entered.  Click OK to use the current date/time string.",
          "", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        apply = answer == DialogResult.OK;
        if (apply)
        {
          str = DateTime.Now.ToString("F");
          Log.InfoFormat("Using seed string: {0}", str);
          seed = str.GetHashCode();
        }
      }
      else if (str.StartsWith(@"\x"))
      {
        str = str.Substring(2);
        const NumberStyles style = NumberStyles.HexNumber;
        var provider = CultureInfo.CurrentCulture;
        if (!int.TryParse(str, style, provider, out seed))
        {
          MessageBox.Show("Error parsing the hex value.  Seed not changed",
            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          apply = false;
        }
      }
      else if (str.StartsWith(@"\d"))
      {
        str = str.Substring(2);
        const NumberStyles style = NumberStyles.Integer;
        var provider = CultureInfo.CurrentCulture;
        if (!int.TryParse(str, style, provider, out seed))
        {
          MessageBox.Show("Error parsing the value.  Seed not changed",
            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          apply = false;
        }
      }
      else
      {
        Log.InfoFormat("Using seed string: {0}", str);
        seed = str.GetHashCode();
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

      var result = MessageBox.Show(
        "Discard the current population and start over?", "",
        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
      if (result == DialogResult.Yes)
      {
        OnNewPopulation();
      }
      
      OnResumeSimulation();
    }

    private void graphicsButton_Click(object sender, EventArgs e)
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
      float rate;
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

    private void clonesComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      var num = clonesComboBox.SelectedItem as int?;
      if (num == null)
      {
        Log.Error("Selected num clones was null");
      }
      OnNumClonesChanged(num.GetValueOrDefault());
    }
    
    private void clonesComboBox_DrawItem(object sender, DrawItemEventArgs e)
    {
      // see: http://stackoverflow.com/questions/11817062/align-text-in-combobox

      ComboBox cbx = sender as ComboBox;
      if (cbx != null)
      {
        // Always draw the background
        e.DrawBackground();

        // Drawing one of the items?
        if (e.Index >= 0)
        {
          // Set the string alignment.  Choices are Center, Near and Far
          StringFormat sf = new StringFormat();
          sf.LineAlignment = StringAlignment.Center;
          sf.Alignment = StringAlignment.Center;

          // Set the Brush to ComboBox ForeColor to maintain any ComboBox color 
          // settings
          // Assumes Brush is solid
          Brush brush = new SolidBrush(cbx.ForeColor);

          // If drawing highlighted selection, change brush
          if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            brush = SystemBrushes.HighlightText;

          // Draw the string
          e.Graphics.DrawString(
            cbx.Items[e.Index].ToString(), cbx.Font, brush, e.Bounds, sf);
        }
      }
    }
    #endregion
  }
}
