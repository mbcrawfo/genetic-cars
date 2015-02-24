# Genetic Cars #
A C# clone of [github.com/red42/HTML5_Genetic_Cars](github.com/red42/HTML5_Genetic_Cars)

# Settings: #

The following settings are available in app.config.

* **MutationRate** - How often a mutation will occur after doing a crossover.
* **NumClones** - How many of the best cars are cloned with no changes when creating a new generation.
* **NumRandom** - How many cars will be randomly regenerated when creating a new generation.
* **PopulationSize** - The number of cars in each generation.  Larger numbers significantly impact performance.
* **NumTrackPieces** - The number pieces generated for the track.  Each piece is 3 meters in length.
* **CarLowSpeedThreshold** - Cars moving slower than this speed (in m/s) will lose health and eventually die.
* **NumBodyPoints** - The number of points used to make up the body polygon.  More than 32 can start to hurt performance and may not draw correctly.
* **MinBodyPointDistance / MaxBodyPointDistance** - Distance in meters from the body center point.  All points used to create the car body will fall within this range.
* **MinBodyDensity / MaxBodyDensity** - The range of possible body densities, in kg/m^3, used to determine the body's mass.  Value is a percentable between 0 and 1.
* **MinWheelDensity / MaxWheelDensity** - Same is above, but for the wheels.
* **MinWheelRadius / MaxWheelRadius** - The range of possible wheel radii.
* **MinWheelSpeed / MaxWheelSpeed** - The range of possible speeds in degrees / second for the car's wheels.  Not that this is the *maximum* speed of the wheel.  If the wheel does not have sufficient torque it will not be able to reach its max speed.
* **MinWheelTorque / MaxWheelTorque** - The range of possible torque outputs for the car wheels in N*m.