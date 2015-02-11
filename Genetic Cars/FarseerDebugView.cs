using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using Transform = FarseerPhysics.Common.Transform;

namespace Genetic_Cars
{
  /// <summary>
  /// Draws physics components for debugging purposes.
  /// </summary>
  /// <remarks>
  /// Finishing this is going to take a lot more effort than I thought because 
  /// Farseer doesn't use the simple model that Box2d does.. to be finished 
  /// if really needed.
  /// </remarks>
  class FarseerDebugView : DebugViewBase
  {
    private const float OutlineThickness = 0.1f;

    private readonly RenderTarget m_renderTarget;

    public FarseerDebugView(World world, RenderTarget renderTarget) 
      : base(world)
    {
      m_renderTarget = renderTarget;
    }

    public override void DrawPolygon(Vector2[] vertices, int count, float red, 
      float blue, float green, bool closed = true)
    {
      var color = RGBtoColor(red, blue, green);
      
      VertexArray vertexArray = new VertexArray(PrimitiveType.LinesStrip);

      for (int i = 0; i < count; i++)
      {
        vertexArray.Append(new Vertex
        {
          Color = color,
          Position = vertices[i].ToVector2f().InvertY()
        });
      }

      m_renderTarget.Draw(vertexArray);
    }

    public override void DrawSolidPolygon(Vector2[] vertices, int count, 
      float red, float blue, float green)
    {
      var shape = new ConvexShape
      {
        FillColor = RGBtoColor(red, blue, green),
      };

      for (int i = 0; i < count; i++)
      {
        shape.SetPoint((uint)i, vertices[i].ToVector2f().InvertY());
      }

      m_renderTarget.Draw(shape);
    }

    public override void DrawCircle(Vector2 center, float radius, float red, 
      float blue, float green)
    {
      var shape = new CircleShape
      {
        FillColor = Color.Transparent,
        OutlineColor = RGBtoColor(red, blue, green),
        OutlineThickness = OutlineThickness,
        Position = center.ToVector2f().InvertY(),
        Radius = radius
      };
      m_renderTarget.Draw(shape);
    }

    public override void DrawSolidCircle(Vector2 center, float radius, 
      Vector2 axis, float red, float blue, float green)
    {
      var shape = new CircleShape
      {
        FillColor = RGBtoColor(red, blue, green),
        Position = center.ToVector2f().InvertY(),
        Radius = radius
      };
      m_renderTarget.Draw(shape);
      // TODO: verify if this is correct
      DrawSegment(center, (center + axis) * radius, 0f, 0f, 0f);
    }

    public override void DrawSegment(Vector2 start, Vector2 end, float red, 
      float blue, float green)
    {
      VertexArray vertexArray = new VertexArray();
      vertexArray.Append(new Vertex
      {
        Position = start.ToVector2f().InvertY(),
        Color = RGBtoColor(red, blue, green)
      });
      vertexArray.Append(new Vertex
      {
        Position = end.ToVector2f().InvertY(),
        Color = RGBtoColor(red, blue, green)
      });
      vertexArray.PrimitiveType = PrimitiveType.Lines;
      m_renderTarget.Draw(vertexArray);
    }

    public override void DrawTransform(ref Transform transform)
    {
      // TODO: implement
    }

    /// <summary>
    /// Converts Farseer float color values to a SFML Color struct.
    /// </summary>
    /// <param name="red"></param>
    /// <param name="blue"></param>
    /// <param name="green"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    private static Color RGBtoColor(float red, float blue, float green)
    {
      byte r = (byte) (red * 255);
      byte b = (byte) (blue * 255);
      byte g = (byte) (green * 255);
      return new Color(r, g, b);
    }
  }
}
