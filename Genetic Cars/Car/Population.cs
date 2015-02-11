using System;
using System.Collections.Generic;
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

    private readonly List<Phenotype> m_phenotypes = 
      new List<Phenotype>(PopulationSize);
    private readonly List<Entity> m_entities = new List<Entity>(PopulationSize);
    private readonly float[] m_scores = new float[PopulationSize];

    private Phenotype m_championPhenotype;
    private Entity m_championEntity;
    

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
//       m_championPhenotype = null;
//       m_championEntity = null;
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
        var bestIdx = 0;
        var bestVal = 0f;
        for (var i = 0; i < m_scores.Length; i++)
        {
          if (m_scores[i] > bestVal)
          {
            bestIdx = i;
            bestVal = m_scores[i];
          }
        }

        if (m_championEntity == null || 
          bestVal > m_championEntity.DistanceTraveled)
        {
          m_championPhenotype = m_phenotypes[bestIdx];
        }
        m_championEntity = new Entity(
          -1, m_championPhenotype.ToDefinition(), m_physicsManager);
        m_championEntity.Type = EntityType.Champion;
        Generate();
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
