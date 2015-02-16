using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using SFML.Graphics;

namespace Genetic_Cars.Car
{
  sealed class Population : IDrawable, IDisposable
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly int PopulationSize =
      Properties.Settings.Default.PopulationSize;

    /// <summary>
    /// The RNG used for all actions in this class.
    /// </summary>
    public static Random Random { get; set; }

    private bool m_disposed = false;
    private readonly PhysicsManager m_physicsManager;
    private int m_numClones = 5;
    private float m_mutationRate;

    private List<Phenotype> m_phenotypes = 
      new List<Phenotype>(PopulationSize);
    private readonly List<Entity> m_entities = new List<Entity>(PopulationSize);
    private readonly float[] m_scores = new float[PopulationSize];

    private Phenotype m_championPhenotype;
    private Entity m_championEntity;
    private float m_championDistance;
    

    public Population(PhysicsManager physicsManager)
    {
      if (physicsManager == null)
      {
        throw new ArgumentNullException("physicsManager");
      }

      m_physicsManager = physicsManager;
      m_physicsManager.PostStep += PhysicsPostStep;

      Generate();
    }

    ~Population()
    {
      Dispose(false);
    }

    public Entity Leader { get; private set; }

    public int Generation { get; private set; }

    public int LiveCount { get; private set; }

    public int NumClones
    {
      get { return m_numClones; }
      set
      {
        if (value < 0 || value > PopulationSize)
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

    public void Draw(RenderTarget target)
    {
      if (m_championEntity != null)
      {
        m_championEntity.Draw(target);
      }
      foreach (var entity in m_entities.Where(e => e != null))
      {
        entity.Draw(target);
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
      // clear the old
      m_phenotypes.Clear();
      foreach (var entity in m_entities.Where(e => e != null))
      {
        entity.Dispose();
      }
      m_entities.Clear();
      m_championPhenotype = null;
      m_championEntity = null;
      Array.Clear(m_scores, 0, m_scores.Length);

      // build the new
      for (var i = 0; i < PopulationSize; i++)
      {
        var phenotype = new Phenotype();
        m_phenotypes.Add(phenotype);

        var entity = new Entity(i, phenotype.ToDefinition(), m_physicsManager);
        entity.Death += EntityDeath;
        m_entities.Add(entity);
      }

      Leader = m_entities[0];
      Generation = 1;
      LiveCount = PopulationSize;
    }

    /// <summary>
    /// Creates the next generation of cars.
    /// </summary>
    public void NextGeneration()
    {
      // clean up the old, just in case
      foreach (var entity in m_entities.Where(e => e != null))
      {
        entity.Dispose();
      }
      m_entities.Clear();
      if (m_championEntity != null)
      {
        m_championEntity.Dispose();
        m_championEntity = null;
      }

      BuildNewPhenotypes();
      Array.Clear(m_scores, 0, m_scores.Length);
      Debug.Assert(m_championPhenotype != null);

      m_championEntity = new Entity(-1, m_championPhenotype.ToDefinition(),
        m_physicsManager);
      m_championEntity.Type = EntityType.Champion;

      for (var i = 0; i < m_phenotypes.Count; i++)
      {
        var phenotype = m_phenotypes[i];
        var entity = new Entity(i, phenotype.ToDefinition(), m_physicsManager);
        if (i < NumClones)
        {
          entity.Type = EntityType.Clone;
        }

        entity.Death += EntityDeath;
        m_entities.Add(entity);
      }

      Leader = m_entities[0];
      Generation++;
      LiveCount = PopulationSize;
    }

    private void BuildNewPhenotypes()
    {
      var result = new List<Phenotype>(PopulationSize);

      var orderedScores = m_scores.OrderByDescending(s => s).ToArray();
      var midScore = m_scores[m_scores.Length / 2];
      var bestHalf = m_phenotypes.Where((t, i) => m_scores[i] >= midScore).ToArray();

      if (m_championPhenotype == null || 
        orderedScores[0] > m_championDistance)
      {
        m_championDistance = orderedScores[0];
        m_championPhenotype = 
          m_phenotypes[m_scores.ToList().IndexOf(m_championDistance)];
      }

      if (NumClones > 0)
      {
        // phenotypes who had a score >= this will be kept
        var keepThreshold = orderedScores[NumClones];
        result.AddRange(
          m_phenotypes.Where((t, i) => m_scores[i] >= keepThreshold));
      }

      while (result.Count < PopulationSize)
      {
        Phenotype a = bestHalf[Random.Next(bestHalf.Length)];
        Phenotype b = a;
        while (a == b)
        {
          b = bestHalf[Random.Next(bestHalf.Length)];
        }

        var c = Phenotype.CrossOver(a, b);
        if (Random.NextDouble() < MutationRate)
        {
          c.Mutate();
        }
        result.Add(c);
      }

      m_phenotypes = result;
    }

    private void Dispose(bool disposeManaged)
    {
      if (m_disposed)
      {
        return;
      }

      if (disposeManaged)
      {
        foreach (var entity in m_entities.Where(e => e != null))
        {
          entity.Dispose();
        }
        if (m_championEntity != null)
        {
          m_championEntity.Dispose();
        }
      }

      m_disposed = true;
    }

    private void SetLeader()
    {
      Leader = m_entities.Where(e => e != null)
        .OrderByDescending(e => e.DistanceTraveled).First();
    }

    private void PhysicsPostStep(float deltaTime)
    {
      if (LiveCount == 0)
      {
        NextGeneration();
      }

      SetLeader();
    }

    private void EntityDeath(int id)
    {
      Log.DebugFormat("Handling death of car {0}", id);
      
      var entity = m_entities[id];
      m_scores[id] = entity.DistanceTraveled;
      entity.Dispose();
      m_entities[id] = null;

      if (--LiveCount > 0)
      {
        SetLeader();
      }
    }
  }
}
