using System;
using System.Windows.Forms;

namespace Genetic_Cars
{
  public partial class MainWindow : Form
  {
    private const float AspectRatio = 4f / 3f;

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
