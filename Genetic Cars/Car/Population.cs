using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using SFML.Graphics;
using SFML.Window;

namespace Genetic_Cars.Car
{
  sealed class Population : IDisposable
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly int Size =
      Properties.Settings.Default.PopulationSize;
    private static readonly float BreedingPopPercent =
      Properties.Settings.Default.BreedingPopulationPercent;

    // A car must make it at least this much farther than the existing 
    // champion to become the new champion
    private const float ChampionThreshold = 1;

    /// <summary>
    /// The id always assigned to the champion ghost car.
    /// </summary>
    public const int ChampionId = -1;
    
    /// <summary>
    /// The RNG used for all actions in this class.
    /// </summary>
    public static Random Random { get; set; }

    /// <summary>
    /// Responds to a new generation of cars.
    /// </summary>
    /// <param name="num">The generation number.</param>
    /// <param name="cars">The list of new cars.</param>
    public delegate void NewGenerationHandler(int num, List<Car> cars);

    /// <summary>
    /// Responds to the population evolving a new champion car.
    /// </summary>
    /// <param name="generation"></param>
    /// <param name="id"></param>
    /// <param name="distance"></param>
    public delegate void NewChampionHandler(int generation, int id, 
      float distance);

    private bool m_disposed = false;
    private readonly PhysicsManager m_physicsManager;
    private int m_numClones = Properties.Settings.Default.NumClones;
    private float m_mutationRate = Properties.Settings.Default.MutationRate;
    private int m_numRandom = Properties.Settings.Default.NumRandom;

    private readonly List<Car> m_cars = new List<Car>(Size); 

    private Car m_championCar;
    private float m_championDistance;
    private RenderStates m_overviewRenderStates = new RenderStates
    {
      BlendMode = BlendMode.Alpha,
      Transform = Transform.Identity
    };
    private readonly Vertex[] m_championLine =
    {
      new Vertex(new Vector2f(0, -1000), Color.Green),
      new Vertex(new Vector2f(0, 1000), Color.Green),
    };

    /// <summary>
    /// Creates a new empty population.
    /// </summary>
    /// <param name="physicsManager"></param>
    public Population(PhysicsManager physicsManager)
    {
      if (physicsManager == null)
      {
        throw new ArgumentNullException("physicsManager");
      }

      m_physicsManager = physicsManager;
    }

    ~Population()
    {
      Dispose(false);
    }

    /// <summary>
    /// Signals that the population has created a new generation of cars.
    /// </summary>
    public event NewGenerationHandler NewGeneration;

    /// <summary>
    /// Signals that a new champion has been found.
    /// </summary>
    public event NewChampionHandler NewChampion;

    /// <summary>
    /// The car in the lead for the current generation.  Null if the population 
    /// is empty.
    /// </summary>
    public Car Leader { get; private set; }

    /// <summary>
    /// The current generation number.
    /// </summary>
    public int Generation { get; private set; }

    /// <summary>
    /// The count of cars that are currently alive.
    /// </summary>
    public int LiveCount { get { return m_cars.Count(c => c.IsAlive); } }

    /// <summary>
    /// The number of cars that will be cloned in each new generation.
    /// </summary>
    public int NumClones
    {
      get { return m_numClones; }
      set
      {
        if (value < 0 || value > Size)
        {
          throw new ArgumentOutOfRangeException("value");
        }
        m_numClones = value;
      }
    }

    /// <summary>
    /// The rate at which mutations happen during crossover.
    /// </summary>
    public float MutationRate
    {
      get { return m_mutationRate; }
      set
      {
        if (value < 0 || value > 1)
        {
          throw new ArgumentOutOfRangeException("value");
        }
        m_mutationRate = value;
      }
    }

    /// <summary>
    /// The number of cars that are randomly generated in each new generation.
    /// </summary>
    public int NumRandom
    {
      get { return m_numRandom; }
      set
      {
        if (value < 0 || value > Size)
        {
          throw new ArgumentOutOfRangeException("value");
        }
        m_numRandom = value;
      }
    }

    /// <summary>
    /// Get a particular car from the current generation.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Car GetCar(int id)
    {
      if (id < 0 || id >= Size)
      {
        throw new ArgumentOutOfRangeException("id");
      }

      var car = m_cars[id];
      Debug.Assert(car.Id == id);
      return car;
    }

    /// <summary>
    /// Does a logic update for the population.
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime)
    {
      if (m_championCar != null)
      {
        m_championCar.Update(deltaTime);
      }

      foreach (var car in m_cars)
      {
        car.Update(deltaTime);
      }

      if (LiveCount == 0)
      {
        NextGeneration();
        return;
      }

      Leader = m_cars.OrderByDescending(c => c.MaxForwardDistance)
        .First(c => c.IsAlive);
    }

    /// <summary>
    /// Draws the cars onto the target.
    /// </summary>
    /// <param name="target"></param>
    public void Draw(RenderTarget target)
    {
      if (target == null)
      {
        return;
      }

      if (m_championCar != null)
      {
        m_championCar.Draw(target);
      }
      foreach (var car in m_cars)
      {
        car.Draw(target);
      }
    }

    /// <summary>
    /// Draws a vertical line representing each car onto the overview.
    /// </summary>
    /// <param name="target"></param>
    public void DrawOverview(RenderTarget target)
    {
      if (target == null)
      {
        return;
      }

      if (m_championCar != null)
      {
        target.Draw(m_championLine, PrimitiveType.Lines, m_overviewRenderStates);
      }

      foreach (var car in m_cars)
      {
        car.DrawOverview(target);
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Generates a new random population, discarding the existing population.
    /// </summary>
    public void Generate()
    {
      if (m_championCar != null)
      {
        m_championCar.Dispose();
        m_championCar = null;
      }

      if (m_cars.Count == 0)
      {
        for (var i = 0; i < Size; i++)
        {
          var car = new Car(m_physicsManager)
          {
            Id = i, 
            Type = EntityType.Random
          };
          m_cars.Add(car);
        }
      }
      else
      {
        foreach (var car in m_cars)
        {
          car.Generate();
          car.Type = EntityType.Random;
        }
      }
      
      m_championDistance = 0;
      Leader = m_cars[0];
      Generation = 1;
      OnNewGeneration(Generation, m_cars);
    }

    /// <summary>
    /// Creates the next generation of cars.
    /// </summary>
    private void NextGeneration()
    {
      m_cars.Sort(CarMaxDistanceComparator);
      Log.DebugFormat("Results of generation {0}", Generation);
      foreach (var car in m_cars)
      {
        Log.DebugFormat("Car {0}: {1:F2} m", car.Id, car.MaxForwardDistance);
      }

      UpdateChamption();
      Generation++;
      Log.DebugFormat("**** Generation {0} ****", Generation);

      var breedingCount = (int)Math.Round(Size * BreedingPopPercent);
      Debug.Assert(breedingCount <= Size);
      var phenotypes = m_cars.GetRange(0, breedingCount)
        .Select(c => c.Phenotype).ToList();

      for (var i = 0; i < Size; i++)
      {
        if (i < NumClones)
        {
          Log.DebugFormat("Gen {0} car {1} will be cloned with new id car {2}",
            Generation - 1, m_cars[i].Id, i);
          m_cars[i].ResetEntity();
          m_cars[i].Id = i;
          m_cars[i].Type = EntityType.Clone;
        }
        else if (i < Size - NumRandom)
        {
          var a = phenotypes[Random.Next(phenotypes.Count())];
          var b = a;
          while (b.Id == a.Id)
          {
            b = phenotypes[Random.Next(phenotypes.Count())];
          }
          var mutate = Random.NextDouble() < MutationRate;

          Log.DebugFormat(
            "Car {0} generated by crossing gen {1} cars {2} and {3}, " +
            "mutation: {4}", i, Generation - 1, a.Id, b.Id, mutate);
          m_cars[i].ReplaceWithCrossover(a, b, mutate);
          m_cars[i].Id = i;
          m_cars[i].Type = EntityType.Normal;
        }
        else
        {
          Log.DebugFormat("Car {0} is randomly generating", i);
          m_cars[i].Phenotype = new Phenotype();
          m_cars[i].ResetEntity();
          m_cars[i].Id = i;
          m_cars[i].Type = EntityType.Random;
        }
      }
      
      OnNewGeneration(Generation, m_cars);
    }

    private void OnNewGeneration(int num, List<Car> cars)
    {
      if (NewGeneration != null)
      {
        NewGeneration(num, cars);
      }
    }

    private void OnNewChampion(int gen, int id, float dist)
    {
      if (NewChampion != null)
      {
        NewChampion(gen, id, dist);
      }
    }

    private void UpdateChamption()
    {
      if (m_cars[0].MaxForwardDistance > m_championDistance + ChampionThreshold)
      {
        var champ = m_cars[0];
        m_championDistance = champ.MaxForwardDistance;
        Log.DebugFormat(
          "New champion in generation {0}, car {1} distance {2:F2} m",
          Generation, champ.Id, m_championDistance);
        OnNewChampion(Generation, champ.Id, m_championDistance);

        var transform = Transform.Identity;
        transform.Translate(champ.Position.X, 0);
        m_overviewRenderStates.Transform = transform;

        if (m_championCar == null)
        {
          m_championCar = new Car(m_physicsManager, m_cars[0].Phenotype)
          {
            Id = ChampionId
          };
        }
        else
        {
          m_championCar.Phenotype = m_cars[0].Phenotype;
          m_championCar.ResetEntity();
        }
      }
      else
      {
        m_championCar.ResetEntity();
      }
      m_championCar.Type = EntityType.Champion;
    }

    private void Dispose(bool disposeManaged)
    {
      if (m_disposed)
      {
        return;
      }

      if (disposeManaged)
      {
        if (m_championCar != null)
        {
          m_championCar.Dispose();
        }
        foreach (var car in m_cars)
        {
          car.Dispose();
        }
      }

      m_disposed = true;
    }
    
    /// <summary>
    /// Sorts two cars with the greater distance first.
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <returns></returns>
    private static int CarMaxDistanceComparator(Car c1, Car c2)
    {
      if (c1.MaxForwardDistance > c2.MaxForwardDistance)
      {
        return -1;
      }
      else if (c1.MaxForwardDistance == c2.MaxForwardDistance)
      {
        return 0;
      }
      else
      {
        return 1;
      }
    }
  }
}
