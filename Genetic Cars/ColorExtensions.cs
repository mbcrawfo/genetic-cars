using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace Genetic_Cars
{
  static class ColorExtensions
  {
    /// <summary>
    /// Sets the alpha of a color leaving RGB unmodified.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="a"></param>
    /// <returns></returns>
    public static Color SetAlpha(this Color c, byte a)
    {
      return new Color(c.R, c.G, c.B, a);
    }
  }
}
