using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FarseerPhysics.Dynamics;
using log4net;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace Genetic_Cars.Car
{
  sealed class Car : IDisposable
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly Font Font = new Font(@"fonts/arial.ttf");

    /// <summary>
    /// The position where all cars are generated.
    /// </summary>
    public static Vector2 StartPosition { get; set; }

    public delegate void HealthChangedHandler(int id, float health);

    private const int SpeedHistorySecs = 5;
    private const int SpeedHistorySamplesPerSec = 4;
    private const float SpeedHistorySampleInterval =
      1f / SpeedHistorySamplesPerSec;

    // roughly the number of seconds the car's speed must be below the 
    // threshold before it will die
    private const int SecondsTilDeath = 5;
    private const int MaxHealth = SecondsTilDeath * SpeedHistorySamplesPerSec;
    private const float LowSpeedThreshold = 0.5f;

    private bool m_disposed = false;
    private int m_id;
    private readonly PhysicsManager m_physicsManager;
    private Entity m_entity;
    private Vector2 m_lastPosition;
    
    private int m_health = MaxHealth;
    private readonly float[] m_speedHistory =
      new float[SpeedHistorySecs * SpeedHistorySamplesPerSec];
    private int m_speedHistoryIndex = 0;
    private float m_speedSampleTime = 0;

    private RenderStates m_overviewRenderStates = new RenderStates
    {
      BlendMode = BlendMode.Alpha,
      Transform = Transform.Identity
    };
    private Transform m_overviewLineTransform = Transform.Identity;
    private Transform m_overviewTextTransform = Transform.Identity;
    private readonly Vertex[] m_overviewLine =
    {
      new Vertex(new Vector2f(0, -1000)), 
      new Vertex(new Vector2f(0, 1000))
    };
    private Text m_overviewText;

    /// <summary>
    /// Creates a new car.
    /// </summary>
    /// <param name="physicsManager"></param>
    /// <param name="phenotype">
    /// Sets the phenotype of the car.  If null, generates a random phenotype.
    /// </param>
    public Car(PhysicsManager physicsManager, Phenotype phenotype = null)
    {
      if (physicsManager == null)
      {
        throw new ArgumentNullException("physicsManager");
      }

      m_physicsManager = physicsManager;
      if (phenotype != null)
      {
        Phenotype = phenotype;
        ResetEntity();
      }
      else
      {
        Generate();
      }
    }

    ~Car()
    {
      Dispose(false);
    }

    public event HealthChangedHandler HealthChanged;
    
    /// <summary>
    /// The id of the car (shared by all the car components).
    /// </summary>
    public int Id
    {
      get { return m_id; }
      set
      {
        m_id = value;
        if (Phenotype != null)
        {
          Phenotype.Id = m_id;
        }
        if (m_entity != null)
        {
          m_entity.Id = m_id;
        }
        m_overviewText = new Text(m_id.ToString(), Font, 8)
        {
          Color = Color.Black
        };
      }
    }

    /// <summary>
    /// Get the phenotype of the car.  After changing the phenotype call 
    /// ResetEntity to rebuild the graphics.
    /// </summary>
    public Phenotype Phenotype { get; set; }

    /// <summary>
    /// Gets the position of the car.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The score of the car is the max forward distance it has traveled.
    /// </summary>
    public float MaxForwardDistance { get; private set; }

    /// <summary>
    /// The total distance traveled by the car.
    /// </summary>
    public float DistanceTraveled { get; private set; }

    /// <summary>
    /// The car's health as a percentage.
    /// </summary>
    public float Health
    {
      get { return m_health / (float)MaxHealth; }
    }

    /// <summary>
    /// The current speed of the car in m/s.
    /// </summary>
    public float Speed { get; private set; }

    /// <summary>
    /// Returns the speed of the car, averaged over the interval defined by 
    /// SpeedHistorySecs.
    /// </summary>
    public float AverageSpeed { get; private set; }

    /// <summary>
    /// Returns true if the car is alive.
    /// </summary>
    public bool IsAlive { get { return m_health > 0; } }

    public EntityType Type
    {
      get
      {
        return m_entity == null ? EntityType.Normal : m_entity.Type;
      }
      set
      {
        if (m_entity != null)
        {
          m_entity.Type = value;
        }
        
        for (var i = 0; i < m_overviewLine.Length; i++)
        {
          switch (value)
          {
            case EntityType.Normal:
              m_overviewLine[i].Color = Color.Red;
              break;
            case EntityType.Clone:
              m_overviewLine[i].Color = Color.Blue;
              break;
            case EntityType.Champion:
              m_overviewLine[i].Color = Color.Green;
              break;
          }
        }
      }
    }

    /// <summary>
    /// Does a logic update for the car.
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime)
    {
      if (m_entity == null)
      {
        return;
      }

      // update the speed average
      m_speedSampleTime += deltaTime;
      while (m_speedSampleTime >= SpeedHistorySampleInterval)
      {
        m_speedSampleTime -= SpeedHistorySampleInterval;
        if (++m_speedHistoryIndex >= m_speedHistory.Length)
        {
          m_speedHistoryIndex = 0;
        }
        m_speedHistory[m_speedHistoryIndex] = Speed;
        AverageSpeed = m_speedHistory.Average();

        if (AverageSpeed < LowSpeedThreshold)
        {
          m_health--;
          if (m_health == 0)
          {
            ClearEntity();
            return;
          }
          OnHealthChanged();
        }
        else
        {
          m_health = MaxHealth;
          OnHealthChanged();
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

      if (m_entity != null)
      {
        m_entity.Draw(target);
      }
    }

    /// <summary>
    /// Draw a line representing the car onto the overview.
    /// </summary>
    /// <param name="target"></param>
    public void DrawOverview(RenderTarget target)
    {
      if (target == null)
      {
        return;
      }

      m_overviewRenderStates.Transform = m_overviewLineTransform;
      target.Draw(m_overviewLine, PrimitiveType.Lines, m_overviewRenderStates);
      
      m_overviewRenderStates.Transform = m_overviewTextTransform;
      target.Draw(m_overviewText, m_overviewRenderStates);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Replace this car by performing a crossover with the two provided cars.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="mutate">
    /// If true, perform a mutation after the crossover.
    /// </param>
    public void ReplaceWithCrossover(Car a, Car b, bool mutate)
    {
      ReplaceWithCrossover(a.Phenotype, b.Phenotype, mutate);
    }

    /// <summary>
    /// Replace this car by performing a crossover with the two provided 
    /// phenotypes.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="mutate">
    /// If true, perform a mutation after the crossover.
    /// </param>
    public void ReplaceWithCrossover(Phenotype a, Phenotype b, bool mutate)
    {
      Phenotype = Phenotype.CrossOver(a, b);
      if (mutate)
      {
        Phenotype.Mutate();
      }

      ResetEntity();
    }

    /// <summary>
    /// Replace this car with a new randomly generated car.
    /// </summary>
    public void Generate()
    {
      ClearEntity();
      Phenotype = new Phenotype();
      ResetEntity();
    }

    /// <summary>
    /// Rebuilds the car graphics at the starting position.
    /// </summary>
    public void ResetEntity()
    {
      ClearEntity();

      var definition = Phenotype.ToDefinition();
      m_entity = new Entity(definition, m_physicsManager) { Id = Id };
      m_physicsManager.PostStep += PhysicsPostStep;

      // reset the overview line alpha
      for (var i = 0; i < m_overviewLine.Length; i++)
      {
        m_overviewLine[i].Color.A = 255;
      }

      HealthChanged = null;
      Position = StartPosition;
      m_lastPosition = StartPosition;
      MaxForwardDistance = 0;
      DistanceTraveled = 0;
      m_health = MaxHealth;
      Speed = 0;
      AverageSpeed = 0;
    }

    private void OnHealthChanged()
    {
      if (HealthChanged != null)
      {
        HealthChanged(Id, Health);
      }
    }

    private void ClearEntity()
    {
      if (m_entity == null)
      {
        return;
      }

      // change the alpha of the overview line to 25%
      for (var i = 0; i < m_overviewLine.Length; i++)
      {
        m_overviewLine[i].Color.A = 64;
      }

      m_physicsManager.PostStep -= PhysicsPostStep;
      m_entity.Dispose();
      m_entity = null;
    }

    private void Dispose(bool disposeManaged)
    {
      if (m_disposed)
      {
        return;
      }

      if (disposeManaged)
      {
        if (m_entity != null)
        {
          m_entity.Dispose();
        }
        m_overviewText.Dispose();
      }

      m_physicsManager.PostStep -= PhysicsPostStep;

      m_disposed = true;
    }

    private void PhysicsPostStep(float deltaTime)
    {
      Debug.Assert(m_entity != null);

      Position = m_entity.Position;

      // position the overview line
      m_overviewLineTransform = Transform.Identity;
      m_overviewLineTransform.Translate(Position.X, 0);
      // offset the text from the line
      m_overviewTextTransform = m_overviewLineTransform;
      m_overviewTextTransform.Translate(2, -(Position.Y + 20));

      // update the car's speed
      var moved = Position - m_lastPosition;
      var movedLen = moved.Length();
      Speed = movedLen / deltaTime;
      if (moved.X < 0)
      {
        Speed = -Speed;
        DistanceTraveled -= movedLen;
      }
      else
      {
        DistanceTraveled += movedLen;
      }
      m_lastPosition = Position;
      MaxForwardDistance = Math.Max(MaxForwardDistance, DistanceTraveled);
    }
  }
}
