using Godot;
using System;
using System.Collections.Generic;

[Tool]
[GlobalClass]
public partial class PlanetThemeGenerator : Resource
{
    [Export]
    public Gradient Gradient
    {
        get => gradient;
        set => gradient = value;
    }

    [Export]
    public Vector3 AtmosphereColor
    {
        get => _atmosphereColor;
        set => _atmosphereColor = value;
    }
    private Vector3 _atmosphereColor;

    private double _warmth = 0.5;
    public double Warmth
    {
        get => _warmth;
        set
        {
            _warmth = value;
            GeneratePair(); // Automatically regenerate the theme
        }
    }

    private Gradient gradient = new Gradient();
    private Vector3 atmosphereWavelengths;

    // Enums for clarity
    public enum ThemeNames
    {
        DESERT,
        EARTH,
        ICE,
        ALIEN,
        PINK,
        MARS
    }

    // Theme color palettes
    private List<List<Color>> planetThemes = new List<List<Color>>
    {
        // PINK 0
        new List<Color>
        {
            new Color(0.0f, 0.3f, 0.7f),
            new Color(0.5f, 0.4f, 0.1f),
            new Color(0.81f, 0.44f, 0.65f),
            new Color(0.86f, 0.63f, 0.47f),
            new Color(1.0f, 1.0f, 1.0f)
        },
        // EARTH-LIKE 1
        new List<Color>
        {
            new Color(0.0f, 0.3f, 0.7f),
            new Color(0.439f, 0.255f, 0.0f),
            new Color(0.035f, 0.31f, 0.0f),
            new Color(0.3f, 0.3f, 0.3f),
            new Color(1.0f, 1.0f, 1.0f)
        },
        // ALIEN 2
        new List<Color>
        {
            new Color(0.02f, 0.0f, 0.1f),
            new Color(0.1f, 0.0f, 0.2f),
            new Color(0.0f, 0.8f, 0.6f),
            new Color(0.3f, 0.1f, 0.5f),
            new Color(0.9f, 1.0f, 1.0f)
        },
        // DESERT 3
        new List<Color>
        {
            new Color(0.35f, 0.2f, 0.1f),
            new Color(0.7f, 0.3f, 0.1f),
            new Color(0.85f, 0.6f, 0.3f),
            new Color(0.95f, 0.8f, 0.5f),
            new Color(1.0f, 1.0f, 0.9f)
        },
        // ICE WORLD 4
        new List<Color>
        {
            new Color(0.0f, 0.2f, 0.4f),
            new Color(0.2f, 0.4f, 0.7f),
            new Color(0.6f, 0.8f, 0.9f),
            new Color(0.8f, 0.9f, 1.0f),
            new Color(1.0f, 1.0f, 1.0f)
        },
        // MARS 5
        new List<Color>
        {
            new Color(0.05f, 0.0f, 0.0f),
            new Color(0.3f, 0.1f, 0.0f),
            new Color(0.6f, 0.1f, 0.0f),
            new Color(0.8f, 0.4f, 0.1f),
            new Color(1.0f, 0.9f, 0.7f)
        },
        // LAVA PLANET 6
        new List<Color>
        {
            new Color(0.1f, 0.0f, 0.0f),
            new Color(0.4f, 0.0f, 0.0f),
            new Color(0.8f, 0.1f, 0.0f),
            new Color(1.0f, 0.4f, 0.0f),
            new Color(1.0f, 0.8f, 0.5f)
        },

        // GAS GIANT 7
        new List<Color>
        {
            new Color(0.1f, 0.1f, 0.3f),
            new Color(0.3f, 0.3f, 0.6f),
            new Color(0.6f, 0.6f, 0.9f),
            new Color(0.9f, 0.9f, 1.0f),
            new Color(1.0f, 1.0f, 1.0f)
        },

        // TOXIC PLANET 8
        new List<Color>
        {
            new Color(0.0f, 0.2f, 0.0f),
            new Color(0.1f, 0.4f, 0.0f),
            new Color(0.5f, 0.6f, 0.0f),
            new Color(0.7f, 0.8f, 0.1f),
            new Color(1.0f, 1.0f, 0.3f)
        },

        // CRYSTAL PLANET 9
        new List<Color>
        {
            new Color(0.2f, 0.0f, 0.4f),
            new Color(0.4f, 0.1f, 0.6f),
            new Color(0.6f, 0.3f, 0.9f),
            new Color(0.8f, 0.6f, 1.0f),
            new Color(1.0f, 0.9f, 1.0f)
        },

        // JUNGLE PLANET 10
        new List<Color>
        {
            new Color(0.0f, 0.2f, 0.0f),
            new Color(0.0f, 0.4f, 0.1f),
            new Color(0.0f, 0.6f, 0.2f),
            new Color(0.2f, 0.7f, 0.3f),
            new Color(0.4f, 0.9f, 0.5f)
        }
    };

    private static readonly Vector3 EARTH_LIKE = new Vector3(700, 530, 440);
    private static readonly Vector3 PURPLE_ISH = new Vector3(540, 700, 380);
    private static readonly Vector3 ORANGE = new Vector3(620, 800, 1000);
    private static readonly Vector3 GREEN = new Vector3(1000, 530, 800);

    // Warmth values per theme (index-aligned with planetThemes)
    private readonly List<double> themeWarmths = new List<double>
    {
        0.5, // PINK
        0.6, // EARTH
        0.4, // ALIEN
        0.9, // DESERT
        0.1, // ICE
        0.85, // MARS
        1.0, // LAVA PLANET
        0.3, // GAS GIANT
        0.7, // TOXIC PLANET
        0.2, // CRYSTAL PLANET
        0.6  // JUNGLE PLANET
    };

    private List<(List<Color> PlanetColors, Vector3 Atmosphere)> colorPairs;

    public PlanetThemeGenerator() {
        GeneratePair();
    }

    private void GeneratePair()
    {
        GD.Randomize();

        // Find the theme index with the closest warmth
        int bestIndex = 0;
        double closestDiff = Math.Abs(themeWarmths[0] - _warmth);

        for (int i = 1; i < themeWarmths.Count; i++)
        {
            double diff = Math.Abs(themeWarmths[i] - _warmth);
            if (diff < closestDiff)
            {
                closestDiff = diff;
                bestIndex = i;
            }
        }

        GD.Print("Selected Warmth: ", _warmth, " -> Theme Index: ", bestIndex);

        var planetColors = planetThemes[bestIndex];
        float[] positions = { 0.0f, 0.2f, 0.5f, 0.7f, 1.0f };

        Gradient = new Gradient();
        for (int i = 0; i < planetColors.Count; i++)
        {
            Gradient.AddPoint(positions[i], planetColors[i]);
        }

    }
}