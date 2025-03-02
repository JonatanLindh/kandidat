using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// A class for creating different types of noise to be used when generating planets
/// </summary>
public partial class PlanetNoise : Node, CelestialBodyNoise
{
    [Export] private Resource PlanetNoiseResource;

    public float[,,] GetNoise()
    {
        if (PlanetNoiseResource is Planet planet)
        {
            GD.Print("GETNOISE!!!!!!!! YAY =D");
            int diameter = 2 * planet.GetRadius();
            float[,,] points = new float[diameter, diameter, diameter];

            Vector3 centerPoint = new Vector3I(diameter, diameter, diameter) / 2;

            //Random random = new Random();
            //Vector3 offset = new Vector3(random.Next(diameter), random.Next(diameter), random.Next(diameter));

            // creates a cube of points
            for (var x = 0; x < diameter; x++)
            {
                for (var y = 0; y < diameter; y++)
                {
                    for (var z = 0; z < diameter; z++)
                    {
                        Vector3 currentPosition = new Vector3I(x, y, z);
                        float distanceToCenter = (centerPoint - currentPosition).Length();

                        // see if the point inside or outside the sphere
                        if (distanceToCenter < planet.GetRadius())
                        {
                            // noise-value between 0-1 - will be closer to 1 when the point is close to the surface 
                            float noiseValue = planet.GetNoise3Dv(currentPosition) * (distanceToCenter / planet.GetRadius());

                            points[x, y, z] = noiseValue;
                        }
                        else

                        {
                            // if not inside the sphere, just set noise-value to -1 so discard it later
                            points[x, y, z] = -1;
                        }


                    }
                }
            }

            return points;

        }

        return null;
    }

}
