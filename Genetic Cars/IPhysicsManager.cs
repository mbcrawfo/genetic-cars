using System;
using FarseerPhysics.Dynamics;

namespace Genetic_Cars
{
  /// <summary>
  /// Interface to the system that manages physics.
  /// </summary>
  interface IPhysicsManager
  {
    /// <summary>
    /// Event fires before the physics simulation step.  The float param is the 
    /// amount of time that will be simulated in the physics step.
    /// </summary>
    event EventHandler<float> PreStep;

    /// <summary>
    /// Event fires after the physics simulation step.  The float param is the 
    /// amount of time that was simulated in the physics step.
    /// </summary>
    event EventHandler<float> PostStep;

    /// <summary>
    /// The physics world.
    /// </summary>
    World World { get; }
  }
}
