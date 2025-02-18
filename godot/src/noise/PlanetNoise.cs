using Godot;
using System;
using System.Collections.Generic;

public partial class PlanetNoise
{

    public List<Vector3> GetNoise()
    {
        return new List<Vector3>();
        //return CreatePlanetNoise();
    }

    private List<Vector3> CreatePlanetNoise()
    {
        List<Vector3> noise = new List<Vector3>();

        FastNoiseLite n = new FastNoiseLite();
        n.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

        int radius = 1;
        float theta = 2 * Mathf.Pi;
        float phi = Mathf.Pi;
        float stepSize = 1 / 8;

        for(float i = 0; i < theta; i += stepSize)
        {
            for(float j = 0; j < phi; j += stepSize)
            {
                for(float k = 0; k < radius; k += stepSize)
                {
                    noise.Add(new Vector3(radius * Mathf.Cos(i) * Mathf.Sin(j), radius * Mathf.Cos(j), radius * Mathf.Sin(theta) * Mathf.Sin(phi))); 

                }
            }
        }

        //TO GET NOISE: n.GetNoise3Dv()


        return noise;


    }

}
