using System;
using System.Diagnostics;
using System.Reflection;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using log4net;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace Genetic_Cars
{
  sealed class Car : IDisposable
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly Color OutlineColor = Color.Black;
    private const float OutlineThickness = -.04f;
    private const float WheelAxisLineThickness = .04f;
    public static readonly Category CollisionCategory = Category.Cat2;

    public static Vector2f StartPosition { get; set; }

    private bool m_disposed = false;

    private readonly World m_world;
    private ConvexShape m_bodyShape;
    private Body m_bodyBody;
    private CircleShape[] m_wheelShapes;
    private RectangleShape[] m_wheelLines;
    private Body[] m_wheelBodies;
    private RevoluteJoint[] m_wheelJoints;

    public Car(CarDef def, World world)
    {
      if (world == null)
      {
        throw new ArgumentNullException("world");
      }
      if (def == null)
      {
        throw new ArgumentNullException("def");
      }
      
      // will throw on failure
      def.Validate();
      m_world = world;
      Definition = def;

      CreateBody();
      CreateWheels();
    }

    ~Car()
    {
      Dispose(false);
    }

    public CarDef Definition { get; private set; }

    public Vector2f Center
    {
      get { return m_bodyShape.Position; }
    }

    public void Draw(RenderTarget target)
    {
      target.Draw(m_bodyShape);

      for (int i = 0; i < m_wheelShapes.Length; i++)
      {
        target.Draw(m_wheelShapes[i]);
        target.Draw(m_wheelLines[i]);
      }
    }

    public void SyncPositions()
    {
      var pos = m_bodyBody.Position.ToVector2f().InvertY();
      m_bodyShape.Position = pos;
      m_bodyShape.Rotation = (float)-MathExtensions.RadToDeg(m_bodyBody.Rotation);

      for (int i = 0; i < m_wheelShapes.Length; i++)
      {
        var wheelPos = m_wheelBodies[i].Position.ToVector2f().InvertY();
        var wheelRot = (float) -MathExtensions.RadToDeg(m_wheelBodies[i].Rotation);

        m_wheelShapes[i].Position = wheelPos;
        m_wheelLines[i].Position = wheelPos;
        m_wheelLines[i].Rotation = wheelRot;
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposeManaged)
    {
      if (m_disposed)
      {
        return;
      }

      if (disposeManaged)
      {
        m_bodyShape.Dispose();

        for (int i = 0; i < m_wheelShapes.Length; i++)
        {
          m_wheelShapes[i].Dispose();
          m_wheelLines[i].Dispose();
        }
      }

      for (int i = 0; i < m_wheelShapes.Length; i++)
      {
        m_world.RemoveJoint(m_wheelJoints[i]);
        m_world.RemoveBody(m_wheelBodies[i]);
      }
      m_world.RemoveBody(m_bodyBody);

      m_disposed = true;
    }

    private void CreateBody()
    {
      m_bodyShape = new ConvexShape((uint)CarDef.NumBodyPoints)
      {
        FillColor = new Color(200, 0, 0),
        OutlineColor = OutlineColor,
        OutlineThickness = OutlineThickness,
        Position = StartPosition.InvertY()
      };

      // build the vertex list for the polygon
      var vertices = new Vertices(CarDef.NumBodyPoints);
      var angleStep = 360f / CarDef.NumBodyPoints;
      var angle = 0f;
      for (int i = 0; i < CarDef.NumBodyPoints; i++)
      {
        // the distance this point is from the center
        var distance = Definition.CalcBodyPoint(i);
        // turn the distance into a point centered around (0,0)
        var point = new Vector2f
        {
          X = distance * (float)Math.Cos(MathExtensions.DegToRad(angle)),
          Y = distance * (float)Math.Sin(MathExtensions.DegToRad(angle))
        };
        m_bodyShape.SetPoint((uint)i, point.InvertY());
        vertices.Add(point.ToVector2());

        angle += angleStep;
      }

      // build the physics shape
      m_bodyBody = BodyFactory.CreatePolygon(
        m_world, vertices, Definition.CalcBodyDensity(), 
        StartPosition.ToVector2()
        );
      m_bodyBody.BodyType = BodyType.Dynamic;
      m_bodyBody.CollidesWith = ~CollisionCategory;
      m_bodyBody.CollisionCategories = CollisionCategory;
    }

    private void CreateWheels()
    {
      Debug.Assert(m_bodyShape != null);
      Debug.Assert(m_bodyBody != null);

      m_wheelShapes = new CircleShape[CarDef.NumWheels];
      m_wheelLines = new RectangleShape[CarDef.NumWheels];
      m_wheelBodies = new Body[CarDef.NumWheels];
      m_wheelJoints = new RevoluteJoint[CarDef.NumWheels];

      for (int i = 0; i < m_wheelShapes.Length; i++)
      {
        // the offset of the attachment point from the center of the main body
        var attachOffset =
          m_bodyShape.GetPoint((uint) Definition.WheelAttachment[i]);
        // the world position of the attachment point
        var attachPos = attachOffset + m_bodyShape.Position;
        var radius = Definition.CalcWheelRadius(i);
        var color = Color.White;

        var shape = new CircleShape
        {
          FillColor = color,
          OutlineColor = OutlineColor,
          OutlineThickness = OutlineThickness,
          Origin = new Vector2f(radius, radius),
          Position = attachPos,
          Radius = radius
        };
        var line = new RectangleShape
        {
          FillColor = Color.Black,
          Origin = new Vector2f(0, -WheelAxisLineThickness / 2f),
          Size = new Vector2f(WheelAxisLineThickness, radius),
          Position = shape.Position
        };

        var body = BodyFactory.CreateCircle(
          m_world, radius, Definition.CalcWheelDensity(i), 
          attachPos.ToVector2().InvertY()
          );
        body.BodyType = BodyType.Dynamic;
        body.Friction = 1;
        body.CollidesWith = ~CollisionCategory;
        body.CollisionCategories = CollisionCategory;

        var joint = new RevoluteJoint(
          m_bodyBody, attachOffset.ToVector2().InvertY(), body,
          new Vector2(0, 0))
        {
          MotorEnabled = true, 
          MaxMotorTorque = 100, 
          MotorSpeed = -10
        };
        m_world.AddJoint(joint);

        m_wheelShapes[i] = shape;
        m_wheelLines[i] = line;
        m_wheelBodies[i] = body;
        m_wheelJoints[i] = joint;
      }
    }
  }
}
