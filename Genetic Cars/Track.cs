using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Dynamics;
using SFML.Graphics;
using SFML.Window;

namespace Genetic_Cars
{
  /// <summary>
  /// Holds the track for the cars to race on.
  /// </summary>
  class Track
  {
    private const int NumPieces = 100;
    private const float MaxPieceAngle = 75;
    private const float PieceAngleShift = 5;

    // info for the SFML shapes that make up the track
    private static readonly Color ShapeFillColor = new Color(128, 128, 128);
    private static readonly Color ShapeOutlineColor = Color.Black;
    private const float ShapeOutlineSize = 0.03f;
    private static readonly Vector2f ShapeSize = new Vector2f(1, .25f);

    private World m_world;
    private Random m_rand;
    private readonly List<Body> m_trackBodies = new List<Body>();
    private readonly List<Shape> m_trackShapes = new List<Shape>();
    
    /// <summary>
    /// The physics world for the track.
    /// </summary>
    public World World
    {
      get { return m_world; }
      set
      {
        Debug.Assert(value != null);
        m_world = value;
      }
    }

    /// <summary>
    /// RNG used in track generation.
    /// </summary>
    public Random Rand
    {
      get { return m_rand; }
      set
      {
        Debug.Assert(value != null);
        m_rand = value;
      }
    }

    public void Generate()
    {
      Debug.Assert(m_world != null);
      Debug.Assert(m_rand != null);
      Debug.Assert(m_trackBodies.Count == 0);
      Debug.Assert(m_trackShapes.Count == 0);

      // the first piece is larger to provide a launch point and positioned 
      // so that the real track can start at 0,0
      var pos = new Vector2f(-10, 0);
      var rot = 0f;
      var shape = new RectangleShape
      {
        FillColor = ShapeFillColor,
        OutlineColor = ShapeOutlineColor,
        OutlineThickness = ShapeOutlineSize,
        Origin = new Vector2f(0, ShapeSize.Y / 2f),
        Position = pos,
        Size = new Vector2f(10, ShapeSize.Y),
        Rotation = rot
      };
      m_trackShapes.Add(shape);
      
      for (int i = 0; i < NumPieces; i++)
      {
        pos = new Vector2f
        {
          X = pos.X + 
            (shape.Size.X * (float)Math.Cos(MathExtensions.DegToRad(rot))),
          Y = pos.Y + 
            (shape.Size.X * (float)Math.Sin(MathExtensions.DegToRad(rot)))
        };

        var maxAngle = CalcMaxAngle(i) + 10;
        var minAngle = CalcMinAngle(i) + 10;
        rot = (float)Rand.NextDouble() * (maxAngle - minAngle) + minAngle;
        if (Rand.NextDouble() < 0.5)
        {
          rot *= -1f;
        }
        
        shape = CreateShape(pos, rot);
        m_trackShapes.Add(shape);
      }
    }

    /// <summary>
    /// Draws the track to the screen.
    /// </summary>
    /// <param name="target"></param>
    public void Draw(RenderTarget target)
    {
      foreach (var shape in m_trackShapes)
      {
        target.Draw(shape);
      }
    }

    /// <summary>
    /// Creates and returns a track piece shape.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    private static RectangleShape CreateShape(Vector2f position, float rotation)
    {
      // SFML uses an inverted Y axis
      position = new Vector2f
      {
        X = position.X, 
        Y = -position.Y
      };
      rotation *= -1f;

      return new RectangleShape
      {
        FillColor = ShapeFillColor,
        OutlineColor = ShapeOutlineColor,
        OutlineThickness = ShapeOutlineSize,
        Origin = new Vector2f(0, ShapeSize.Y / 2f),
        Size = ShapeSize,
        Position = position,
        Rotation = rotation
      };
    }

    /// <summary>
    /// Uses a cubic function to calculate the maximum angle for a piece of 
    /// track in the range 
    /// [<see cref="PieceAngleShift"/>, <see cref="MaxPieceAngle"/>].
    /// </summary>
    /// <param name="index">The index of the track piece with 
    /// <see cref="NumPieces"/> as the maximum.
    /// </param>
    /// <returns></returns>
    private static float CalcMaxAngle(int index)
    {
      Debug.Assert(index < NumPieces);

      // uses y=x^3 in the range x=[0,10]
      // scale x to [0..10] based on its location in the track
      float x = ((index + 1f) / NumPieces) * 10f;
      float y = x * x * x;
      float percent = y / 1000f;
      return (percent * (MaxPieceAngle - PieceAngleShift)) + PieceAngleShift;
    }

    /// <summary>
    /// Uses a cubic function to calculate the maximum angle for a piece of 
    /// track in the range 
    /// [<see cref="PieceAngleShift"/>, <see cref="MaxPieceAngle"/>].  Will 
    /// always return a value less than <see cref="CalcMaxAngle"/> for the 
    /// same index.
    /// </summary>
    /// <param name="index">The index of the track piece with 
    /// <see cref="NumPieces"/> as the maximum.
    /// </param>
    /// <returns></returns>
    private static float CalcMinAngle(int index)
    {
      Debug.Assert(index < NumPieces);

      // uses y=x^3-x^2.7 in the range x=[0,10]
      // scale x to [0..10] based on its location in the track
      float x = ((index + 1f) / NumPieces) * 10f;
      float y = (x * x * x) - (float)Math.Pow(x, 2.7);
      float percent = y / 1000f;
      return (percent * (MaxPieceAngle - PieceAngleShift)) + PieceAngleShift;
    }
  }
}
