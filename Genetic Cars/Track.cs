using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using log4net;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace Genetic_Cars
{
  /// <summary>
  /// Holds the track for the cars to race on.
  /// </summary>
  class Track
  {
    public readonly Category CollisionCategory = Category.Cat1;
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

      // The first piece is larger to provide a launch point and positioned 
      // so that the real track can start at 0,0
      var start = new Vector2f(-10, 0);
      var rot = 0f;
      var shape = new RectangleShape
      {
        FillColor = ShapeFillColor,
        OutlineColor = ShapeOutlineColor,
        OutlineThickness = ShapeOutlineSize,
        Position = start,
        Size = new Vector2f(10, ShapeSize.Y),
        Rotation = rot
      };
      var end = CalcEndPoint(start, shape.Size, rot);
      m_trackShapes.Add(shape);
      var body = CreateBody(start, end);
      m_trackBodies.Add(body);
      
      // place the rest of the pieces
      for (int i = 0; i < NumPieces; i++)
      {
        start = end;
        // the angle of the piece is randomized, with a 50% chance to be negative
        var maxAngle = CalcMaxAngle(i) + 10;
        var minAngle = CalcMinAngle(i) + 10;
        rot = (float)rand.NextDouble() * (maxAngle - minAngle) + minAngle;
        if (rand.NextDouble() < 0.5)
        {
          rot *= -1f;
        }
        shape = CreateShape(start, rot);
        end = CalcEndPoint(start, shape.Size, rot);
        body = CreateBody(start, end);

        m_trackShapes.Add(shape);
        m_trackBodies.Add(body);
      }

      Debug.Assert(m_trackShapes.Count == m_trackBodies.Count,
        "m_trackShapes.Count == m_trackBodies.Count");
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
    /// Creates a physics body for a piece of track.  The body is only an edge 
    /// that runs along the top of the track piece.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private Body CreateBody(Vector2f start, Vector2f end)
    {
      var body = BodyFactory.CreateEdge(
        m_world, start.ToVector2(), end.ToVector2()
        );
      body.BodyType = BodyType.Static;
      body.CollidesWith = Category.All;
      body.CollisionCategories = CollisionCategory;
      return body;
    }

    /// <summary>
    /// Calculates the end point (upper right corner) for a piece of track.  
    /// This is the point where the next piece of track will attach.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="size"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    private static Vector2f CalcEndPoint(Vector2f start, Vector2f size,
      float rotation)
    {
      return new Vector2f
      {
        X = start.X +
          (size.X * (float)Math.Cos(MathExtensions.DegToRad(rotation))),
        Y = start.Y +
          (size.X * (float)Math.Sin(MathExtensions.DegToRad(rotation)))
      };
    }

    /// <summary>
    /// Creates and returns a track piece shape.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    private static RectangleShape CreateShape(Vector2f position, float rotation)
    {
      return new RectangleShape
      {
        FillColor = ShapeFillColor,
        OutlineColor = ShapeOutlineColor,
        OutlineThickness = ShapeOutlineSize,
        Size = ShapeSize,
        // invert the position and rotation for SFML
        Position = position.InvertY(),
        Rotation = rotation * -1f
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
