using Godot;
using System;
using System.Collections.Generic;

public partial class PlanetNoise
{

    public List<Vector3> GetNoise()
    {
        return CreatePlanetNoise();
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

        for(float i = 0; i < 5; i ++)
        {
            for(float j = 0; j < 5; j ++)
            {
                for(float k = 0; k < 5; k ++)
                {
                    noise.Add(new Vector3(i, j, k));
                    //noise.Add(new Vector3(radius * Mathf.Cos(i) * Mathf.Sin(j), radius * Mathf.Cos(j), radius * Mathf.Sin(theta) * Mathf.Sin(phi))); 

                }
            }
        }

        //TO GET NOISE: n.GetNoise3Dv()


        return noise;


    }

}
