using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// A class for creating different types of noise to be used when generating planets
/// </summary>
public partial class PlanetNoise
{

    ///<summary>
    /// Returns 3d noise of a sphere to be used with marching cubes
    ///</summary>
    public float[,,] GetSphere(int diameter)
    {
        float[,,] points = new float[diameter, diameter, diameter];
        FastNoiseLite fastNoise = new FastNoiseLite();

        float radius = diameter / 2.0f;
        Vector3 centerPoint =  new Vector3I(diameter, diameter, diameter) / 2;

        Random random = new Random();
        Vector3 offset = new Vector3(random.Next(diameter), random.Next(diameter), random.Next(diameter));

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
                    if (distanceToCenter < radius)
                    {
                        // noise-value between 0-1 - will be closer to 1 when the point is close to the surface 
                        float noise = fastNoise.GetNoise3Dv(currentPosition + offset) * (distanceToCenter / radius);

                        points[x, y, z] = noise;
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

}
