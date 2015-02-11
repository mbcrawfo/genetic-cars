using System.Diagnostics;
using FarseerPhysics.Dynamics;

namespace Genetic_Cars
{
  /// <summary>
  /// Interface to the system that manages physics.
  /// </summary>
  abstract class PhysicsManager
  {
    /// <summary>
    /// Handles the PostStep event.
    /// </summary>
    /// <param name="deltaTime">
    /// The time in seconds that will be simulated in the physics step.
    /// </param>
    public delegate void PreStepHandler(float deltaTime);

    /// <summary>
    /// Handles the PreStep event.
    /// </summary>
    /// <param name="deltaTime">
    /// The time in seconds that was simulated in the physics step.
    /// </param>
    public delegate void PostStepHandler(float deltaTime);

    /// <summary>
    /// Event fires before the physics simulation step.
    /// </summary>
    public event PreStepHandler PreStep;

    /// <summary>
    /// Event fires after the physics simulation step.
    /// </summary>
    public event PostStepHandler PostStep;

    /// <summary>
    /// The physics world.
    /// </summary>
    public World World { get; protected set; }

    /// <summary>
    /// Steps the physics simulation by the specified time.
    /// </summary>
    /// <param name="deltaTime"></param>
    protected void StepWorld(float deltaTime)
    {
      Debug.Assert(World != null);

      OnPostStep(deltaTime);
      World.Step(deltaTime);
      World.ClearForces();
      OnPostStep(deltaTime);
    }

    /// <summary>
    /// Fires the PreStep event.
    /// </summary>
    /// <param name="deltaTime"></param>
    protected virtual void OnPreStep(float deltaTime)
    {
      if (PreStep != null)
      {
        PreStep(deltaTime);
      }
    }

    /// <summary>
    /// Fires the PostStep event.
    /// </summary>
    /// <param name="deltaTime"></param>
    protected virtual void OnPostStep(float deltaTime)
    {
      if (PostStep != null)
      {
        PostStep(deltaTime);
      }
    }
  }
}
