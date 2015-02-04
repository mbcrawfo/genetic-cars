using SFML.Graphics;

namespace Genetic_Cars
{
  /// <summary>
  /// Abstracts objects that will be drawn to the screen.
  /// </summary>
  interface IDrawable
  {
    /// <summary>
    /// Draws the object onto the RenderTarget.
    /// </summary>
    /// <param name="target"></param>
    void Draw(RenderTarget target);
  }
}
