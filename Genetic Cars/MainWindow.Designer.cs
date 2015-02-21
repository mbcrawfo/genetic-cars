using System.ComponentModel;
using System.Windows.Forms;

namespace Genetic_Cars
{
  partial class MainWindow
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;
    
    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; 
    /// otherwise, false.
    /// </param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
      this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
      this.drawingPanel = new System.Windows.Forms.Panel();
      this.settingsBox = new System.Windows.Forms.GroupBox();
      this.clonesComboBox = new System.Windows.Forms.ComboBox();
      this.clonesLabel = new System.Windows.Forms.Label();
      this.mutationRateApplyButton = new System.Windows.Forms.Button();
      this.mutationRateTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.newPopulationButton = new System.Windows.Forms.Button();
      this.graphicsButton = new System.Windows.Forms.Button();
      this.pauseButton = new System.Windows.Forms.Button();
      this.seedApplyButton = new System.Windows.Forms.Button();
      this.seedTextBox = new System.Windows.Forms.TextBox();
      this.seedLabel = new System.Windows.Forms.Label();
      this.historyBox = new System.Windows.Forms.GroupBox();
      this.highScoreListBox = new System.Windows.Forms.ListBox();
      this.populationBox = new System.Windows.Forms.GroupBox();
      this.distanceLabel = new System.Windows.Forms.Label();
      this.liveCountLabel = new System.Windows.Forms.Label();
      this.generationLabel = new System.Windows.Forms.Label();
      this.overviewBox = new System.Windows.Forms.GroupBox();
      this.overviewPanel = new System.Windows.Forms.Panel();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.populationList = new System.Windows.Forms.FlowLayoutPanel();
      this.mainLayout.SuspendLayout();
      this.settingsBox.SuspendLayout();
      this.historyBox.SuspendLayout();
      this.populationBox.SuspendLayout();
      this.overviewBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // mainLayout
      // 
      this.mainLayout.ColumnCount = 4;
      this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 307F));
      this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 169F));
      this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 156F));
      this.mainLayout.Controls.Add(this.drawingPanel, 0, 0);
      this.mainLayout.Controls.Add(this.settingsBox, 0, 1);
      this.mainLayout.Controls.Add(this.historyBox, 1, 1);
      this.mainLayout.Controls.Add(this.populationBox, 3, 0);
      this.mainLayout.Controls.Add(this.overviewBox, 2, 1);
      this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.mainLayout.Location = new System.Drawing.Point(0, 0);
      this.mainLayout.Margin = new System.Windows.Forms.Padding(0);
      this.mainLayout.Name = "mainLayout";
      this.mainLayout.RowCount = 2;
      this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 69.05405F));
      this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30.94595F));
      this.mainLayout.Size = new System.Drawing.Size(1008, 730);
      this.mainLayout.TabIndex = 0;
      // 
      // drawingPanel
      // 
      this.mainLayout.SetColumnSpan(this.drawingPanel, 3);
      this.drawingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.drawingPanel.Location = new System.Drawing.Point(0, 0);
      this.drawingPanel.Margin = new System.Windows.Forms.Padding(0);
      this.drawingPanel.Name = "drawingPanel";
      this.drawingPanel.Size = new System.Drawing.Size(852, 504);
      this.drawingPanel.TabIndex = 0;
      // 
      // settingsBox
      // 
      this.settingsBox.Controls.Add(this.clonesComboBox);
      this.settingsBox.Controls.Add(this.clonesLabel);
      this.settingsBox.Controls.Add(this.mutationRateApplyButton);
      this.settingsBox.Controls.Add(this.mutationRateTextBox);
      this.settingsBox.Controls.Add(this.label1);
      this.settingsBox.Controls.Add(this.newPopulationButton);
      this.settingsBox.Controls.Add(this.graphicsButton);
      this.settingsBox.Controls.Add(this.pauseButton);
      this.settingsBox.Controls.Add(this.seedApplyButton);
      this.settingsBox.Controls.Add(this.seedTextBox);
      this.settingsBox.Controls.Add(this.seedLabel);
      this.settingsBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.settingsBox.Location = new System.Drawing.Point(3, 507);
      this.settingsBox.Name = "settingsBox";
      this.settingsBox.Size = new System.Drawing.Size(301, 220);
      this.settingsBox.TabIndex = 1;
      this.settingsBox.TabStop = false;
      this.settingsBox.Text = "Settings";
      // 
      // clonesComboBox
      // 
      this.clonesComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.clonesComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.clonesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.clonesComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.clonesComboBox.FormattingEnabled = true;
      this.clonesComboBox.Location = new System.Drawing.Point(155, 109);
      this.clonesComboBox.Name = "clonesComboBox";
      this.clonesComboBox.Size = new System.Drawing.Size(50, 24);
      this.clonesComboBox.TabIndex = 10;
      this.clonesComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.clonesComboBox_DrawItem);
      this.clonesComboBox.SelectedIndexChanged += new System.EventHandler(this.clonesComboBox_SelectedIndexChanged);
      // 
      // clonesLabel
      // 
      this.clonesLabel.AutoSize = true;
      this.clonesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.clonesLabel.Location = new System.Drawing.Point(9, 110);
      this.clonesLabel.Name = "clonesLabel";
      this.clonesLabel.Size = new System.Drawing.Size(140, 20);
      this.clonesLabel.TabIndex = 9;
      this.clonesLabel.Text = "Number of Clones:";
      // 
      // mutationRateApplyButton
      // 
      this.mutationRateApplyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mutationRateApplyButton.Location = new System.Drawing.Point(211, 80);
      this.mutationRateApplyButton.Name = "mutationRateApplyButton";
      this.mutationRateApplyButton.Size = new System.Drawing.Size(75, 23);
      this.mutationRateApplyButton.TabIndex = 8;
      this.mutationRateApplyButton.Text = "Apply";
      this.mutationRateApplyButton.UseVisualStyleBackColor = true;
      this.mutationRateApplyButton.Click += new System.EventHandler(this.mutationRateApplyButton_Click);
      // 
      // mutationRateTextBox
      // 
      this.mutationRateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mutationRateTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.mutationRateTextBox.Location = new System.Drawing.Point(128, 77);
      this.mutationRateTextBox.Name = "mutationRateTextBox";
      this.mutationRateTextBox.Size = new System.Drawing.Size(77, 26);
      this.mutationRateTextBox.TabIndex = 7;
      this.mutationRateTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.toolTip.SetToolTip(this.mutationRateTextBox, "Set the mutation rate for the genetic algorithm.\r\nMust be between 0 and 1.");
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(8, 80);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(114, 20);
      this.label1.TabIndex = 6;
      this.label1.Text = "Mutation Rate:";
      // 
      // newPopulationButton
      // 
      this.newPopulationButton.AutoSize = true;
      this.newPopulationButton.Location = new System.Drawing.Point(13, 19);
      this.newPopulationButton.Name = "newPopulationButton";
      this.newPopulationButton.Size = new System.Drawing.Size(92, 23);
      this.newPopulationButton.TabIndex = 5;
      this.newPopulationButton.Text = "New Population";
      this.toolTip.SetToolTip(this.newPopulationButton, "Generates a new random population, but\r\nkeeps the same track.");
      this.newPopulationButton.UseVisualStyleBackColor = true;
      this.newPopulationButton.Click += new System.EventHandler(this.newPopulationButton_Click);
      // 
      // graphicsButton
      // 
      this.graphicsButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
      this.graphicsButton.AutoSize = true;
      this.graphicsButton.Location = new System.Drawing.Point(117, 19);
      this.graphicsButton.Name = "graphicsButton";
      this.graphicsButton.Size = new System.Drawing.Size(85, 23);
      this.graphicsButton.TabIndex = 4;
      this.graphicsButton.Text = "Graphics: On";
      this.toolTip.SetToolTip(this.graphicsButton, "Toggle a rapid simulation mode that disables\r\ndrawing and simulates only physics " +
        "to reach a\r\nsolution faster.");
      this.graphicsButton.UseVisualStyleBackColor = true;
      this.graphicsButton.Click += new System.EventHandler(this.graphicsButton_Click);
      // 
      // pauseButton
      // 
      this.pauseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.pauseButton.Location = new System.Drawing.Point(212, 19);
      this.pauseButton.Name = "pauseButton";
      this.pauseButton.Size = new System.Drawing.Size(75, 23);
      this.pauseButton.TabIndex = 3;
      this.pauseButton.Text = "Pause";
      this.toolTip.SetToolTip(this.pauseButton, "Pause or resume the simulation.");
      this.pauseButton.UseVisualStyleBackColor = true;
      this.pauseButton.Click += new System.EventHandler(this.pauseButton_Click);
      // 
      // seedApplyButton
      // 
      this.seedApplyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.seedApplyButton.AutoSize = true;
      this.seedApplyButton.Location = new System.Drawing.Point(211, 48);
      this.seedApplyButton.Name = "seedApplyButton";
      this.seedApplyButton.Size = new System.Drawing.Size(75, 23);
      this.seedApplyButton.TabIndex = 2;
      this.seedApplyButton.Text = "Apply";
      this.seedApplyButton.UseVisualStyleBackColor = true;
      this.seedApplyButton.Click += new System.EventHandler(this.seedApplyButton_Click);
      // 
      // seedTextBox
      // 
      this.seedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.seedTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.seedTextBox.Location = new System.Drawing.Point(65, 48);
      this.seedTextBox.Name = "seedTextBox";
      this.seedTextBox.Size = new System.Drawing.Size(140, 23);
      this.seedTextBox.TabIndex = 1;
      this.toolTip.SetToolTip(this.seedTextBox, resources.GetString("seedTextBox.ToolTip"));
      // 
      // seedLabel
      // 
      this.seedLabel.AutoSize = true;
      this.seedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.seedLabel.Location = new System.Drawing.Point(8, 48);
      this.seedLabel.Name = "seedLabel";
      this.seedLabel.Size = new System.Drawing.Size(51, 20);
      this.seedLabel.TabIndex = 0;
      this.seedLabel.Text = "Seed:";
      // 
      // historyBox
      // 
      this.historyBox.Controls.Add(this.highScoreListBox);
      this.historyBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.historyBox.Location = new System.Drawing.Point(310, 507);
      this.historyBox.Name = "historyBox";
      this.historyBox.Size = new System.Drawing.Size(163, 220);
      this.historyBox.TabIndex = 2;
      this.historyBox.TabStop = false;
      this.historyBox.Text = "High Scores";
      // 
      // highScoreListBox
      // 
      this.highScoreListBox.BackColor = System.Drawing.SystemColors.Control;
      this.highScoreListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.highScoreListBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.highScoreListBox.FormattingEnabled = true;
      this.highScoreListBox.Location = new System.Drawing.Point(3, 16);
      this.highScoreListBox.Name = "highScoreListBox";
      this.highScoreListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
      this.highScoreListBox.Size = new System.Drawing.Size(157, 201);
      this.highScoreListBox.TabIndex = 0;
      // 
      // populationBox
      // 
      this.populationBox.Controls.Add(this.populationList);
      this.populationBox.Controls.Add(this.distanceLabel);
      this.populationBox.Controls.Add(this.liveCountLabel);
      this.populationBox.Controls.Add(this.generationLabel);
      this.populationBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.populationBox.Location = new System.Drawing.Point(855, 3);
      this.populationBox.Name = "populationBox";
      this.populationBox.Size = new System.Drawing.Size(150, 498);
      this.populationBox.TabIndex = 3;
      this.populationBox.TabStop = false;
      this.populationBox.Text = "Population";
      // 
      // distanceLabel
      // 
      this.distanceLabel.AutoSize = true;
      this.distanceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.distanceLabel.Location = new System.Drawing.Point(6, 50);
      this.distanceLabel.Name = "distanceLabel";
      this.distanceLabel.Size = new System.Drawing.Size(114, 17);
      this.distanceLabel.TabIndex = 2;
      this.distanceLabel.Text = "Distance: 2.00 m";
      // 
      // liveCountLabel
      // 
      this.liveCountLabel.AutoSize = true;
      this.liveCountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.liveCountLabel.Location = new System.Drawing.Point(6, 33);
      this.liveCountLabel.Name = "liveCountLabel";
      this.liveCountLabel.Size = new System.Drawing.Size(83, 17);
      this.liveCountLabel.TabIndex = 1;
      this.liveCountLabel.Text = "Live Cars: 0";
      // 
      // generationLabel
      // 
      this.generationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.generationLabel.AutoSize = true;
      this.generationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.generationLabel.Location = new System.Drawing.Point(6, 16);
      this.generationLabel.Name = "generationLabel";
      this.generationLabel.Size = new System.Drawing.Size(95, 17);
      this.generationLabel.TabIndex = 0;
      this.generationLabel.Text = "Generation: 1";
      // 
      // overviewBox
      // 
      this.mainLayout.SetColumnSpan(this.overviewBox, 2);
      this.overviewBox.Controls.Add(this.overviewPanel);
      this.overviewBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.overviewBox.Location = new System.Drawing.Point(479, 507);
      this.overviewBox.Name = "overviewBox";
      this.overviewBox.Padding = new System.Windows.Forms.Padding(5, 0, 5, 5);
      this.overviewBox.Size = new System.Drawing.Size(526, 220);
      this.overviewBox.TabIndex = 4;
      this.overviewBox.TabStop = false;
      this.overviewBox.Text = "Track Overview";
      // 
      // overviewPanel
      // 
      this.overviewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.overviewPanel.Location = new System.Drawing.Point(5, 13);
      this.overviewPanel.Name = "overviewPanel";
      this.overviewPanel.Size = new System.Drawing.Size(516, 202);
      this.overviewPanel.TabIndex = 0;
      // 
      // populationList
      // 
      this.populationList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.populationList.AutoScroll = true;
      this.populationList.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
      this.populationList.Location = new System.Drawing.Point(6, 70);
      this.populationList.Name = "populationList";
      this.populationList.Size = new System.Drawing.Size(138, 422);
      this.populationList.TabIndex = 3;
      this.populationList.WrapContents = false;
      // 
      // MainWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1008, 730);
      this.Controls.Add(this.mainLayout);
      this.MinimumSize = new System.Drawing.Size(1024, 726);
      this.Name = "MainWindow";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Genetic Cars";
      this.mainLayout.ResumeLayout(false);
      this.settingsBox.ResumeLayout(false);
      this.settingsBox.PerformLayout();
      this.historyBox.ResumeLayout(false);
      this.populationBox.ResumeLayout(false);
      this.populationBox.PerformLayout();
      this.overviewBox.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private TableLayoutPanel mainLayout;
    private Panel drawingPanel;
    private GroupBox settingsBox;
    private Button seedApplyButton;
    private TextBox seedTextBox;
    private Label seedLabel;
    private Button pauseButton;
    private Button newPopulationButton;
    private Button graphicsButton;
    private ToolTip toolTip;
    private Button mutationRateApplyButton;
    private TextBox mutationRateTextBox;
    private Label label1;
    private ComboBox clonesComboBox;
    private Label clonesLabel;
    private GroupBox historyBox;
    private Label generationLabel;
    private GroupBox populationBox;
    private Label liveCountLabel;
    private Label distanceLabel;
    private GroupBox overviewBox;
    private Panel overviewPanel;
    private ListBox highScoreListBox;
    private FlowLayoutPanel populationList;


  }
}

