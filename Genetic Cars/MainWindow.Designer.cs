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
      this.drawingSurface = new System.Windows.Forms.Panel();
      this.settingsBox = new System.Windows.Forms.GroupBox();
      this.mutationRateApplyButton = new System.Windows.Forms.Button();
      this.mutationRateTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.newPopulationButton = new System.Windows.Forms.Button();
      this.rapidSimButton = new System.Windows.Forms.Button();
      this.pauseButton = new System.Windows.Forms.Button();
      this.seedApplyButton = new System.Windows.Forms.Button();
      this.seedTextBox = new System.Windows.Forms.TextBox();
      this.seedLabel = new System.Windows.Forms.Label();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.mainLayout.SuspendLayout();
      this.settingsBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // mainLayout
      // 
      this.mainLayout.ColumnCount = 3;
      this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.mainLayout.Controls.Add(this.drawingSurface, 0, 0);
      this.mainLayout.Controls.Add(this.settingsBox, 0, 1);
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
      // drawingSurface
      // 
      this.drawingSurface.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mainLayout.SetColumnSpan(this.drawingSurface, 3);
      this.drawingSurface.Location = new System.Drawing.Point(0, 0);
      this.drawingSurface.Margin = new System.Windows.Forms.Padding(0);
      this.drawingSurface.Name = "drawingSurface";
      this.drawingSurface.Size = new System.Drawing.Size(1008, 504);
      this.drawingSurface.TabIndex = 0;
      // 
      // settingsBox
      // 
      this.settingsBox.Controls.Add(this.mutationRateApplyButton);
      this.settingsBox.Controls.Add(this.mutationRateTextBox);
      this.settingsBox.Controls.Add(this.label1);
      this.settingsBox.Controls.Add(this.newPopulationButton);
      this.settingsBox.Controls.Add(this.rapidSimButton);
      this.settingsBox.Controls.Add(this.pauseButton);
      this.settingsBox.Controls.Add(this.seedApplyButton);
      this.settingsBox.Controls.Add(this.seedTextBox);
      this.settingsBox.Controls.Add(this.seedLabel);
      this.settingsBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.settingsBox.Location = new System.Drawing.Point(3, 507);
      this.settingsBox.Name = "settingsBox";
      this.settingsBox.Size = new System.Drawing.Size(330, 220);
      this.settingsBox.TabIndex = 1;
      this.settingsBox.TabStop = false;
      this.settingsBox.Text = "Settings";
      // 
      // mutationRateApplyButton
      // 
      this.mutationRateApplyButton.Location = new System.Drawing.Point(241, 94);
      this.mutationRateApplyButton.Name = "mutationRateApplyButton";
      this.mutationRateApplyButton.Size = new System.Drawing.Size(75, 23);
      this.mutationRateApplyButton.TabIndex = 8;
      this.mutationRateApplyButton.Text = "Apply";
      this.mutationRateApplyButton.UseVisualStyleBackColor = true;
      this.mutationRateApplyButton.Click += new System.EventHandler(this.mutationRateApplyButton_Click);
      // 
      // mutationRateTextBox
      // 
      this.mutationRateTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.mutationRateTextBox.Location = new System.Drawing.Point(129, 91);
      this.mutationRateTextBox.Name = "mutationRateTextBox";
      this.mutationRateTextBox.Size = new System.Drawing.Size(106, 26);
      this.mutationRateTextBox.TabIndex = 7;
      this.mutationRateTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.toolTip.SetToolTip(this.mutationRateTextBox, "Set the mutation rate for the genetic algorithm.\r\nMust be between 0 and 1.");
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(9, 94);
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
      // rapidSimButton
      // 
      this.rapidSimButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
      this.rapidSimButton.AutoSize = true;
      this.rapidSimButton.Location = new System.Drawing.Point(132, 19);
      this.rapidSimButton.Name = "rapidSimButton";
      this.rapidSimButton.Size = new System.Drawing.Size(85, 23);
      this.rapidSimButton.TabIndex = 4;
      this.rapidSimButton.Text = "Rapid Sim: Off";
      this.toolTip.SetToolTip(this.rapidSimButton, "Toggle a rapid simulation mode that disables\r\ndrawing and simulates only physics " +
        "to reach a\r\nsolution faster.");
      this.rapidSimButton.UseVisualStyleBackColor = true;
      this.rapidSimButton.Click += new System.EventHandler(this.rapidSimButton_Click);
      // 
      // pauseButton
      // 
      this.pauseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.pauseButton.Location = new System.Drawing.Point(241, 19);
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
      this.seedApplyButton.Location = new System.Drawing.Point(241, 62);
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
      this.seedTextBox.Location = new System.Drawing.Point(66, 62);
      this.seedTextBox.Name = "seedTextBox";
      this.seedTextBox.Size = new System.Drawing.Size(169, 23);
      this.seedTextBox.TabIndex = 1;
      this.toolTip.SetToolTip(this.seedTextBox, resources.GetString("seedTextBox.ToolTip"));
      // 
      // seedLabel
      // 
      this.seedLabel.AutoSize = true;
      this.seedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.seedLabel.Location = new System.Drawing.Point(9, 62);
      this.seedLabel.Name = "seedLabel";
      this.seedLabel.Size = new System.Drawing.Size(51, 20);
      this.seedLabel.TabIndex = 0;
      this.seedLabel.Text = "Seed:";
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
      this.ResumeLayout(false);

    }

    #endregion

    private TableLayoutPanel mainLayout;
    private Panel drawingSurface;
    private GroupBox settingsBox;
    private Button seedApplyButton;
    private TextBox seedTextBox;
    private Label seedLabel;
    private Button pauseButton;
    private Button newPopulationButton;
    private Button rapidSimButton;
    private ToolTip toolTip;
    private Button mutationRateApplyButton;
    private TextBox mutationRateTextBox;
    private Label label1;


  }
}

