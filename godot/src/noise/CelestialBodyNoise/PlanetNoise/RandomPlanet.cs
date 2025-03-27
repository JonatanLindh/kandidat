using Godot;
using System;

[Tool]
public partial class RandomPlanet : RandomCelestialBody
{
    private const int MAX_RADIUS = 64;
    private const int MAX_WIDTH = 2 * MAX_RADIUS;
    private const int MAX_HEIGHT = 2 * MAX_RADIUS;
    private const int MAX_DEPTH = 2 * MAX_RADIUS;
    private const int MAX_SIZE = 2 * MAX_RADIUS;

    private const int MAX_OCTAVES = 8;
    private const float MAX_AMPLITUDE = 20.0f;
    private const float MAX_FREQUENCY = 2.0f;
    private const float MAX_LACUNARITY = 4.0f;
    private const float MAX_PERSISTENCE = 8.0f;

    private int radius;
    private int width;
    private int height;
    private int depth;
    private int size;

    private int octaves;
    private int seed;
    private float amplitude;
    private float frequency;
    private float persistence;
    private float lacunarity;


    public RandomPlanet()
    {
        RandomizePlanet();
    }

    private void RandomizePlanet()
    {
        Random random = new Random();
     
        this.radius = random.Next(MAX_RADIUS / 2, MAX_RADIUS);
        this.width = 2 * radius;
        this.height = 2 * radius;
        this.depth = 2 * radius;
        this.size = 2 * radius;

        this.octaves = random.Next(4, MAX_OCTAVES);
        this.seed = random.Next();

        // The latter decides which range the one directly underneath can be
        // e.g. amplitdue < 4 then frequency can be within 0-16
        //      amplitude => 4 then frequency can be within 0-4

        this.amplitude = Math.Max(1.0f, (float)Math.Round(random.NextDouble(), 2) * MAX_AMPLITUDE);

        if(amplitude < 4) this.frequency = (float)Math.Round(random.NextDouble(), 2) * 16.0f;   // amplitude 1-4, freq. 0-16
        else this.frequency = (float)Math.Round(random.NextDouble(), 2) * 4.0f;                 // amplitude 1-20, freq. 0-4

        if (frequency < 4) this.lacunarity = (float)Math.Round(random.NextDouble(), 2) * 4.0f;  // amplitude 1-20, freq. 0-4, lac. 0-4
        else this.lacunarity = (float)Math.Round(random.NextDouble(), 2) * 8.0f;                // amplitdue 1-4, freq. 0-16, lac. 0-8

        this.persistence = Math.Max(0.1f, Math.Min(0.25f, (float)Math.Round(random.NextDouble(), 2)));  // persistence 0.1-0.25 always
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

    public int GetSize()
    {
        return size;
    }

    public int GetOctaves()
    {
        return octaves;
    }
    public int GetSeed()
    {
        return seed;
    }

    public float GetAmplitude()
    {
        return amplitude;
    }

    public float GetFrequency()
    {
        return frequency;
    }
    public float GetPersistence()
    {
        return persistence;
    }

    public float GetLacunarity()
    {
        return lacunarity;
    }
}
