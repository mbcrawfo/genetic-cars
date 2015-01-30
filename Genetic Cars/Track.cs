using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FarseerPhysics.Dynamics;
using log4net;
using SFML.Graphics;
using SFML.Window;

namespace Genetic_Cars
{
  /// <summary>
  /// Holds the track for the cars to race on.
  /// </summary>
  class Track
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    private const int NumPieces = 100;
    private const float MaxPieceAngle = 75;
    private const float PieceAngleShift = 5;

    // info for the SFML shapes that make up the track
    private static readonly Color ShapeFillColor = new Color(128, 128, 128);
    private static readonly Color ShapeOutlineColor = Color.Black;
    private const float ShapeOutlineSize = 0.03f;
    private static readonly Vector2f ShapeSize = new Vector2f(1, .25f);

    private readonly World m_world;
    private readonly List<Body> m_trackBodies = new List<Body>();
    private readonly List<Shape> m_trackShapes = new List<Shape>();

    /// <summary>
    /// Initializes, but does not generate the track.
    /// </summary>
    /// <param name="world">The physics world the track lives in.</param>
    public Track(World world)
    {
      if (world == null)
      {
        throw new ArgumentNullException("world");
      }
      m_world = world;
    }
    
    /// <summary>
    /// Randomly generates a new track.
    /// </summary>
    /// <param name="rand">The RNG used to generate the track.</param>
    public void Generate(Random rand)
    {
      if (rand == null)
      {
        throw new ArgumentNullException("rand");
      }

      Clear();
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      // Track pieces have their origin placed halfway up the left side of the 
      // piece, with the next piece connected at the point halfway up the right 
      // side of the piece to form a chain:
      // |------------------------------------|
      // @                                    @
      // |------------------------------------|
      // The first piece is larger to provide a launch point and positioned 
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
      
      // place the rest of the pieces
      for (int i = 0; i < NumPieces; i++)
      {
        // the piece of the new position is offset from the previous piece's 
        // position
        pos = new Vector2f
        {
          X = pos.X + 
            (shape.Size.X * (float)Math.Cos(MathExtensions.DegToRad(rot))),
          Y = pos.Y + 
            (shape.Size.X * (float)Math.Sin(MathExtensions.DegToRad(rot)))
        };

        // the angle of the piece is randomized, with a 50% chance to be negative
        var maxAngle = CalcMaxAngle(i) + 10;
        var minAngle = CalcMinAngle(i) + 10;
        rot = (float)rand.NextDouble() * (maxAngle - minAngle) + minAngle;
        if (rand.NextDouble() < 0.5)
        {
          rot *= -1f;
        }
        
        shape = CreateShape(pos, rot);
        m_trackShapes.Add(shape);
        //Log.DebugFormat("Added piece at {0}, rotation {1} degrees", pos, rot);
      }

      // TODO: uncomment me
      //Debug.Assert(m_trackShapes.Count == m_trackBodies.Count,
      //  "m_trackShapes.Count == m_trackBodies.Count");
      Log.DebugFormat("Generated {0} track pieces in {1} ms", 
        m_trackShapes.Count, stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Clears the track allowing a new track to be generated.
    /// </summary>
    public void Clear()
    {
      Log.DebugFormat("Clearing {0} track pieces", m_trackShapes.Count);

      foreach (var body in m_trackBodies)
      {
        m_world.RemoveBody(body);
      }

      m_trackBodies.Clear();
      m_trackShapes.Clear();
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
