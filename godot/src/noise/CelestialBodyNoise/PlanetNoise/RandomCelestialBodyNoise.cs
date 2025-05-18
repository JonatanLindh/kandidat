using Godot;
using System;
using System.Threading.Tasks;

[Tool]
public abstract partial class RandomCelestialBodyNoise : Node3D, CelestialBodyNoise, ModifiableCelestialBody
{
    [Export] private int radius = 0;
    [Export] private int seed = 0;
    [Export] private float falloffStrength = 8.0f;
 
    private CelestialBodyParameters param;
    private PlanetNoise planetNoise;
    private FastNoiseLite fastNoise;

    public override void _Ready()
    {
        this.param = new CelestialBodyParameters();
        param.Radius = radius;
        param.Seed = seed;
        param.FalloffStrength = falloffStrength;

        this.planetNoise = new PlanetNoise();
        this.fastNoise = new FastNoiseLite();
    }

    protected abstract void RandomizeParameters(CelestialBodyParameters param);

    public float[,,] GetNoise()
    {
        UpdateNoise();
        RandomizeParameters(param);
        return planetNoise.CreateDataPoints(param, fastNoise, voxelSize: VoxelSize);
    }

    public float[,,] GetNoise(Vector3 offset)
    {
        RandomizeParameters(param);
        return planetNoise.CreateDataPoints(param, fastNoise, offset, VoxelSize);
    }
    private void UpdateNoise()
    {
        fastNoise.Seed = param.Seed;
        fastNoise.NoiseType = param.NoiseType;
    }

    public int GetRadius()
    {
        return radius;
    }

    public int Resolution { get; set; }

    public float VoxelSize { get; set; } = 1f;

    public void SetRadius(int radius)
    {
        this.radius = radius;
    }

    public void SetSeed(int seed)
    {
        this.seed = seed;
    }
}
