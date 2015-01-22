using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Genetic_Cars
{
  partial class MainWindow
  {
    private DrawingSurface m_drawingSurface;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// The handle for the SFML drawing surface.
    /// </summary>
    public IntPtr DrawingSurfaceHandle
    {
      get
      {
        Debug.Assert(m_drawingSurface != null);
        return m_drawingSurface.Handle;
      }
    }

    protected override void OnCreateControl()
    {
      base.OnCreateControl();

      // SFML surface
      m_drawingSurface = new DrawingSurface();
      m_drawingSurface.Size = new Size(ClientSize.Width, 
        ClientSize.Height - 200);
      m_drawingSurface.Location = new Point(0, 0);
      Controls.Add(m_drawingSurface);

      this.Resize += (sender, args) => m_drawingSurface.Size = 
        new Size(ClientSize.Width, ClientSize.Height - 200);
    }

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
      this.SuspendLayout();
      // 
      // MainWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(784, 562);
      this.MinimumSize = new System.Drawing.Size(800, 600);
      this.Name = "MainWindow";
      this.Text = "Genetic Cars";
      this.ResumeLayout(false);

    }

    #endregion

    /// <summary>
    /// Custom control for SFML graphics that does not paint in the foreground 
    /// or background.
    /// </summary>
    private class DrawingSurface : Control
    {
      protected override void OnPaint(PaintEventArgs e)
      {
        //base.OnPaint(e);
      }

      protected override void OnPaintBackground(PaintEventArgs pevent)
      {
        //base.OnPaintBackground(pevent);
      }
    }
  }
}

