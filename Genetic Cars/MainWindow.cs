using System;
using System.Windows.Forms;

namespace Genetic_Cars
{
  public partial class MainWindow : Form
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    /// <summary>
    /// The handle for the SFML drawing surface.
    /// </summary>
    public IntPtr DrawingSurfaceHandle
    {
      get { return drawingSurface.Handle; }
    }
  }
}
