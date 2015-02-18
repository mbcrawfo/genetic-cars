using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using SFML.Graphics;

namespace Genetic_Cars.Car
{
  sealed class Population : IDisposable
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly int Size =
      Properties.Settings.Default.PopulationSize;

    private const int ChampionId = -1;
    
    /// <summary>
    /// The RNG used for all actions in this class.
    /// </summary>
    public static Random Random { get; set; }

    public delegate void NewGenerationHandler(int num);

    private bool m_disposed = false;
    private readonly PhysicsManager m_physicsManager;
    private int m_numClones = Properties.Settings.Default.DefaultNumClones;
    private float m_mutationRate;

    private readonly List<Car> m_cars = new List<Car>(Size); 

    private Car m_championCar;
    private float m_championDistance;

    public Population(PhysicsManager physicsManager)
    {
      if (physicsManager == null)
      {
        throw new ArgumentNullException("physicsManager");
      }

      m_physicsManager = physicsManager;
      Generate();
    }

    ~Population()
    {
      Dispose(false);
    }

    public event NewGenerationHandler NewGeneration;

    public Car Leader { get; private set; }

    public int Generation { get; private set; }

    public int LiveCount { get { return m_cars.Count(c => c.IsAlive); } }

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
    /// Does a logic update for the population.
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime)
    {
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
            Type = EntityType.Normal
          };
          m_cars.Add(car);
        }
      }
      else
      {
        foreach (var car in m_cars)
        {
          car.Generate();
          car.Type = EntityType.Normal;
        }
      }
      
      m_championDistance = 0;
      Leader = m_cars[0];
      Generation = 1;
      OnNewGeneration(Generation);
    }

    /// <summary>
    /// Creates the next generation of cars.
    /// </summary>
    private void NextGeneration()
    {
      m_cars.Sort(CarMaxDistanceComparator);
      UpdateChamption();

      var phenotypes = m_cars.GetRange(0, Size / 2)
        .Select(c => c.Phenotype).ToList();

      for (var i = 0; i < Size; i++)
      {
        if (i < NumClones)
        {
          m_cars[i].Id = 0;
          m_cars[i].ResetEntity();
          m_cars[i].Type = EntityType.Clone;
        }
        else
        {
          var a = phenotypes[Random.Next(phenotypes.Count())];
          var b = a;
          while (b == a)
          {
            b = phenotypes[Random.Next(phenotypes.Count())];
          }

          m_cars[i].Id = i;
          m_cars[i].ReplaceWithCrossover(a, b, 
            Random.NextDouble() < MutationRate);
          m_cars[i].Type = EntityType.Normal;
        }
      }

      Generation++;
      OnNewGeneration(Generation);
    }

    private void OnNewGeneration(int num)
    {
      if (NewGeneration != null)
      {
        NewGeneration(num);
      }
    }

    private void UpdateChamption()
    {
      if (m_cars[0].MaxForwardDistance > m_championDistance)
      {
        m_championDistance = m_cars[0].MaxForwardDistance;
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
