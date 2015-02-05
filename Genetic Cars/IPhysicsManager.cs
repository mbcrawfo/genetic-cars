using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;

namespace Genetic_Cars
{
  interface IPhysicsManager
  {
    event EventHandler<float> PreStep;

    event EventHandler<float> PostStep;

    World World { get; }
  }
}
