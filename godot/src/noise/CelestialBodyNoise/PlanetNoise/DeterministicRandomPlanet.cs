using Godot;
using System;

[Tool]
public partial class DeterministicRandomPlanet : RandomCelestialBodyNoise
{
    private const int MAX_OCTAVES = 8;
    private const float MAX_AMPLITUDE = 20.0f;
    private Random random;

    protected override void RandomizeParameters(CelestialBodyParameters param)
    {
        GD.Print("Planet seed: " + param.Seed);

        // Set seed of Random to system-seed to make deterministic
        random = new Random(param.Seed);

        param.Width = Resolution + 2;
        param.Height = Resolution + 2;
        param.Depth = Resolution + 2;
        param.Size = Resolution + 2;

        param.Octaves = random.Next(4, MAX_OCTAVES);

        // The latter decides which range the one directly underneath can be
        // e.g. amplitdue < 4 then frequency can be within 0-16
        //      amplitude => 4 then frequency can be within 0-4

        param.Amplitude = Math.Max(5.0f, (float)Math.Round(random.NextSingle(), 2) * MAX_AMPLITUDE);

        //               amplitude 1-4, freq. 0-16                                             amplitude 1-20, freq. 0-4
        param.Frequency = (param.Amplitude < 4) ? (float)Math.Round(random.NextSingle(), 2) * 12.0f : (float)Math.Round(random.NextDouble(), 2) * 2.0f;


        if (param.Frequency < 4) param.Lacunarity = (float)Math.Round(random.NextSingle(), 2) * 4.0f;  // amplitude 1-20, freq. 0-4, lac. 0-4
        else param.Lacunarity = (float)Math.Round(random.NextDouble(), 2) * 8.0f;                // amplitdue 1-4, freq. 0-16, lac. 0-8

        param.Persistence = Math.Max(0.1f, Math.Min(0.25f, (float)Math.Round(random.NextSingle(), 2)));  // persistence 0.1-0.25 always
    }
}
