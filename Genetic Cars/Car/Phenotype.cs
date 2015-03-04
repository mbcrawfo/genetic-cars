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
    public static readonly int GenomeLength =
       BodySectionLength + (Definition.NumWheels * WheelSectionLength);

    /// <summary>
    /// Applies a mutation to the genome. Should use Phenotype.Random as the 
    /// RNG for the mutation.
    /// </summary>
    /// <param name="genome">
    /// The genome string.
    /// </param>
    /// <returns>
    /// The new, mutated genome string.
    /// </returns>
    public delegate string GenomeMutator(string genome);

    /// <summary>
    /// Takes two parent genome strings and produces a child genome string.  
    /// Should use Phonetype.Random as the RNG for the mutation.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public delegate string GenomeCrossover(string a, string b);

    /// <summary>
    /// The mutation strategy for the phenotype.  Must be set before any 
    /// cars are constructed.
    /// </summary>
    public static GenomeMutator MutateStrategy;

    /// <summary>
    /// The crossover strategy for the phenotype.  Must be set before any 
    /// cars are constructed.
    /// </summary>
    public static GenomeCrossover CrossoverStrategy;

    /// <summary>
    /// The RNG used for all actions in this class.
    /// </summary>
    public static Random Random { get; set; }

    public static string DefaultMutator(string genome)
    {
      StringBuilder sb = new StringBuilder(genome);
      var idx = Random.Next(sb.Length);
      if (sb[idx] == '0')
      {
        sb[idx] = '1';
      }
      else
      {
        sb[idx] = '0';
      }
      return sb.ToString();
    }

    public static string DefaultCrossOver(string a, string b)
    {
      StringBuilder sb = new StringBuilder(GenomeLength);
      var parent = Random.NextDouble() < 0.5 ? a : b;

      for (var i = 0; i < GenomeLength; i++)
      {
        sb.Append(parent[i]);
        if (Random.NextDouble() < 0.4)
        {
          parent = parent == a ? b : a;
        }
      }

      return sb.ToString();
    }

    /// <summary>
    /// Generates a new Phenotype by crossing the two parents.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Phenotype CrossOver(Phenotype a, Phenotype b)
    {
      Debug.Assert(CrossoverStrategy != null);
      return new Phenotype(CrossoverStrategy(a.m_genome, b.m_genome));
    }

    private string m_genome;

    /// <summary>
    /// Creates and randomizes the phenotype.
    /// </summary>
    public Phenotype()
    {
      Debug.Assert(Random != null);

      StringBuilder sb = new StringBuilder();
      for (var i = 0; i < GenomeLength; i++)
      {
        if (Random.NextDouble() < 0.5)
        {
          sb.Append('0');
        }
        else
        {
          sb.Append('1');
        }
      }

      m_genome = sb.ToString();
    }

    /// <summary>
    /// Creates a phenotype using a genome string.
    /// </summary>
    /// <param name="genome"></param>
    public Phenotype(string genome)
    {
      if (genome == null)
      {
        throw  new ArgumentNullException("genome");
      }
      if (genome.Length != GenomeLength)
      {
        throw new ArgumentOutOfRangeException("genome",
          "length is incorrect");
      }
      if (!genome.All(c => c == '0' || c == '1'))
      {
        throw new ArgumentOutOfRangeException("genome",
          "genome is not binary");
      }

      m_genome = genome;
    }

    public int Id { get; set; }

    public override string ToString()
    {
      return m_genome;
    }
    
    /// <summary>
    /// Applies a mutation to this individual.
    /// </summary>
    public void Mutate()
    {
      Debug.Assert(MutateStrategy != null);
      m_genome = MutateStrategy(m_genome);

      if (m_genome == null)
      {
        Log.Error("Genome null after mutation");
        throw new InvalidOperationException("Invalid mutation");
      }
      if (m_genome.Length != GenomeLength)
      {
        Log.ErrorFormat("Expected genome length {0} after mutation, got {1}",
          GenomeLength, m_genome.Length);
        throw new InvalidOperationException("Invalid mutation");
      }
      if (!m_genome.All(c => c == '0' || c == '1'))
      {
        Log.Error("Genome is not binary after mutation");
        throw new InvalidOperationException("Invalid mutation");
      }
    }
    
    /// <summary>
    /// Converts the genome to a Definition that can be used to create a 
    /// Entity.
    /// </summary>
    /// <returns></returns>
    public Definition ToDefinition()
    {
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

      var offset = i * 8;
      var str = m_genome.Substring(offset, 8);
      return Convert.ToByte(str, 2) / 255f;
    }

    private float GetBodyDensity()
    {
      var offset = Definition.NumBodyPoints * 8;
      var str = m_genome.Substring(offset, 8);
      return Convert.ToByte(str, 2) / 255f;
    }

    private int GetWheelAttachment(int i)
    {
      if (i < 0 || i >= Definition.NumWheels)
      {
        throw new ArgumentOutOfRangeException("i");
      }
      
      var offset = BodySectionLength + (i * WheelSectionLength);
      var str = m_genome.Substring(offset, 8);
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

      var offset = BodySectionLength + 
        ((i * WheelSectionLength) + 8);
      var str = m_genome.Substring(offset, 8);
      return Convert.ToByte(str, 2) / 255f;
    }

    private float GetWheelDensity(int i)
    {
      if (i < 0 || i >= Definition.NumWheels)
      {
        throw new ArgumentOutOfRangeException("i");
      }

      var offset = BodySectionLength + 
        ((i * WheelSectionLength) + 16);
      var str = m_genome.Substring(offset, 8);
      return Convert.ToByte(str, 2) / 255f;
    }

    private float GetWheelSpeed(int i)
    {
      if (i < 0 || i >= Definition.NumWheels)
      {
        throw new ArgumentOutOfRangeException("i");
      }

      var offset = BodySectionLength + 
        ((i * WheelSectionLength) + 24);
      var str = m_genome.Substring(offset, 8);
      return Convert.ToByte(str, 2) / 255f;
    }

    private float GetWheelTorque(int i)
    {
      if (i < 0 || i >= Definition.NumWheels)
      {
        throw new ArgumentOutOfRangeException("i");
      }

      var offset = BodySectionLength + 
        ((i * WheelSectionLength) + 32);
      var str = m_genome.Substring(offset, 8);
      return Convert.ToByte(str, 2) / 255f;
    }
  }
}
