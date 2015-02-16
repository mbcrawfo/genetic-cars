using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace Genetic_Cars.Car
{
  sealed class Car : IDrawable, IDisposable
  {
    private bool m_disposed = false;

    private int m_id;
    private readonly PhysicsManager m_physicsManager;
    private Phenotype m_phenotype;
    private Definition m_definition;
    private Entity m_entity;

    public Car(PhysicsManager physicsManager, Phenotype phenotype = null)
    {
      if (physicsManager == null)
      {
        throw new ArgumentNullException("physicsManager");
      }

      m_physicsManager = physicsManager;
      m_phenotype = phenotype ?? new Phenotype();
      m_definition = m_phenotype.ToDefinition();
      m_entity = new Entity(m_definition, m_physicsManager);

      m_physicsManager.PostStep += PhysicsPostStep;
    }

    ~Car()
    {
      Dispose(false);
    }

    public int Id
    {
      get { return m_id;}
      set
      {
        m_id = value;
        if (m_phenotype != null)
        {
          m_phenotype.Id = m_id;
        }
        if (m_entity != null)
        {
          m_entity.Id = m_id;
        }
      }
    }

    /// <summary>
    /// The score of the car is the max forward distance it has traveled.
    /// </summary>
    public float Score { get; private set; }

    public void SetType(EntityType type)
    {
      if (m_entity != null)
      {
        m_entity.Type = type;
      }
    }

    public void Draw(RenderTarget target)
    {
      if (m_entity != null)
      {
        m_entity.Draw(target);
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
        if (m_entity != null)
        {
          m_entity.Dispose();
        }
      }

      m_physicsManager.PostStep -= PhysicsPostStep;

      m_disposed = true;
    }

    private void PhysicsPostStep(float deltaTime)
    {
      if (m_entity == null)
      {
        return;
      }

      Score = Math.Max(Score, m_entity.DistanceTraveled);
    }
  }
}
