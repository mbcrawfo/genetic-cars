using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
  sealed class Track : IDisposable
  {
    public static readonly Category CollisionCategory = Category.Cat1;
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    private const int NumPieces = 100;
    private const float MaxPieceAngle = 60;
    private const float PieceAngleShift = 5;

    // info for the SFML shapes that make up the track
    private static readonly Color ShapeFillColor = new Color(128, 128, 128);
    private static readonly Color ShapeOutlineColor = Color.Black;
    private const float ShapeOutlineSize = 0.03f;
    private static readonly Vector2f ShapeSize = new Vector2f(3, .25f);

    private bool m_disposed = false;
    private bool m_generated = false;

    private readonly World m_world;
    private readonly List<Body> m_trackBodies = new List<Body>(NumPieces);
    private readonly List<Shape> m_trackShapes = new List<Shape>(NumPieces);

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

     ~Track()
    {
      Dispose(false);
    }

    /// <summary>
    /// The X position of the world where cars should start.
    /// </summary>
    public float StartingLine { get; private set; }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposeManaged)
    {
      if (m_disposed || !m_generated)
      {
        return;
      }

      if (disposeManaged)
      {
        foreach (var shape in m_trackShapes)
        {
          shape.Dispose();
        }
      }

      foreach (var body in m_trackBodies)
      {
        m_world.RemoveBody(body);
      }

      m_disposed = true;
    }
    
    /// <summary>
    /// Randomly generates a new track, replacing an existing track.
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

      // first an invisible physics piece to block cars from running off the 
      // track to the left
      var start = new Vector2f(-10, 0);
      var body = BodyFactory.CreateEdge(
        m_world, start.ToVector2(), start.ToVector2() + new Vector2(0, 10));
      body.CollidesWith = Category.All;
      body.CollisionCategories = CollisionCategory;
      m_trackBodies.Add(body);
      
      // the first visible piece is the starting pad and is positioned so that 
      // the random track starts at (0,0)
      var rot = 0f;
      var shape = new RectangleShape
      {
        FillColor = ShapeFillColor,
        OutlineColor = ShapeOutlineColor,
        OutlineThickness = ShapeOutlineSize,
        Position = start.InvertY(),
        Size = new Vector2f(10, ShapeSize.Y),
        Rotation = rot
      };
      var end = CalcEndPoint(start, shape.Size, rot);
      m_trackShapes.Add(shape);
      body = CreateBody(start, end);
      m_trackBodies.Add(body);
      StartingLine = (start / 2f).X;
      
      // place the rest of the pieces
      for (int i = 0; i < NumPieces; i++)
      {
        start = end;
        // the angle of the piece is randomized
        var maxAngle = CalcMaxAngle(i);
        var minAngle = CalcMinAngle(i);
        var rotSign = rot < 0 ? -1f : 1f;
        rot = (float)rand.NextDouble() * (maxAngle - minAngle) + minAngle;
        // with a 40% chance to flip the sign from the last piece
        if (rand.NextDouble() < 0.4)
        {
          rot *= -rotSign;
        }
        else
        {
          rot *= rotSign;
        }

        shape = CreateShape(start, rot);
        end = CalcEndPoint(start, shape.Size, rot);
        body = CreateBody(start, end);

        m_trackShapes.Add(shape);
        m_trackBodies.Add(body);
      }

      m_generated = true;
      Log.DebugFormat("Generated {0} track pieces in {1} ms", 
        m_trackShapes.Count, stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Clears the track allowing a new track to be generated.
    /// </summary>
    public void Clear()
    {
      if (!m_generated)
      {
        return;
      }
      Log.DebugFormat("Clearing {0} track pieces", m_trackShapes.Count);

      foreach (var body in m_trackBodies)
      {
        m_world.RemoveBody(body);
      }
      foreach (var shape in m_trackShapes)
      {
        shape.Dispose();
      }

      m_trackBodies.Clear();
      m_trackShapes.Clear();
      m_generated = false;
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
      body.Friction = 1;
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
