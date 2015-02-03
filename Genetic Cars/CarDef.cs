using System;
using Genetic_Cars.Properties;

namespace Genetic_Cars
{
  public sealed class CarDef
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

    /// <summary>
    /// The points that make up the polygons in the car's body, with 
    /// BodyPoints[0] starting at 0� and going counter clockwise evenly spaced. 
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
    /// The speed of the wheels, as a percentage [0,1] of the max speed.
    /// </summary>
    public float WheelSpeed { get; set; }

    public CarDef()
    {
      BodyPoints = new float[NumBodyPoints];
      WheelAttachment = new int[NumWheels];
      WheelRadius = new float[NumWheels];
      WheelDensity = new float[NumWheels];
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
            "BodyPoints", @"BodyPoints[" + i + @"] out of range");
        }
      }
      if (BodyDensity < 0 || BodyDensity > 1)
      {
        // ReSharper disable once LocalizableElement
        throw new ArgumentOutOfRangeException(
          "BodyDensity", "BodyDensity out of range");
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
      }
      for (int i = 0; i < WheelRadius.Length; i++)
      {
        if (WheelRadius[i] < 0 || WheelRadius[i] > 1)
        {
          throw new ArgumentOutOfRangeException(
            "WheelRadius",
            String.Format("WheelRadius[{0}] out of range", i)
            );
        }
      }
      for (int i = 0; i < WheelDensity.Length; i++)
      {
        if (WheelDensity[i] < 0 || WheelDensity[i] > 1)
        {
          throw new ArgumentOutOfRangeException(
            "WheelDensity",
            String.Format("WheelDensity[{0}] out of range", i)
            );
        }
      }
    }

    /// <summary>
    /// Calculates the position of the requested body point.
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public float CalcBodyPoint(int idx)
    {
      if (idx < 0 || idx > BodyPoints.Length)
      {
        throw new ArgumentOutOfRangeException("idx");
      }
      return (BodyPoints[idx] * (MaxBodyPointDistance - MinBodyPointDistance)) +
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
    /// <param name="idx"></param>
    /// <returns></returns>
    public float CalcWheelRadius(int idx)
    {
      if (idx < 0 || idx > WheelRadius.Length)
      {
        throw new ArgumentOutOfRangeException("idx");
      }
      return (WheelRadius[idx] * (MaxWheelRadius - MinWheelRadius)) + 
        MinWheelRadius;
    }

    /// <summary>
    /// Calculates the density of the requested wheel.
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public float CalcWheelDensity(int idx)
    {
      if (idx < 0 || idx > WheelDensity.Length)
      {
        throw new ArgumentOutOfRangeException("idx");
      }
      return (WheelDensity[idx] * (MaxWheelDensity - MinWheelDensity)) +
             MinWheelDensity;
    }
  }
}
