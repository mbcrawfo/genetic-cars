using System;

namespace Genetic_Cars
{
  /// <summary>
  /// Contains useful math functions.
  /// </summary>
  static class MathExtensions
  {
    private const double PiOver180 = Math.PI / 180d;

    /// <summary>
    /// Converts an angle in degrees to radians.
    /// </summary>
    /// <param name="degrees"></param>
    /// <returns></returns>
    public static double DegToRad(double degrees)
    {
      return degrees * PiOver180;
    }

    /// <summary>
    /// Converst an angle in radians to degrees.
    /// </summary>
    /// <param name="radians"></param>
    /// <returns></returns>
    public static double RadToDeg(double radians)
    {
      return radians / PiOver180;
    }
  }
}
