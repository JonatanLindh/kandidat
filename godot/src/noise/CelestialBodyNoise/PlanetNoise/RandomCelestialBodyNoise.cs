using Godot;
using System;
using System.Threading.Tasks;

[Tool]
public abstract partial class RandomCelestialBodyNoise : Node3D, CelestialBodyNoise, ModifiableCelestialBody
{
    [Export] private int radius = 0;
    [Export] private int seed = 0;
 
    private CelestialBodyParameters param;
    private PlanetNoise planetNoise;
    private FastNoiseLite fastNoise;

    public override void _Ready()
    {
        this.param = new CelestialBodyParameters();
        param.Radius = radius;
        param.Seed = seed;

        this.planetNoise = new PlanetNoise();
        this.fastNoise = new FastNoiseLite();
    }

    protected abstract void RandomizeParameters(CelestialBodyParameters param);

    public float[,,] GetNoise()
    {
        RandomizeParameters(param);
        return planetNoise.CreateDataPoints(param, fastNoise, voxelSize: VoxelSize);
    }

    public float[,,] GetNoise(Vector3 offset)
    {
        RandomizeParameters(param);
        return planetNoise.CreateDataPoints(param, fastNoise, offset, VoxelSize);
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
