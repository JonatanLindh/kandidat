using Godot;
using System;
using System.Threading.Tasks;

[Tool]
public partial class RandomCelestialBodyNoise : Node, CelestialBodyNoise
{
    [Export] private Node CelestialBody;
    private CelestialBodyNoise celestialBodyNoise;

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

    private Random random = new Random();

    private void RandomizeParameters()
    {
        int radius = random.Next(MAX_RADIUS / 2, MAX_RADIUS);
        SetWidth(2 * radius);
        SetHeight(2 * radius);
        SetDepth(2 * radius);
        SetSize(2 * radius);
        SetRadius(radius);

        SetOctaves(random.Next(4, MAX_OCTAVES));
        SetSeed(random.Next());

        // The latter decides which range the one directly underneath can be
        // e.g. amplitdue < 4 then frequency can be within 0-16
        //      amplitude => 4 then frequency can be within 0-4

        float amplitude = Math.Max(1.0f, (float)Math.Round(random.NextDouble(), 2) * MAX_AMPLITUDE);
        SetAmplitude(amplitude);

         //               amplitude 1-4, freq. 0-16                                             amplitude 1-20, freq. 0-4
        float frequency = (amplitude < 4) ? (float)Math.Round(random.NextDouble(), 2) * 16.0f : (float)Math.Round(random.NextDouble(), 2) * 4.0f;
        SetFrequency(frequency);


        if (frequency < 4) SetLacunarity((float)Math.Round(random.NextDouble(), 2) * 4.0f);  // amplitude 1-20, freq. 0-4, lac. 0-4
        else SetLacunarity((float)Math.Round(random.NextDouble(), 2) * 8.0f);                // amplitdue 1-4, freq. 0-16, lac. 0-8

        SetPersistence(Math.Max(0.1f, Math.Min(0.25f, (float)Math.Round(random.NextDouble(), 2))));  // persistence 0.1-0.25 always
    }

    public float[,,] GetNoise()
    {
        celestialBodyNoise = CelestialBody as CelestialBodyNoise;
        if (celestialBodyNoise is not CelestialBodyNoise)
        {
            GD.PrintErr("celestialBodyNoise is not CelestialBodyNoise");
        }
        RandomizeParameters();
        return celestialBodyNoise.GetNoise();
    }

    public void SetRadius(int radius)
    {
        celestialBodyNoise.SetRadius(radius);
    }

    public void SetWidth(int width)
    {
        celestialBodyNoise.SetWidth(width);
    }

    public void SetHeight(int height)
    {
        celestialBodyNoise.SetHeight(height);
    }

    public void SetDepth(int depth)
    {
        celestialBodyNoise.SetDepth(depth);
    }

    public void SetSize(int size)
    {
        celestialBodyNoise.SetSize(size);
    }

    public void SetOctaves(int octaves)
    {
        celestialBodyNoise.SetOctaves(octaves);
    }

    public void SetSeed(int seed)
    {
        celestialBodyNoise.SetSeed(seed);
    }

    public void SetAmplitude(float amplitude)
    {
        celestialBodyNoise.SetAmplitude(amplitude);
    }

    public void SetFrequency(float frequency)
    {
        celestialBodyNoise.SetFrequency(frequency);
    }

    public void SetLacunarity(float lacunarity)
    {
        celestialBodyNoise.SetLacunarity(lacunarity);
    }

    public void SetPersistence(float persistence)
    {
        celestialBodyNoise.SetPersistence(persistence);
    }
}
