using Godot;
using System;
using System.Collections.Generic;

public partial class PlanetNoise : FastNoiseLite
{
    ///<summary>
    ///<para>
    /// Returns a List of vector4 (x, y, z, n)
    /// </para>
    /// (x, y, z) is a point in the sphere and n is the noise-value for that point 
    ///</summary>
    public List<Vector4> GetNoisedSphere(Vector3 vector)
    {
        FastNoiseLite noise = new FastNoiseLite();
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

        List<Vector3> sphere = GetSphere();
        List<Vector4> sphereAndNoise = new List<Vector4>();

        foreach(Vector3 point in sphere)
        {
            float n = noise.GetNoise3Dv(point);
            sphereAndNoise.Add(new Vector4(point.X, point.Y, point.Z, n));
        }

        return sphereAndNoise;
    }

    public List<Vector3> GetSphere()
    {
        List<Vector3> noise = new List<Vector3>();

        // amount of layered spheres
        float depth = 4.0f;

        // for spherical coordinates
        float theta = 2.0f * Mathf.Pi;
        float phi = Mathf.Pi;
        float stepSize = 0.25f;
        for(float r = 1; r <= depth; r++)
        {
            for (float the = 0; the <= theta; the += stepSize)
            {
                for (float ph = 0; ph < phi; ph += stepSize)
                {
                    /* 
                     calculates the x,y,z position on the surface of the sphere with radius r.
                     r varies between [1, depth].
                          - this means that the number of spheres created is equal to depth.
                    */

                    float x = r * Mathf.Cos(the) * MathF.Sin(ph);
                    float y = r * Mathf.Cos(ph);
                    float z = r * Mathf.Sin(the) * Mathf.Sin(ph);
                    noise.Add(new Vector3(x, y, z));

                }
            }
     
        }

        return noise;
    }

}
