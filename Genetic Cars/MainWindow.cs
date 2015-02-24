using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Genetic_Cars.Car;
using Genetic_Cars.Properties;
using log4net;
// ReSharper disable LocalizableElement
// ReSharper disable RedundantDefaultMemberInitializer

namespace Genetic_Cars
{
  partial class MainWindow : Form
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly int PopulationSize =
      Settings.Default.PopulationSize;
    private const int MaxHighScores = 20;

    public const int LeaderCarId = -1;

    /// <summary>
    /// Handles events that use no parameters.
    /// </summary>
    public delegate void GenericHandler();
    
    /// <summary>
    /// Handles the request for a seed change.
    /// </summary>
    /// <param name="seed"></param>
    public delegate void SeedChangedHandler(int seed);
    
    private bool m_paused = false;
    private bool m_graphicsEnabled = true;
    private readonly List<HighScore> m_highScores = new List<HighScore>();
    private int m_followingCarId;

    public MainWindow()
    {
      InitializeComponent();

      var str = toolTip.GetToolTip(seedTextBox);
      toolTip.SetToolTip(seedLabel, str);
      str = toolTip.GetToolTip(mutationRateTextBox);
      toolTip.SetToolTip(mutationRateLabel, str);
      str = toolTip.GetToolTip(clonesComboBox);
      toolTip.SetToolTip(clonesLabel, str);

      // initialize default values
      mutationRateTextBox.Text = 
        Settings.Default.MutationRate.ToString(
        CultureInfo.CurrentCulture);

      FollowingCarId = LeaderCarId;

      
      clonesComboBox.Items.Clear();
      var maxClones = PopulationSize / 2;
      for (var i = 0; i <= maxClones; i++)
      {
        clonesComboBox.Items.Add(i);
      }
      clonesComboBox.SelectedIndex = Properties.Settings.Default.NumClones;

      for (var i = 0; i < PopulationSize; i++)
      {
        var pb = new ColorProgressBar
        {
          Id = i,
          Minimum = 0,
          Maximum = 100,
          Value = 100,
          Text = i.ToString(),
          Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold),
          Margin = new Padding(0)
        };
        pb.Click += (sender, args) => 
          FollowingCarId = ((ColorProgressBar) sender).Id;
        toolTip.SetToolTip(pb, string.Format("Click to view car {0}", i));

        populationList.Controls.Add(pb);
      }
    }
    
    public event SeedChangedHandler SeedChanged;
    public event GenericHandler PauseSimulation;
    public event GenericHandler ResumeSimulation;
    public event GenericHandler NewPopulation;
    public event GenericHandler EnableGraphics;
    public event GenericHandler DisableGraphics;

    /// <summary>
    /// The id of the car the user wants the camera to follow.
    /// </summary>
    public int FollowingCarId 
    { get { return m_followingCarId; }
      set
      {
        m_followingCarId = value;
        var status = m_followingCarId == LeaderCarId ? "Yes" : "No";
        followLeaderButton.Text = string.Format("Follow Leader: {0}", status);
      }
    }

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
    /// Sets the id number of the car being followed.  Needed because the gui 
    /// doesn't know the id of the leader car.
    /// </summary>
    /// <param name="num"></param>
    public void SetFollowingNumber(int num)
    {
      followingLabel.Text = string.Format("Following: Car {0}", num);
    }

    /// <summary>
    /// Set the text indicating the current generation.
    /// </summary>
    /// <param name="generation"></param>
    /// <param name="cars"></param>
    public void NewGeneration(int generation, List<Car.Car> cars)
    {
      Debug.Assert(PopulationSize == cars.Count);

      FollowingCarId = LeaderCarId;
      generationLabel.Text = string.Format(
        "Generation: {0}", generation);

      for (var i = 0; i < PopulationSize; i++)
      {
        var pb = (ColorProgressBar)populationList.Controls[i];
        var car = cars[i];
        car.HealthChanged += SetHealthValue;

        // setting them to max value makes drawing bug?  whatever...
        pb.Value = 99;
        switch (car.Type)
        {
          case EntityType.Normal:
            pb.FillColor = Color.Red;
            break;

          case EntityType.Clone:
            pb.FillColor = Color.Blue;
            break;
        }

        pb.Visible = true;
        pb.Refresh();
      }
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

    public void AddChampion(int generation, int id, float distance)
    {
      m_highScores.Add(new HighScore
      {
        Index = 0,
        Generation = generation,
        Id = id,
        Distance = distance
      });

      m_highScores.Sort();
      if (m_highScores.Count > MaxHighScores)
      {
        m_highScores.RemoveAt(m_highScores.Count - 1);
      }

      highScoreListBox.Items.Clear();
      for (var i = 0; i < m_highScores.Count; i++)
      {
        m_highScores[i].Index = i + 1;
        highScoreListBox.Items.Add(m_highScores[i].DisplayValue);
      }
    }

    /// <summary>
    /// Clear the gui components that aren't regularly updated.
    /// </summary>
    public void ResetUi()
    {
      m_highScores.Clear();
      highScoreListBox.Items.Clear();
    }
    
    private void SetHealthValue(int id, float health)
    {
      var pb = (ColorProgressBar)populationList.Controls[id];
      var value = (int)Math.Round(health * 100); 
      pb.Value = value;

      if (value <= 0)
      {
        pb.Visible = false;
      }
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

    private void OnNewPopulation()
    {
      if (NewPopulation != null)
      {
        NewPopulation();
      }
    }

    private void OnEnableGraphics()
    {
      if (EnableGraphics != null)
      {
        EnableGraphics();
      }
    }

    private void OnDisableGraphics()
    {
      if (DisableGraphics != null)
      {
        DisableGraphics();
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
          ResetUi();
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
        ResetUi();
        OnNewPopulation();
      }
      
      OnResumeSimulation();
    }

    private void graphicsButton_Click(object sender, EventArgs e)
    {
      if (m_graphicsEnabled)
      {
        m_graphicsEnabled = false;
        graphicsButton.Text = "Graphics: Off";
        OnDisableGraphics();
      }
      else
      {
        m_graphicsEnabled = true;
        graphicsButton.Text = "Graphics: On";
        OnEnableGraphics();
      }
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
        Properties.Settings.Default.MutationRate = rate;
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
      var num = (int)clonesComboBox.SelectedItem;
      Properties.Settings.Default.NumClones = num;
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

    private void followLeaderButton_Click(object sender, EventArgs e)
    {
      FollowingCarId = LeaderCarId;
    }
    #endregion

    private sealed class HighScore : IComparable
    {
      public int Index { get; set; }
      public int Generation { get; set; }
      public int Id { get; set; }
      public float Distance { get; set; }

      public string DisplayValue
      {
        get { return ToString(); }
      }

      public override string ToString()
      {
        return string.Format("{0}. Generation {1}, {2:F2} m",
          Index, Generation, Distance);
      }

      public int CompareTo(object obj)
      {
        HighScore hs = obj as HighScore;
        if (hs == null)
        {
          return 1;
        }

        if (hs.Distance < Distance)
        {
          return -1;
        }
        else if (hs.Distance == Distance)
        {
          return 0;
        }
        else
        {
          return 1;
        }
      }
    }

    /// <summary>
    /// Simple progress bar that is colorable.
    /// </summary>
    private sealed class ColorProgressBar : ProgressBar
    {
      private SolidBrush m_brush;

      public ColorProgressBar()
      {
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        SetStyle(ControlStyles.UserPaint, true);
        FillColor = Color.ForestGreen;
      }

      public int Id { get; set; }

      public Color FillColor
      {
        get { return m_brush.Color; }
        set { m_brush = new SolidBrush(value); }
      }

      protected override void OnPaint(PaintEventArgs e)
      {
        Rectangle rec = e.ClipRectangle;

        rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 4;
        if (ProgressBarRenderer.IsSupported)
        {
          ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);
        }
        rec.Height = rec.Height - 4;
        e.Graphics.FillRectangle(m_brush, 2, 2, rec.Width, rec.Height);
        
        if (!string.IsNullOrEmpty(Text))
        {
          SizeF len = e.Graphics.MeasureString(Text, Font);
          Point location = 
            new Point(Convert.ToInt32((Width / 2) - len.Width / 2), 
              Convert.ToInt32((Height / 2) - len.Height / 2)
              );
          e.Graphics.DrawString(Text, Font, Brushes.Black, location);
        }
      }
    }
  }
}
