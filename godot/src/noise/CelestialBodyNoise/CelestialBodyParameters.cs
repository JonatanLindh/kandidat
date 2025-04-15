using Godot;
using System;

public partial class CelestialBodyParameters
{
    private int radius;
    private int width;
    private int height;
    private int depth;
    private int size;

    private int octaves;
    private int seed;
    private float amplitude;
    private float frequency;
    private float lacunarity;
    private float persistence;
    private FastNoiseLite.NoiseTypeEnum noiseType;

    public int Radius { get => radius; set { radius = value; } }
    public int Width { get => width; set { width = value; } }
    public int Height { get => height; set { height = value; } }  
    public int Depth { get => depth; set { depth = value; } }
    public int Size { get => size; set { size = value; } }
    public int Octaves { get => octaves; set { octaves = value; } }
    public int Seed { get => seed; set { seed = value; } }
    public float Amplitude { get => amplitude; set { amplitude = value; } }
    public float Frequency { get => frequency; set { frequency = value; } }
    public float Lacunarity { get => lacunarity; set { lacunarity = value; } }
    public float Persistence { get => persistence; set { persistence = value; } }
    public FastNoiseLite.NoiseTypeEnum NoiseType { get => noiseType; set { noiseType = value; } }
}
