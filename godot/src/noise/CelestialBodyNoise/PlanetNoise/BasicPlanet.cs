using Godot;
using System;
using static Godot.XmlParser;
using System.Drawing;

[Tool]
public partial class BasicPlanet : Node, CelestialBodyNoise
{
    private CelestialBodyParameters param;
    private PlanetNoise planetNoise;
    private FastNoiseLite fastNoise;

    private CelestialBodyInput input;
    private CelestialBodyInput Input;

    public override void _Ready()
    {
        param = new CelestialBodyParameters();
        planetNoise = new PlanetNoise();
        fastNoise = new FastNoiseLite();

        Input = GetParent<CelestialBodyInput>();
        if (Input == null)
        {
            GD.PrintErr("Input is null");
        }
        input = Input as CelestialBodyInput;
    }

    public float[,,] GetNoise()
    {
        GetParameters();
        UpdateNoise();
        return planetNoise.CreateDataPoints(param, fastNoise, voxelSize: VoxelSize);
    }

    public float[,,] GetNoise(Vector3 offset)
    {
        GetParameters();
        UpdateNoise();
        return planetNoise.CreateDataPoints(param, fastNoise, offset, VoxelSize);
    }

    private void GetParameters()
    {
        param.Radius = input.GetRadius();
        param.Width = input.GetWidth();
        param.Height = input.GetHeight();
        param.Depth = input.GetDepth();

        param.Octaves = input.GetOctaves();
        param.Seed = input.GetSeed();
        param.Amplitude = input.GetAmplitude();
        param.Frequency = input.GetFrequency();
        param.Lacunarity = input.GetLacunarity();
        param.Persistence = input.GetPersistence();
        param.FalloffStrength = input.GetFalloffStrength();
        param.NoiseType = input.GetNoiseType();
    }

    private void UpdateNoise()
    {
        fastNoise.Seed = param.Seed;
        fastNoise.NoiseType = param.NoiseType;
    }

    public int GetRadius()
    {
        return param.Radius;
    }

    public int Resolution { get; set; }

    public float VoxelSize { get; set; } = 1f;
    
    public int Seed
    {
        get => param.Seed;
        set => param.Seed = value;
    }
}
