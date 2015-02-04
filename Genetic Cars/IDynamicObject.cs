using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Genetic_Cars
{
  /// <summary>
  /// Dynamic physics objects which must have their graphical components 
  /// synced to the physics engine.
  /// </summary>
  interface IDynamicObject
  {
    /// <summary>
    /// Syncs the positions of the object's graphic components to the 
    /// physics engine.
    /// </summary>
    void Sync();
  }
}
