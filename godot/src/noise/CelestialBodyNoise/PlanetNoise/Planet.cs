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

    /// <summary>
    /// Returns the noise value from the given position <paramref name="pos"/>
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>
    /// a float value in the range [0, 1]
    /// </returns>
    public float GetNoise3Dv(Vector3 pos)
    {
        // FastNoiseLite.GetNoise returns a value between [-1, 1]. It feels more natural with [0, 1], hence the absolute value.
        // The distribution of values should still be the same
        return Mathf.Abs(noise.GetNoise3Dv(pos));
    }

    public int GetRadius()
    {
        return radius;
    }
}
