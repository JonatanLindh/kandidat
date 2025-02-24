using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PlanetNoise : FastNoiseLite
{
    ///<summary>
    /// Returns 3d noise of a sphere to be used with marching cubes
    ///</summary>
    public float[,,] GetSphere()
    {
        FastNoiseLite fastNoise = new FastNoiseLite();
        int size = 128;
        float[,,] points = new float[size, size, size];

        Vector3 centerPoint =  new Vector3I(size, size, size) / 2;
        Random random = new Random();
        Vector3 offset = new Vector3(random.Next(size), random.Next(size), random.Next(size));

        // creates a cube of points
        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < size; y++)
            {
                for (var z = 0; z < size; z++)
                {
                    float radiusOfSphere = size / 2.0f;
                    Vector3 currentPosition = new Vector3I(x, y, z);
                    float distanceToCenter = (centerPoint - currentPosition).Length();

                    // see if the point inside or outside the sphere
                    if (distanceToCenter < radiusOfSphere)
                    {
                        // noise-value between 0-1 - will be closer to 1 when the point is close to the surface 
                        float noise = fastNoise.GetNoise3Dv(currentPosition + offset) * (distanceToCenter / radiusOfSphere);

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
