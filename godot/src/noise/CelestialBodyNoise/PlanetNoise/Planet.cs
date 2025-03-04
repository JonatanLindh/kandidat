using Godot;
using System;

/// <summary>
/// Resource for a planet. To be used together with some noise type for planets such as PlanetNoise
/// </summary>
[GlobalClass]
public partial class Planet : Resource
{
    private FastNoiseLite _noise;
    [Export]
    private FastNoiseLite noise
    {
        get { return _noise; }
        set
        {
            _noise = value;
            _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
            _noise.Seed = new Random().Next();
        }
    }
    [Export] private int radius;


    public float GetNoise3Dv(Vector3 pos)
    {
        return noise.GetNoise3Dv(pos);
    }

    public int GetRadius()
    {
        return radius;
    }
}
