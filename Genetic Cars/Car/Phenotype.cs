using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;

namespace Genetic_Cars.Car
{
  /// <summary>
  /// Holds the genome for one car individual.
  /// </summary>
  sealed class Phenotype
  {
    private static readonly ILog Log = LogManager.GetLogger(
      MethodBase.GetCurrentMethod().DeclaringType);

    private const int WheelSectionLength =
      5 * 8;
    private static readonly int BodySectionLength =
      (Definition.NumBodyPoints * 8) + 8;
    private static readonly int GenomeLength =
       BodySectionLength + (Definition.NumWheels * WheelSectionLength);

    /// <summary>
    /// Applies a mutation to the genome.
    /// </summary>
    /// <param name="genome">
    /// The genome string.  The length may not be changed, and all the 
    /// characters in the string must be 0 or 1.
    /// </param>
    public delegate void GenomeMutator(StringBuilder genome);

    /// <summary>
    /// The mutation strategy for the phenotype.  Must be set before any 
    /// cars are constructed.
    /// </summary>
    public static GenomeMutator Mutator { get; set; }

    /// <summary>
    /// The RNG used for all actions in this class.
    /// </summary>
    public static Random Random { get; set; }

    private readonly StringBuilder m_genome = 
      new StringBuilder(new string('0', GenomeLength));

    public override string ToString()
    {
      return m_genome.ToString();
    }

    /// <summary>
    /// Randomizes the genome bit string.
    /// </summary>
    public void Randomize()
    {
      Debug.Assert(Random != null);
      Debug.Assert(m_genome.Length == GenomeLength);

      for (var i = 0; i < m_genome.Length; i++)
      {
        if (Random.NextDouble() < 0.5)
        {
          m_genome[i] = '0';
        }
        else
        {
          m_genome[i] = '1';
        }
      }
    }

    /// <summary>
    /// Applies a mutation to this individual.
    /// </summary>
    public void Mutate()
    {
      Debug.Assert(Mutator != null);
      Mutator(m_genome);
      Debug.Assert(m_genome.Length == GenomeLength);
      Debug.Assert(m_genome.ToString().All(c => c == '0' || c == '1'));
    }

    /// <summary>
    /// Converts the genome to a Definition that can be used to create a 
    /// Entity.
    /// </summary>
    /// <returns></returns>
    public Definition ToDefinition()
    {
      Debug.Assert(m_genome.Length == GenomeLength);

      var cd = new Definition();
      for (var i = 0; i < Definition.NumBodyPoints; i++)
      {
        cd.BodyPoints[i] = GetBodyPoint(i);
      }
      cd.BodyDensity = GetBodyDensity();
      for (var i = 0; i < Definition.NumWheels; i++)
      {
        cd.WheelAttachment[i] = GetWheelAttachment(i);
        cd.WheelRadius[i] = GetWheelRadius(i);
        cd.WheelDensity[i] = GetWheelDensity(i);
        cd.WheelSpeed[i] = GetWheelSpeed(i);
        cd.WheelTorque[i] = GetWheelTorque(i);
      }
      return cd;
    }

    private float GetBodyPoint(int i)
    {
      if (i < 0 || i >= Definition.NumBodyPoints)
      {
        throw new ArgumentOutOfRangeException("i");
      }
      Debug.Assert(m_genome.Length == GenomeLength);

      var offset = i * 8;
      var str = m_genome.ToString(offset, 8);
      return Convert.ToByte(str, 2) / 255f;
    }

    private float GetBodyDensity()
    {
      Debug.Assert(m_genome.Length == GenomeLength);
      var offset = Definition.NumBodyPoints * 8;
      var str = m_genome.ToString(offset, 8);
      return Convert.ToByte(str, 2) / 255f;
    }

    private int GetWheelAttachment(int i)
    {
      if (i < 0 || i >= Definition.NumWheels)
      {
        throw new ArgumentOutOfRangeException("i");
      }
      Debug.Assert(m_genome.Length == GenomeLength);

      var offset = BodySectionLength + (i * WheelSectionLength);
      var str = m_genome.ToString(offset, 8);
      // division by 256 is intentional so the percent is < 1
      var percent =  Convert.ToByte(str, 2) / 256f;
      return (int)(percent * Definition.NumBodyPoints);
    }

    private float GetWheelRadius(int i)
    {
      if (i < 0 || i >= Definition.NumWheels)
      {
        throw new ArgumentOutOfRangeException("i");
      }
      Debug.Assert(m_genome.Length == GenomeLength);

      var offset = BodySectionLength + 
        ((i * WheelSectionLength) + 8);
      var str = m_genome.ToString(offset, 8);
      return Convert.ToByte(str, 2) / 255f;
    }

    private float GetWheelDensity(int i)
    {
      if (i < 0 || i >= Definition.NumWheels)
      {
        throw new ArgumentOutOfRangeException("i");
      }
      Debug.Assert(m_genome.Length == GenomeLength);

      var offset = BodySectionLength + 
        ((i * WheelSectionLength) + 16);
      var str = m_genome.ToString(offset, 8);
      return Convert.ToByte(str, 2) / 255f;
    }

    private float GetWheelSpeed(int i)
    {
      if (i < 0 || i >= Definition.NumWheels)
      {
        throw new ArgumentOutOfRangeException("i");
      }
      Debug.Assert(m_genome.Length == GenomeLength);

      var offset = BodySectionLength + 
        ((i * WheelSectionLength) + 24);
      var str = m_genome.ToString(offset, 8);
      return Convert.ToByte(str, 2) / 255f;
    }

    private float GetWheelTorque(int i)
    {
      if (i < 0 || i >= Definition.NumWheels)
      {
        throw new ArgumentOutOfRangeException("i");
      }
      Debug.Assert(m_genome.Length == GenomeLength);

      var offset = BodySectionLength + 
        ((i * WheelSectionLength) + 32);
      var str = m_genome.ToString(offset, 8);
      return Convert.ToByte(str, 2) / 255f;
    }
  }
}
