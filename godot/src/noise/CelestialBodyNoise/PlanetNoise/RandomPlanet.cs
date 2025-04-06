using Godot;
using System;
[Tool]
public partial class RandomPlanet : RandomCelestialBodyNoise
{
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

    protected override void RandomizeParameters(CelestialBodyParameters param)
    {
        param.Width = GetRadius() * 2 + 2;
        param.Height = GetRadius() * 2 + 2;
        param.Depth = GetRadius() * 2 + 2;
        param.Size = GetRadius() * 2 + 2;

        param.Octaves = random.Next(4, MAX_OCTAVES);
        param.Seed = random.Next();

        // The latter decides which range the one directly underneath can be
        // e.g. amplitdue < 4 then frequency can be within 0-16
        //      amplitude => 4 then frequency can be within 0-4

        param.Amplitude =  Math.Max(5.0f, (float)Math.Round(random.NextSingle(), 2) * MAX_AMPLITUDE);

        //               amplitude 1-4, freq. 0-16                                             amplitude 1-20, freq. 0-4
        param.Frequency = (param.Amplitude < 4) ? (float)Math.Round(random.NextSingle(), 2) * 12.0f : (float)Math.Round(random.NextDouble(), 2) * 2.0f;


        if (param.Frequency < 4) param.Lacunarity = (float)Math.Round(random.NextSingle(), 2) * 4.0f;  // amplitude 1-20, freq. 0-4, lac. 0-4
        else param.Lacunarity = (float)Math.Round(random.NextDouble(), 2) * 8.0f;                // amplitdue 1-4, freq. 0-16, lac. 0-8

        param.Persistence = Math.Max(0.1f, Math.Min(0.25f, (float)Math.Round(random.NextSingle(), 2)));  // persistence 0.1-0.25 always
    }
}
