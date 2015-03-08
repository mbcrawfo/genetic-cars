using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Genetic_Cars.Properties;
using log4net;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;
// ReSharper disable RedundantDefaultMemberInitializer

namespace Genetic_Cars
{
  /// <summary>
  /// Holds the track for the cars to race on.
  /// </summary>
  sealed class Track : IDisposable
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    // track properties
    private static readonly int NumPieces = 
      Settings.Default.NumTrackPieces;
    private static readonly float MaxPieceAngle = 
      Settings.Default.MaxTrackAngle;
    private static readonly float MinPieceAngle = 
      Settings.Default.MinTrackAngle;

    // graphical properties of the track
    private static readonly Color FillColor = new Color(128, 128, 128);
    private static readonly Color OutlineColor = Color.Black;
    private const float OutlineSize = 0.03f;
    private static readonly Vector2f PieceSize = new Vector2f(3, .25f);

    /// <summary>
    /// The collision category for all of the track components.
    /// </summary>
    public static readonly Category CollisionCategory = Category.Cat1;

    /// <summary>
    /// The collision category for the track's finish line sensor.
    /// </summary>
    public static readonly Category FinishLineCategory = Category.Cat3;
    
    public delegate void FinishLineCrossedHandler(int id);

    /// <summary>
    /// The RNG used for all track generation.  Must be set before any tracks 
    /// are created.
    /// </summary>
    public static Random Random { get; set; }

    private bool m_disposed = false;
    private bool m_generated = false;

    private readonly World m_world;
    private readonly List<Body> m_trackBodies = new List<Body>(NumPieces);
    private readonly List<Shape> m_trackShapes = new List<Shape>(NumPieces);
    private readonly VertexArray m_trackOutline = 
      new VertexArray(PrimitiveType.LinesStrip);
    private float m_startingLine;
    private bool m_finishLineCrossed = false;

    /// <summary>
    /// Initializes, but does not generate the track.
    /// </summary>
    /// <param name="physics"></param>
    public Track(PhysicsManager physics)
    {
      if (physics == null)
      {
        throw new ArgumentNullException("physics");
      }
      m_world = physics.World;
    }

     ~Track()
    {
      Dispose(false);
    }

    /// <summary>
    /// Signals that a car has crossed the finish line of the track.
    /// </summary>
    public event FinishLineCrossedHandler FinishLineCrossed;

    /// <summary>
    /// The X position of the world where cars should start.
    /// </summary>
    public float StartingLine
    {
      get
      {
        Debug.Assert(m_generated);
        return m_startingLine;
      }
    }

    /// <summary>
    /// The width and height of the track.
    /// </summary>
    public Vector2 Dimensions { get; private set; }

    /// <summary>
    /// The center point of the track.
    /// </summary>
    public Vector2 Center { get; private set; }

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

        m_trackOutline.Dispose();
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
    public void Generate()
    {
      Debug.Assert(Random != null);

      Clear();
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      float maxX = 0;
      float minX = 0;
      float maxY = 0;
      float minY = 0;

      // first an invisible physics piece to block cars from running off the 
      // track to the left
      var start = new Vector2f(-10, 0);
      minX = start.X;
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
        FillColor = FillColor,
        OutlineColor = OutlineColor,
        OutlineThickness = OutlineSize,
        Position = start.InvertY(),
        Size = new Vector2f(10, PieceSize.Y),
        Rotation = rot
      };
      var end = CalcEndPoint(start, shape.Size, rot);
      m_trackShapes.Add(shape);
      body = CreateBody(start, end);
      m_trackBodies.Add(body);
      m_startingLine = (start / 2f).X;
      m_trackOutline.Append(new Vertex(start.InvertY(), FillColor));
      m_trackOutline.Append(new Vertex(end.InvertY(), FillColor));
      
      // place the rest of the pieces
      for (int i = 0; i < NumPieces; i++)
      {
        start = end;
        // the angle of the piece is randomized
        var maxAngle = CalcMaxAngle(i);
        var minAngle = CalcMinAngle(i);
        var rotSign = rot < 0 ? -1f : 1f;
        rot = (float)Random.NextDouble() * (maxAngle - minAngle) + minAngle;
        // with a 40% chance to flip the sign from the last piece
        if (Random.NextDouble() < 0.4)
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
        m_trackOutline.Append(new Vertex(end.InvertY(), FillColor));

        m_trackShapes.Add(shape);
        m_trackBodies.Add(body);

        maxY = Math.Max(maxY, end.Y);
        minY = Math.Min(minY, end.Y);
      }

      // the finish line sensor is halfway down the finish line piece
      var sensor = BodyFactory.CreateEdge(m_world, 
        (end + new Vector2f(5, 0)).ToVector2(), 
        (end + new Vector2f(5, 10)).ToVector2()
        );
      sensor.IsSensor = true;
      sensor.CollisionCategories = FinishLineCategory;
      sensor.CollidesWith = Car.Entity.CollisionCategory;
      sensor.OnCollision += FinishLineOnCollision;
      m_trackBodies.Add(sensor);

      // create a landing pad at the end of the track
      start = end;
      shape = new RectangleShape
      {
        FillColor = FillColor,
        OutlineColor = OutlineColor,
        OutlineThickness = OutlineSize,
        Position = start.InvertY(),
        Size = new Vector2f(10, PieceSize.Y),
        Rotation = 0f
      };
      end = start + new Vector2f(shape.Size.X, 0);
      body = CreateBody(start, end);
      m_trackShapes.Add(shape);
      m_trackBodies.Add(body);
      m_trackOutline.Append(new Vertex(end.InvertY(), FillColor));
      maxX = end.X;

      // create a holder so cars don't go off the end of the track
      start = end;
      end = start + new Vector2f(0, 10);
      body = CreateBody(start, end);
      m_trackBodies.Add(body);

      Dimensions = new Vector2(maxX - minX, maxY - minY + 10);
      Center = new Vector2(
        minX + (Dimensions.X / 2f), minY + (Dimensions.Y / 2f));

      m_generated = true;
//       Log.DebugFormat("Generated {0} track pieces in {1} ms", 
//         m_trackShapes.Count, stopwatch.ElapsedMilliseconds);
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
      //Log.DebugFormat("Clearing {0} track pieces", m_trackShapes.Count);

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
      m_trackOutline.Clear();
      m_generated = false;
      m_finishLineCrossed = false;
    }

    /// <summary>
    /// Draws the track to the screen.
    /// </summary>
    /// <param name="target"></param>
    public void Draw(RenderTarget target)
    {
      if (target == null || !m_generated)
      {
        return;
      }

      foreach (var shape in m_trackShapes)
      {
        target.Draw(shape);
      }
    }

    /// <summary>
    /// Draws an outline of the track onto the overview.
    /// </summary>
    /// <param name="target"></param>
    public void DrawOverview(RenderTarget target)
    {
      if (target == null || !m_generated)
      {
        return;
      }

      target.Draw(m_trackOutline);
    }

    private bool FinishLineOnCollision(Fixture fixtureA, Fixture fixtureB, 
      Contact contact)
    {
      if (m_finishLineCrossed)
      {
        return true;
      }

      var car = fixtureA.IsSensor ? fixtureB.Body : fixtureA.Body;
      // the champion really shouldn't be the first to hit the sensor...
      // but just in case
      if ((int) car.UserData == Car.Population.ChampionId)
      {
        return true;
      }

      m_finishLineCrossed = true;
      Log.DebugFormat("Car {0} hit finish line", (int)car.UserData);
      if (FinishLineCrossed != null)
      {
        FinishLineCrossed((int)car.UserData);
      }
      
      return true;
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
        FillColor = FillColor,
        OutlineColor = OutlineColor,
        OutlineThickness = OutlineSize,
        Size = PieceSize,
        // invert the position and rotation for SFML
        Position = position.InvertY(),
        Rotation = rotation * -1f
      };
    }

    /// <summary>
    /// Calculates the maximum angle for a piece of 
    /// track in the range 
    /// [<see cref="MinPieceAngle"/>, <see cref="MaxPieceAngle"/>].
    /// </summary>
    /// <param name="index">The index of the track piece with 
    /// <see cref="NumPieces"/> as the maximum.
    /// </param>
    /// <returns></returns>
    private static float CalcMaxAngle(int index)
    {
      Debug.Assert(index < NumPieces);

      // uses y=x^3.5 in the range x=[2,10]
      // scale x to [2..10] based on its location in the track
      float x = (((index + 1f) / NumPieces) * 8f) + 2f;
      float y = (float) Math.Pow(x, 3.5);
      float percent = Math.Min(y, 1000f) / 1000f;
      return (percent * (MaxPieceAngle - MinPieceAngle)) + MinPieceAngle;
    }

    /// <summary>
    /// Calculates the maximum angle for a piece of 
    /// track in the range 
    /// [<see cref="MinPieceAngle"/>, <see cref="MaxPieceAngle"/>].  Will 
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

      // uses y=x^3.1-x^2.5 in the range x=[2,10]
      // scale x to [2..10] based on its location in the track
      float x = (((index + 1f) / NumPieces) * 8f) + 2;
      float y = (float)(Math.Pow(x, 3.1) - Math.Pow(x, 2.5));
      float percent = y / 1000f;
      return (percent * (MaxPieceAngle - MinPieceAngle)) + MinPieceAngle;
    }
  }
}
