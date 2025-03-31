using Godot;
using System;
using System.Threading.Tasks;

[Tool]
public partial class RandomCelestialBodyNoise : Node, CelestialBodyNoise, ModifiableCelestialBody
{
    [Export] private Node CelestialBody;
    private CelestialBodyNoise celestialBodyNoise;
    private ModifiableCelestialBody modifiableBody;

    private const int RADIUS = 64;
    private const int WIDTH = 2 * RADIUS + 2;
    private const int HEIGHT = 2 * RADIUS + 2;
    private const int DEPTH = 2 * RADIUS + 2;
    private const int SIZE = 2 * RADIUS + 2;

    private const int MAX_OCTAVES = 8;
    private const float MAX_AMPLITUDE = 20.0f;
    // private const float MAX_FREQUENCY = 2.0f;
    // private const float MAX_LACUNARITY = 4.0f;
    // private const float MAX_PERSISTENCE = 8.0f;

    private Random random = new Random();

    private void RandomizeParameters()
    {
        SetRadius(RADIUS);
        SetWidth(WIDTH);
        SetHeight(HEIGHT);
        SetDepth(DEPTH);
        SetSize(SIZE);

        SetOctaves(random.Next(4, MAX_OCTAVES));
        SetSeed(random.Next());

        // The latter decides which range the one directly underneath can be
        // e.g. amplitdue < 4 then frequency can be within 0-16
        //      amplitude => 4 then frequency can be within 0-4

        float amplitude = Math.Max(5.0f, (float)Math.Round(random.NextSingle(), 2) * MAX_AMPLITUDE);
        SetAmplitude(amplitude);

         //               amplitude 1-4, freq. 0-16                                             amplitude 1-20, freq. 0-4
        float frequency = (amplitude < 4) ? (float)Math.Round(random.NextSingle(), 2) * 12.0f : (float)Math.Round(random.NextDouble(), 2) * 2.0f;
        SetFrequency(frequency);


        if (frequency < 4) SetLacunarity((float)Math.Round(random.NextSingle(), 2) * 4.0f);  // amplitude 1-20, freq. 0-4, lac. 0-4
        else SetLacunarity((float)Math.Round(random.NextDouble(), 2) * 8.0f);                // amplitdue 1-4, freq. 0-16, lac. 0-8

        SetPersistence(Math.Max(0.1f, Math.Min(0.25f, (float)Math.Round(random.NextSingle(), 2))));  // persistence 0.1-0.25 always
    }

    public float[,,] GetNoise()
    {
        celestialBodyNoise = CelestialBody as CelestialBodyNoise;
        if (celestialBodyNoise is not CelestialBodyNoise)
        {
            GD.PrintErr("CelestialBody needs to implement CelestialBodyNoise");
        }

        modifiableBody = CelestialBody as ModifiableCelestialBody;
        if(modifiableBody is not ModifiableCelestialBody)
        {
            GD.PrintErr("CelestialBody needs to implement ModifiableCelestialBody");
        }
        RandomizeParameters();
        return celestialBodyNoise.GetNoise();
    }

    public int GetRadius()
    {
        return celestialBodyNoise.GetRadius();
    }

    public void SetRadius(int radius)
    {
        modifiableBody.SetRadius(radius);
    }

    public void SetWidth(int width)
    {
        modifiableBody.SetWidth(width);
    }

    public void SetHeight(int height)
    {
        modifiableBody.SetHeight(height);
    }

    public void SetDepth(int depth)
    {
        modifiableBody.SetDepth(depth);
    }

    public void SetSize(int size)
    {
        modifiableBody.SetSize(size);
    }

    public void SetOctaves(int octaves)
    {
        modifiableBody.SetOctaves(octaves);
    }

    public void SetSeed(int seed)
    {
        modifiableBody.SetSeed(seed);
    }

    public void SetAmplitude(float amplitude)
    {
        modifiableBody.SetAmplitude(amplitude);
    }

    public void SetFrequency(float frequency)
    {
        modifiableBody.SetFrequency(frequency);
    }

    public void SetLacunarity(float lacunarity)
    {
        modifiableBody.SetLacunarity(lacunarity);
    }

    public void SetPersistence(float persistence)
    {
        modifiableBody.SetPersistence(persistence);
    }
}
