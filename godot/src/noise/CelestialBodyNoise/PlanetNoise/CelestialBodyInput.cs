using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

[Tool]
public partial class CelestialBodyInput : Node3D
{
    /*
    [Export] private Resource PlanetNoiseResource;
    Planet planet;

    public override void _Ready()
    {
        if (PlanetNoiseResource is not Planet)
        {
            planet = null;
        }
        else
        {
            planet = PlanetNoiseResource as Planet;
        }
    
    }*/

    [ExportCategory("Celestial body parameters")]
    private int radius = 32;
    [Export(PropertyHint.Range, "0,128,1")]
    private int Radius
    {
        get { return radius; }
        set { radius = value; }
    }


    [ExportCategory("Cube parameters")]
    [Export] private bool keepSame = true;

    private int width = 32;
    [Export(PropertyHint.Range, "0,128,1")]
    private int Width
    {
        get { return width; }
        set
        {
            if (keepSame) { width = value; height = value; depth = value; }
            else { width = value; }
        }
    }

    private int height = 32;
    [Export(PropertyHint.Range, "0,128,1")]
    private int Height
    {
        get { return height; }
        set
        {
            if (keepSame) { width = value; height = value; depth = value; }
            else { height = value; }
        }
    }

    private int depth = 32;
    [Export(PropertyHint.Range, "0,128,1")]
    private int Depth
    {
        get { return depth; }
        set
        {
            if (keepSame) { width = value; height = value; depth = value; }
            else { depth = value; }
        }
    }


    [ExportCategory("Noise parameters")]
    private int octaves = 1;
    [Export(PropertyHint.Range, "0,16,1")]
    private int Octaves
    {
        get { return octaves; }
        set { octaves = value; }
    }
    private float frequency = 1.0f;
    [Export(PropertyHint.Range, "0,32,00.1")]
    private float Frequency
    {
        get { return frequency; }
        set { frequency = value; }
    }
    private float amplitude = 10.0f;
    [Export(PropertyHint.Range, "0,32,00.1")]
    private float Amplitude
    {
        get { return amplitude; }
        set { amplitude = value; }
    }

    private float lacunarity = 1.0f;
    [Export(PropertyHint.Range, "0,16,0.01")]
    private float Lacunarity
    {
        get { return lacunarity; }
        set { lacunarity = value; }
    }

    private float persistence = 1.0f;
    [Export(PropertyHint.Range, "0,16,0.01")]
    private float Persistence
    {
        get { return persistence; }
        set { persistence = value; }
    }

    [Export] private FastNoiseLite.NoiseTypeEnum noiseType = FastNoiseLite.NoiseTypeEnum.Simplex;

    [Export] private int seed = 0;

    private bool randomizeSeed;
    [Export]
    public bool RandomizeSeed
    {
        get { return randomizeSeed; }
        set
        {
            randomizeSeed = !value;
            seed = new Random().Next();
        }
    }

    public int GetRadius()
    {
        return radius;
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public int GetDepth()
    {
        return depth;
    }

    public int GetOctaves()
    {
        return octaves;
    }

    public float GetFrequency()
    {
        return frequency;
    }

    public float GetAmplitude()
    {
        return amplitude;
    }

    public float GetLacunarity()
    {
        return lacunarity;
    }

    public float GetPersistence()
    {
        return persistence;
    }

    public FastNoiseLite.NoiseTypeEnum GetNoiseType()
    {
        return noiseType;
    }

    public int GetSeed()
    {
        return seed;
    }
}
