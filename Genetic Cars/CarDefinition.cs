using System;
using System.Text;
using Genetic_Cars.Properties;
using log4net;

namespace Genetic_Cars
{
  /// <summary>
  /// Holds the information defining how a car will be constructed.
  /// </summary>
  /// <remarks>
  /// Unless otherwise specified, units used are as follows.
  /// Length: meters
  /// Density: kg/m^3 (all bodies have a depth of 1m)
  /// Torque: newton meters
  /// Rotational Speed: degrees per second
  /// </remarks>
  sealed class CarDefinition
  {
    /// <summary>
    /// The number of points that make up the car body polygon.
    /// </summary>
    public static readonly int NumBodyPoints = 
      Settings.Default.NumBodyPoints;

    /// <summary>
    /// The number of wheels on the car.
    /// </summary>
    public static readonly int NumWheels = 
      Settings.Default.NumWheels;
    
    /// <summary>
    /// The shortest distance a point can be from the center of the body.
    /// </summary>
    public static readonly float MinBodyPointDistance =
      Settings.Default.MinBodyPointDistance;

    /// <summary>
    /// The longest distance a point can be from the center of the body.
    /// </summary>
    public static readonly float MaxBodyPointDistance =
      Settings.Default.MaxBodyPointDistance;

    /// <summary>
    /// The smallest possible body density. 
    /// </summary>
    public static readonly float MinBodyDensity =
      Settings.Default.MinBodyDensity;

    /// <summary>
    /// The largest possible body density.
    /// </summary>
    public static readonly float MaxBodyDensity =
      Settings.Default.MaxBodyDensity;

    /// <summary>
    /// The smallest possible wheel radius
    /// </summary>
    public static readonly float MinWheelRadius =
      Settings.Default.MinWheelRadius;

    /// <summary>
    /// The largest possible wheel radius.
    /// </summary>
    public static readonly float MaxWheelRadius =
      Settings.Default.MaxWheelRadius;

    /// <summary>
    /// The smallest possible wheel density.
    /// </summary>
    public static readonly float MinWheelDensity =
      Settings.Default.MinWheelDensity;

    /// <summary>
    /// The largest possible wheel density.
    /// </summary>
    public static readonly float MaxWheelDensity =
      Settings.Default.MaxWheelDensity;

    public static readonly float MinWheelSpeed =
      Settings.Default.MinWheelSpeed;

    public static readonly float MaxWheelSpeed =
      Settings.Default.MaxWheelSpeed;

    public static readonly float MinWheelTorque =
      Settings.Default.MinWheelTorque;

    public static readonly float MaxWheelTorque =
      Settings.Default.MaxWheelTorque;

    /// <summary>
    /// The points that make up the polygons in the car's body, with 
    /// BodyPoints[0] starting at 0° and going counter clockwise evenly spaced. 
    /// Each value is a percentage [0,1] of the max distance from the center.
    /// </summary>
    public float[] BodyPoints { get; private set; }

    /// <summary>
    /// The density for the body, as a percentage [0,1] of the max density.
    /// </summary>
    public float BodyDensity { get; set; }
    
    /// <summary>
    /// The attachment points of the wheels.  Must match a point in 
    /// BodyPoints.
    /// </summary>
    public int[] WheelAttachment { get; private set; }
    
    /// <summary>
    /// The radius for each wheel, as a percentage [0,1] of the max radius.
    /// </summary>
    public float[] WheelRadius { get; private set; }

    /// <summary>
    /// The density for each wheel, as a percentage [0,1] of the max density.
    /// </summary>
    public float[] WheelDensity { get; private set; }

    /// <summary>
    /// The speed of the wheel, as a percentage [0,1] of the max speed.
    /// </summary>
    public float[] WheelSpeed { get; private set; }

    /// <summary>
    /// The torque of the wheel, as a percentage [0,1] of the max torque.
    /// </summary>
    public float[] WheelTorque { get; private set; }

    public CarDefinition()
    {
      BodyPoints = new float[NumBodyPoints];
      WheelAttachment = new int[NumWheels];
      WheelRadius = new float[NumWheels];
      WheelDensity = new float[NumWheels];
      WheelSpeed = new float[NumWheels];
      WheelTorque = new float[NumWheels];
    }

    /// <summary>
    /// Checks the properties of the definition for valid values.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Specifies the property that is out of range.
    /// </exception>
    public void Validate()
    {
      for (int i = 0; i < BodyPoints.Length; i++)
      {
        if (BodyPoints[i] < 0 || BodyPoints[i] > 1)
        {
          throw new ArgumentOutOfRangeException(
            "BodyPoints", "BodyPoints[" + i + "] out of range");
        }
      }

      if (BodyDensity < 0 || BodyDensity > 1)
      {
        throw new ArgumentOutOfRangeException("BodyDensity");
      }

      for (int i = 0; i < WheelAttachment.Length; i++)
      {
        if (WheelAttachment[i] < 0 || WheelAttachment[i] >= NumBodyPoints)
        {
          throw new ArgumentOutOfRangeException(
            "WheelAttachment",
            String.Format("WheelAttachment[{0}] out of range", i)
            );
        }
        if (WheelRadius[i] < 0 || WheelRadius[i] > 1)
        {
          throw new ArgumentOutOfRangeException(
            "WheelRadius",
            String.Format("WheelRadius[{0}] out of range", i)
            );
        }
        if (WheelDensity[i] < 0 || WheelDensity[i] > 1)
        {
          throw new ArgumentOutOfRangeException(
            "WheelDensity",
            String.Format("WheelDensity[{0}] out of range", i)
            );
        }
        if (WheelSpeed[i] < 0 || WheelSpeed[i] > 1)
        {
          throw new ArgumentOutOfRangeException(
            "WheelSpeed",
            String.Format("WheelSpeed[{0}] out of range", i)
            );
        }

        if (WheelTorque[i] < 0 || WheelTorque[i] > 1)
        {
          throw new ArgumentOutOfRangeException(
            "WheelTorque",
            String.Format("WheelTorque[{0}] out of range", i)
            );
        }
      }
    }

    /// <summary>
    /// Calculates the position of the requested body point.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public float CalcBodyPoint(int i)
    {
      if (i < 0 || i > BodyPoints.Length)
      {
        throw new ArgumentOutOfRangeException("i");
      }
      return (BodyPoints[i] * (MaxBodyPointDistance - MinBodyPointDistance)) +
             MinBodyPointDistance;
    }

    /// <summary>
    /// Calculates the density of the body.
    /// </summary>
    /// <returns></returns>
    public float CalcBodyDensity()
    {
      return (BodyDensity * (MaxBodyDensity - MinBodyDensity)) + MinBodyDensity;
    }

    /// <summary>
    /// Calculates the radius of the requested wheel.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public float CalcWheelRadius(int i)
    {
      if (i < 0 || i > WheelRadius.Length)
      {
        throw new ArgumentOutOfRangeException("i");
      }
      return (WheelRadius[i] * (MaxWheelRadius - MinWheelRadius)) + 
        MinWheelRadius;
    }

    /// <summary>
    /// Calculates the density of the requested wheel.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public float CalcWheelDensity(int i)
    {
      if (i < 0 || i > WheelDensity.Length)
      {
        throw new ArgumentOutOfRangeException("i");
      }
      return (WheelDensity[i] * (MaxWheelDensity - MinWheelDensity)) +
             MinWheelDensity;
    }

    /// <summary>
    /// Calculates the speed of the requested wheel.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public float CalcWheelSpeed(int i)
    {
      if (i < 0 || i > WheelDensity.Length)
      {
        throw new ArgumentOutOfRangeException("i");
      }
      return (WheelSpeed[i] * (MaxWheelSpeed - MinWheelSpeed)) + MinWheelSpeed;
    }

    /// <summary>
    /// Calculates the torque of the requested wheel.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public float CalcWheelTorque(int i)
    {
      if (i < 0 || i > WheelDensity.Length) 
      {
        throw new ArgumentOutOfRangeException("i");
      }
      return (WheelTorque[i] * (MaxWheelTorque - MinWheelTorque)) + MinWheelTorque;
    }

    /// <summary>
    /// Dumps the details of the definition out to a logger.
    /// </summary>
    /// <param name="log"></param>
    public void DumpToLog(ILog log)
    {
      if (log == null)
      {
        throw new ArgumentNullException("log");
      }

      StringBuilder sb = new StringBuilder();
      
      sb.AppendFormat("BodyPoints[{0}]={{", BodyPoints.Length);
      foreach (var point in BodyPoints)
      {
        sb.AppendFormat("{0:F2}, ", point);
      }
      sb.Remove(sb.Length - 2, 2);
      sb.Append("}");
      log.Debug(sb.ToString());
      sb.Clear();

      sb.Append("BodyPointsCalcd={");
      for (var i = 0; i < BodyPoints.Length; i++)
      {
        sb.AppendFormat("{0:F2}, ", CalcBodyPoint(i));
      }
      sb.Remove(sb.Length - 2, 2);
      sb.Append("}");
      log.Debug(sb.ToString());
      sb.Clear();

      log.DebugFormat("BodyDensity={0:F2}", BodyDensity);

      for (var i = 0; i < NumWheels; i++)
      {
        sb.AppendFormat(
          "WheelData[{0}]={{Attach={1}, Radius={2:F2}, Density={3:F2}, " +
          "Speed={4:F2}, Torque={5:F2}",
          i, WheelAttachment[i], WheelRadius[i], WheelDensity[i],
          WheelSpeed[i], WheelTorque[i]
          );
        sb.Append("}");
        log.Debug(sb.ToString());
        sb.Clear();

        sb.AppendFormat(
          "WheelStats[{0}]={{Radius={1:F2}, Speed={2:F2}, Torque={3:F2}",
          i, CalcWheelRadius(i), CalcWheelSpeed(i), CalcWheelTorque(i)
          );
        sb.Append("}");
        log.Debug(sb.ToString());
        sb.Clear();
      }
    }
  }
}
