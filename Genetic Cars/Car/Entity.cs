using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using log4net;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

// ReSharper disable RedundantDefaultMemberInitializer

namespace Genetic_Cars.Car
{
  enum EntityType
  {
    Normal,
    Clone,
    Random,
    Champion
  }

  /// <summary>
  /// Holds the graphics and physics objects for a car.
  /// </summary>
  sealed class Entity : IDisposable
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    // collision category for all car components
    public static readonly Category CollisionCategory = Category.Cat2;
    
    // graphical properties for the car
    private static readonly Color OutlineColor = Color.Black;
    private const float OutlineThickness = -.05f;
    private const float WheelAxisLineThickness = .04f;

    // cars are accelerated over a period of time to try to avoid wheelies
    // delta time for acceleration in seconds
    private const float AccelerationTime = 5;
    // time between each acceleration step
    private const float AccelerationInterval = 0.1f;
    // total number of acceleration steps
    private static readonly int AccelerationSteps =
      (int)Math.Round(AccelerationTime / AccelerationInterval);
    
    private bool m_disposed = false;
    
    private readonly PhysicsManager m_physicsManager;
    private readonly Definition m_definition;
    private EntityType m_type;
    private int m_id;

    // graphics fields
    private ConvexShape m_bodyShape;
    private CircleShape[] m_wheelShapes;
    private RectangleShape[] m_wheelLines;
    // physics fields
    private Body m_bodyBody;
    private Body[] m_wheelBodies;
    private RevoluteJoint[] m_wheelJoints;
    // acceleration fields
    private float m_accelerationTime;
    private float[] m_torqueStep;

    /// <summary>
    /// Builds a car.
    /// </summary>
    /// <param name="def">The parameters used to generate the car.</param>
    /// <param name="physics">The program physics system.</param>
    public Entity(Definition def, PhysicsManager physics)
    {
      if (physics == null)
      {
        throw new ArgumentNullException("physics");
      }
      if (def == null)
      {
        throw new ArgumentNullException("def");
      }
      // will throw on failure
      def.Validate();
      m_definition = def;
      m_physicsManager = physics;
      m_physicsManager.PostStep += PhysicsPostStep;
      
      CreateBody();
      CreateWheels();

      Type = EntityType.Normal;
    }

    ~Entity()
    {
      Dispose(false);
    }

    /// <summary>
    /// Just an identifier for this car.
    /// </summary>
    public int Id
    {
      get { return m_id; }
      set
      {
        m_id = value;
        m_bodyBody.UserData = m_id;
        foreach (var wheelBody in m_wheelBodies)
        {
          wheelBody.UserData = m_id;
        }
      }
    }
    
    /// <summary>
    /// The geometric center of the car's body.
    /// </summary>
    public Vector2 Position
    {
      get { return m_bodyBody.Position; }
    }
    
    /// <summary>
    /// Sets the type of the car entity, affecting how cars are displayed.  
    /// Defaults to normal.
    /// Normal: Red body
    /// Clone: Blue body
    /// Champion: Transparent green body, transparent wheels.
    /// </summary>
    public EntityType Type
    {
      get { return m_type; }
      set
      {
        Debug.Assert(m_bodyShape != null);
        Debug.Assert(m_wheelShapes.All(s => s != null));

        m_type = value;
        //Log.DebugFormat("Car {0} type set to {1}", Id, m_type);

        var density = 
          m_definition.CalcBodyDensity() / Definition.MaxBodyDensity;
        // greater density = darker color
        byte color = (byte)(255 - (125 * density));
        byte alpha = 255;

        switch (m_type)
        {
          case EntityType.Normal:
            m_bodyShape.FillColor = new Color(color, 0, 0);
            break;

          case EntityType.Clone:
            m_bodyShape.FillColor = new Color(0, 0, color);
            break;

          case EntityType.Random:
            m_bodyShape.FillColor = new Color(color, 0, color);
            break;

          case EntityType.Champion:
            alpha = 64;
            m_bodyShape.FillColor = new Color(0, color, 0, alpha);
            break;
        }

        m_bodyShape.OutlineColor = m_bodyShape.OutlineColor.SetAlpha(alpha);
        for (var i = 0; i < m_wheelShapes.Length; i++)
        {
          var shape = m_wheelShapes[i];
          shape.FillColor = shape.FillColor.SetAlpha(alpha);
          shape.OutlineColor = shape.OutlineColor.SetAlpha(alpha);

          var line = m_wheelLines[i];
          line.FillColor = line.FillColor.SetAlpha(alpha);
        }
      }
    }

    /// <summary>
    /// Draws the car onto the target.
    /// </summary>
    /// <param name="target"></param>
    public void Draw(RenderTarget target)
    {
      if (target == null)
      {
        return;
      }

      for (var i = 0; i < m_wheelShapes.Length; i++)
      {
        target.Draw(m_wheelShapes[i]);
        target.Draw(m_wheelLines[i]);
      }
      target.Draw(m_bodyShape);
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

        for (var i = 0; i < m_wheelShapes.Length; i++)
        {
          m_wheelShapes[i].Dispose();
          m_wheelLines[i].Dispose();
        }
      }

      m_physicsManager.PreStep -= ApplyAcceleration;
      m_physicsManager.PostStep -= PhysicsPostStep;

      for (var i = 0; i < m_wheelShapes.Length; i++)
      {
        m_wheelBodies[i].OnCollision -= WheelInitialCollision;
        m_physicsManager.World.RemoveJoint(m_wheelJoints[i]);
        m_physicsManager.World.RemoveBody(m_wheelBodies[i]);
      }
      m_physicsManager.World.RemoveBody(m_bodyBody);

      m_disposed = true;
    }

    /// <summary>
    /// Creates the graphics and physics objects for the body of the car.
    /// </summary>
    private void CreateBody()
    {
      m_bodyShape = new ConvexShape((uint)Definition.NumBodyPoints)
      {
        OutlineColor = OutlineColor,
        OutlineThickness = OutlineThickness,
        Position = Car.StartPosition.ToVector2f().InvertY()
      };

      // build the vertex list for the polygon
      var vertices = new Vertices(Definition.NumBodyPoints);
      var angleStep = 360f / Definition.NumBodyPoints;
      var angle = 0f;
      for (int i = 0; i < Definition.NumBodyPoints; i++)
      {
        // the distance this point is from the center
        var distance = m_definition.CalcBodyPoint(i);
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
        m_physicsManager.World, vertices, m_definition.CalcBodyDensity(),
        Car.StartPosition
        );
      m_bodyBody.BodyType = BodyType.Dynamic;
      m_bodyBody.Friction = 1;
      m_bodyBody.CollidesWith = ~CollisionCategory;
      m_bodyBody.CollisionCategories = CollisionCategory;
    }

    /// <summary>
    /// Creates the wheels for the car.  Must be called after CreateBody.
    /// </summary>
    private void CreateWheels()
    {
      Debug.Assert(m_bodyShape != null);
      Debug.Assert(m_bodyBody != null);

      m_wheelShapes = new CircleShape[Definition.NumWheels];
      m_wheelLines = new RectangleShape[Definition.NumWheels];
      m_wheelBodies = new Body[Definition.NumWheels];
      m_wheelJoints = new RevoluteJoint[Definition.NumWheels];
      m_torqueStep = new float[Definition.NumWheels];

      for (int i = 0; i < m_wheelShapes.Length; i++)
      {
        // the offset of the attachment point from the center of the main body
        var attachOffset =
          m_bodyShape.GetPoint((uint) m_definition.WheelAttachment[i]);
        // the world position of the attachment point
        var attachPos = attachOffset + m_bodyShape.Position;
        var radius = m_definition.CalcWheelRadius(i);
        var density = m_definition.CalcWheelDensity(i);
        var densityFraction = density / Definition.MaxWheelDensity;
        // greater density = darker color
        byte color = (byte)(255 - (210 * densityFraction));

        var shape = new CircleShape
        {
          FillColor = new Color(color, color, color),
          OutlineColor = OutlineColor,
          OutlineThickness = OutlineThickness,
          Origin = new Vector2f(radius, radius),
          Position = attachPos,
          Radius = radius
        };
        var line = new RectangleShape
        {
          FillColor = Color.Black,
          Origin = new Vector2f(0, WheelAxisLineThickness / 2f),
          Size = new Vector2f(WheelAxisLineThickness, radius - WheelAxisLineThickness),
          Position = shape.Position
        };

        var body = BodyFactory.CreateCircle(
          m_physicsManager.World, radius, density, 
          attachPos.ToVector2().InvertY()
          );
        body.BodyType = BodyType.Dynamic;
        body.Friction = 1;
        body.CollidesWith = Track.CollisionCategory;
        body.CollisionCategories = CollisionCategory;

        // need to catch the first collision of this body
        body.OnCollision += WheelInitialCollision;

        var joint = new RevoluteJoint(
          m_bodyBody, attachOffset.ToVector2().InvertY(), body,
          new Vector2(0, 0))
        {
          MotorEnabled = false, 
          MaxMotorTorque = 0, 
          // speed must be negative for clockwise rotation
          MotorSpeed =
            -(float)MathExtensions.DegToRad(m_definition.CalcWheelSpeed(i))
        };
        m_physicsManager.World.AddJoint(joint);

        m_wheelShapes[i] = shape;
        m_wheelLines[i] = line;
        m_wheelBodies[i] = body;
        m_wheelJoints[i] = joint;
      }
    }

    /// <summary>
    /// Called on the first collision for either wheel (should always be with 
    /// the track) to enable the motors, and starts the wheel acceleration.
    /// </summary>
    /// <param name="fixtureA"></param>
    /// <param name="fixtureB"></param>
    /// <param name="contact"></param>
    /// <returns></returns>
    private bool WheelInitialCollision(
      Fixture fixtureA, Fixture fixtureB, Contact contact)
    {
      //Log.DebugFormat("Starting the motors of car {0}", Id);

      for (var i = 0; i < m_wheelBodies.Length; i++)
      {
        m_wheelJoints[i].MotorEnabled = true;
        m_torqueStep[i] = m_definition.CalcWheelTorque(i) / AccelerationSteps;
        m_wheelBodies[i].OnCollision -= WheelInitialCollision;
      }
      
      m_accelerationTime = 0;
      m_physicsManager.PreStep += ApplyAcceleration;

      return true;
    }

    /// <summary>
    /// Updates the state of the car following a physics step.
    /// </summary>
    /// <param name="deltaTime"></param>
    private void PhysicsPostStep(float deltaTime)
    {
      // sync the positions of the graphical shapes to the physics bodies
      var pos = m_bodyBody.Position.ToVector2f().InvertY();
      m_bodyShape.Position = pos;
      m_bodyShape.Rotation = 
        (float)-MathExtensions.RadToDeg(m_bodyBody.Rotation);
      for (int i = 0; i < m_wheelShapes.Length; i++)
      {
        var wheelPos = m_wheelBodies[i].Position.ToVector2f().InvertY();
        var wheelRot = 
          (float)-MathExtensions.RadToDeg(m_wheelBodies[i].Rotation);
        m_wheelShapes[i].Position = wheelPos;
        m_wheelLines[i].Position = wheelPos;
        m_wheelLines[i].Rotation = wheelRot;
      }
    }

    /// <summary>
    /// Increases the torque for each wheel until the defined max torque is 
    /// reached.
    /// </summary>
    /// <param name="deltaTime"></param>
    private void ApplyAcceleration(float deltaTime)
    {
      var done = false;
      m_accelerationTime += deltaTime;
      while (m_accelerationTime >= AccelerationInterval)
      {
        m_accelerationTime -= AccelerationInterval;
        for (var i = 0; i < m_wheelJoints.Length; i++)
        {
          m_wheelJoints[i].MaxMotorTorque += m_torqueStep[i];
          if (m_wheelJoints[i].MaxMotorTorque >= 
            m_definition.CalcWheelTorque(i))
          {
            done = true;
          }
        }

        if (done)
        {
          //Log.DebugFormat("Car {0} completed acceleration", Id);
          m_physicsManager.PreStep -= ApplyAcceleration;
          break;
        }
      }
    }
  }
}
