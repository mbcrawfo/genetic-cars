using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using log4net;
using SFML.Graphics;
using SFML.Window;

namespace Genetic_Cars
{
  struct CarDef
  {
    // the points that make up the polygon of the car's body, starting with 
    // point 0 at 3 o'clock and going CCW around the center.  each value 
    // is in the range [0,1] indicating the distance from the center as a 
    // percentage of the max distance
    public float[] bodyPoints;
    // body mass, in the range [0,1] as a percentage of the max mass
    public float bodyMass;
    // the points on the body where the wheels are attached, given as indices 
    // in bodyPoints
    public int[] wheelAttachment;
    // the radiuses of the wheels, with each value in the range [0,1] as a 
    // percentage of the max radius
    public float[] wheelRadius;
    // the speed of the wheels, in the range [0,1] as a percentage of the max
    // speed
    public float wheelSpeed;
  }

  class Car : IDisposable
  {
    public const int NumBodyPoints = 8;

    private const float MinBodyDistance = .5f;
    private const float MaxBodyDistance = 2;

    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly Vector2f StartPosition = new Vector2f(1, 4);

    private readonly World m_world;
    private ConvexShape m_bodyShape;
    private Body m_bodyBody;
    
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
    }

    public CarDef Definition { get; private set; }

    public void Draw(RenderTarget target)
    {
      var pos = m_bodyBody.Position.ToVector2f().InvertY();
      m_bodyShape.Position = pos;
      m_bodyShape.Rotation = (float)-MathExtensions.RadToDeg(m_bodyBody.Rotation);
      target.Draw(m_bodyShape);
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
      m_bodyBody.CollidesWith = Track.CollisionCategory;

      var vertices = new Vertices(NumBodyPoints);
      const float angleStep = 360f / NumBodyPoints;
      var angle = 0f;
      for (int i = 0; i < NumBodyPoints; i++)
      {
        var distance = (Definition.bodyPoints[i] * 
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
    }

    private static void ValidateCarDef(CarDef def)
    {
      if (def.bodyPoints.Length != NumBodyPoints)
      {
        throw new ArgumentOutOfRangeException(
          "def", "bodyPoints doesn't match Car.NumBodyPoints");
      }
      for (int i = 0; i < def.bodyPoints.Length; i++)
      {
        if (def.bodyPoints[i] < 0 || def.bodyPoints[i] > 1)
        {
          throw new ArgumentOutOfRangeException(
            "def", "bodyPoints[" + i + "] out of range");
        }
      }
    }
  }
}
