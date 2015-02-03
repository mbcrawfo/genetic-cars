using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using log4net;
using SFML.Graphics;
using SFML.Window;
using CircleShape = SFML.Graphics.CircleShape;

namespace Genetic_Cars
{
  class CarDef
  {
    // the points that make up the polygon of the car's body, starting with 
    // point 0 at 3 o'clock and going CCW around the center.  each value 
    // is in the range [0,1] indicating the distance from the center as a 
    // percentage of the max distance
    public float[] BodyPoints { get; private set; }
    // body mass, in the range [0,1] as a percentage of the max mass
    public float BodyMass { get; set; }
    // the points on the body where the wheels are attached, given as indices 
    // in bodyPoints
    public int[] WheelAttachment { get; private set; }
    // the radiuses of the wheels, with each value in the range [0,1] as a 
    // percentage of the max radius
    public float[] WheelRadius { get; private set; }
    // the speed of the wheels, in the range [0,1] as a percentage of the max
    // speed
    public float WheelSpeed { get; set; }

    public CarDef()
    {
      BodyPoints = new float[Car.NumBodyPoints];
      WheelAttachment = new int[Car.NumWheels];
      WheelRadius = new float[Car.NumWheels];
    }
  }

  class Car : IDisposable
  {
    public const int NumBodyPoints = 8;
    public const int NumWheels = 2;

    private const float MinBodyDistance = .5f;
    private const float MaxBodyDistance = 2;
    private const float MinBodyMass = 10;
    private const float MaxBodyMass = 100;

    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly Vector2f StartPosition = new Vector2f(1, 4);
    public static readonly Category CollisionCategory = Category.Cat2;

    private readonly World m_world;
    private ConvexShape m_bodyShape;
    private Body m_bodyBody;
    private CircleShape[] m_wheelShape;
    
    public Car(CarDef def, World world)
    {
      if (world == null)
      {
        throw new ArgumentNullException("world");
      }
      m_world = world;

      ValidateCarDef(def);
      Definition = def;

      BuildBody();
      BuildWheels();
    }

    public CarDef Definition { get; private set; }

    public void Draw(RenderTarget target)
    {
      var pos = m_bodyBody.Position.ToVector2f().InvertY();
      m_bodyShape.Position = pos;
      m_bodyShape.Rotation = (float)-MathExtensions.RadToDeg(m_bodyBody.Rotation);
      target.Draw(m_bodyShape);

      m_wheelShape[0].Position = pos;
      target.Draw(m_wheelShape[0]);
    }

    public void Dispose()
    {
    }

    private void BuildBody()
    {
      m_bodyShape = new ConvexShape(NumBodyPoints)
      {
        FillColor = Color.Red,
        Position = StartPosition.InvertY()
      };
      m_bodyBody = BodyFactory.CreateBody(m_world, StartPosition.ToVector2());
      m_bodyBody.BodyType = BodyType.Dynamic;

      var vertices = new Vertices(NumBodyPoints);
      const float angleStep = 360f / NumBodyPoints;
      var angle = 0f;
      for (int i = 0; i < NumBodyPoints; i++)
      {
        var distance = (Definition.BodyPoints[i] * 
          (MaxBodyDistance - MinBodyDistance)) + MinBodyDistance;
        var point = new Vector2f
        {
          X = distance * (float)Math.Cos(MathExtensions.DegToRad(angle)),
          Y = distance * (float)Math.Sin(MathExtensions.DegToRad(angle))
        };
        m_bodyShape.SetPoint((uint)i, point.InvertY());
        vertices.Add(point.ToVector2());

        angle += angleStep;
      }
      FixtureFactory.AttachPolygon(vertices, 1, m_bodyBody);
      m_bodyBody.CollidesWith = ~CollisionCategory;
      m_bodyBody.CollisionCategories = CollisionCategory;
      var mass = (Definition.BodyMass * 
        (MaxBodyMass - MinBodyMass)) + MinBodyMass;
      m_bodyBody.Mass = mass;
    }

    private void BuildWheels()
    {
      m_wheelShape = new CircleShape[2];

      for (int i = 0; i < m_wheelShape.Length; i++)
      {
        var pos = m_bodyShape.GetPoint((uint)Definition.WheelAttachment[i]);
        var offset = pos - m_bodyShape.Position;
        var color = Color.Black;
        color.A = 128;
        m_wheelShape[i] = new CircleShape
        {
          FillColor = color,
          Origin = pos + new Vector2f(.25f, .25f),
          Position = m_bodyShape.Position,
          Radius = .5f
        };
      }
    }

    private static void ValidateCarDef(CarDef def)
    {
      for (int i = 0; i < def.BodyPoints.Length; i++)
      {
        if (def.BodyPoints[i] < 0 || def.BodyPoints[i] > 1)
        {
          throw new ArgumentOutOfRangeException(
            "def", @"BodyPoints[" + i + @"] out of range");
        }
      }
      if (def.BodyMass < 0 || def.BodyMass > 1)
      {
        throw new ArgumentOutOfRangeException("def", @"BodyMass out of range");
      }
      for (int i = 0; i < def.WheelAttachment.Length; i++)
      {
        if (def.WheelAttachment[i] < 0 || def.WheelAttachment[i] >= NumBodyPoints)
        {
          throw new ArgumentOutOfRangeException(
            "def", @"WheelAttachment[" + i + @"] out of range");
        }
      }
      for (int i = 0; i < def.WheelRadius.Length; i++)
      {
        if (def.WheelRadius[i] < 0 || def.WheelRadius[i] > 1)
        {
          throw new ArgumentOutOfRangeException(
            "def", @"WheelRadius[" + i + @"] out of range");
        }
      }
    }
  }
}
