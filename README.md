# Genetic Cars #
A C# version of the "genetic algorithm car creator" app, such as [github.com/red42/HTML5_Genetic_Cars](github.com/red42/HTML5_Genetic_Cars).

Randomly generates a track and uses a genetic algorithm to find a car capable of crossing the finish line of the track.  Includes the ability to control most aspects of the genetic algorithm through settings and script files.

Purple cars are randomly generated.  Blue cars are identical clones of the best cars from the previous generation.  Red cars are created by crossing two members of the previous generation.  The transparent green car is a duplicate of the champion, the car that has made it the farthest out of all generations so far.

# Scripting: #

Lua scripts can be used to replace the crossover and mutation functions in genetic algorithm.  All scripts should be located in the scripts directory.  A script must provide two global functions:

1. **CrossOver(genomeA:string, genomeB:string)->string** - Takes two parent genome bit strings and combines them to form a child genome bit string, which is returned. 
2. **Mutate(genome:string)->string** - Takes a genome bit string and performs a random modification to it, returning the modified string.

Three global variables are provided to use in the functions:

1. **GenomeLength:** The number of bits in the genome.  The parameters to CrossOver and Mutate are of this length, and the string they return must be of this length.
2. **RNG:** An instance of [Random](https://msdn.microsoft.com/en-us/library/system.random%28v=vs.110%29.aspx) that should be used for all random number generation.
3. **Log:** An instance of [ILog](http://logging.apache.org/log4net/release/sdk/log4net.ILogMembers.html) that can be used to log information in the script functions.

Crossover and mutation default to using C# functions unless you load a script through the UI.  The [demo.lua](scripts/demo.lua) example provides a lua implementation of the default crossover and mutate functions. 

# Settings: #

The following settings are available in app.config.

* **MutationRate** - How often a mutation will occur after doing a crossover.
* **NumClones** - How many of the best cars are cloned with no changes when creating a new generation.
* **NumRandom** - How many cars will be randomly regenerated when creating a new generation.
* **PopulationSize** - The number of cars in each generation.  Larger numbers significantly impact performance.
* **BreedingPopulationPercent** - The percentage of the population that is used during crossover.  Must be between 0 and 1, and 
* **NumTrackPieces** - The number pieces generated for the track.  Each piece is 3 meters in length.
* **MinTrackAngle / MaxTrackAngle** - Determines the range of angles possible for each piece of track.  Setting these to higher values will make a much more difficult track.
* **CarLowSpeedThreshold** - Cars moving slower than this speed (in m/s) will lose health and eventually die.
* **NumBodyPoints** - The number of points used to make up the body polygon.  More than 32 can start to hurt performance and may not draw correctly.
* **MinBodyPointDistance / MaxBodyPointDistance** - Distance in meters from the body center point.  All points used to create the car body will fall within this range.
* **MinBodyDensity / MaxBodyDensity** - The range of possible body densities, in kg/m^3, used to determine the body's mass.
* **MinWheelDensity / MaxWheelDensity** - Same is above, but for the wheels.
* **MinWheelRadius / MaxWheelRadius** - The range of possible wheel radii.
* **MinWheelSpeed / MaxWheelSpeed** - The range of possible speeds in degrees / second for the car's wheels.  Not that this is the *maximum* speed of the wheel.  If the wheel does not have sufficient torque it will not be able to reach its max speed.
* **MinWheelTorque / MaxWheelTorque** - The range of possible torque outputs for the car wheels in N*m.

# Genome: #

The genome for a car is expressed as a bit string.  A car with *N* body points and *M* wheels has the following genome.

* **Body:**
 * **Points:** N*8 bits representing the points of the car's body.  Each  value is a byte, expressing the point's distance from the body center as a percentage of the min/max body point distance settings.  The points are distributed evenly around a center point, spaced 360/N degrees apart.
 * **Density:** 8 bits representing the density of the body as a percentage of the min/max density in the settings.
* ***M* Wheel Entries:**
 * **Attachment Point:** 8 bits, used to map to a point on the body where the wheel will attach.
 * **Radius:** 8 bits, representing the wheel radius as a percentage of the min/max radius settings.
 * **Density:** 8 bits, representing the wheel density as a percentage of the min/max wheel density settings.
 * **Speed:** 8 bits representing the wheel speed as a percentage of the min/max wheel speed settings.
 * **Torque:** 8 bits representing the wheel's maximum torque as a percentage of the min/max wheel torque settings.
