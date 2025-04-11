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

    // Warmness indicator associated with themes in planetThemes dictionary, 0 = coldest, 1 = warmest
    private readonly List<double> themeWarmths = new List<double>
    {
        0, 0.2, 0.5, 0.8, 1
    };

    // Plaet themes
    private List<Color> pink = new List<Color>
        {
            new Color(0.0f, 0.3f, 0.7f),
            new Color(0.5f, 0.4f, 0.1f),
            new Color(0.81f, 0.44f, 0.65f),
            new Color(0.86f, 0.63f, 0.47f),
            new Color(1.0f, 1.0f, 1.0f)
        };
    private List<Color> earth = new List<Color>
        {
            new Color(0.0f, 0.3f, 0.7f),
            new Color(0.439f, 0.255f, 0.0f),
            new Color(0.035f, 0.31f, 0.0f),
            new Color(0.3f, 0.3f, 0.3f),
            new Color(1.0f, 1.0f, 1.0f)
        };
    private List<Color> alien = new List<Color>
        {
            new Color(0.02f, 0.0f, 0.1f),
            new Color(0.1f, 0.0f, 0.2f),
            new Color(0.0f, 0.8f, 0.6f),
            new Color(0.3f, 0.1f, 0.5f),
            new Color(0.9f, 1.0f, 1.0f)
        };
    private List<Color> desert = new List<Color>
        {
            new Color(0.35f, 0.2f, 0.1f),
            new Color(0.7f, 0.3f, 0.1f),
            new Color(0.85f, 0.6f, 0.3f),
            new Color(0.95f, 0.8f, 0.5f),
            new Color(1.0f, 1.0f, 0.9f)
        };
    private List<Color> redDunes = new List<Color>
    {
        new Color(0.45f, 0.15f, 0.10f),
        new Color(0.60f, 0.25f, 0.15f),
        new Color(0.75f, 0.35f, 0.20f),
        new Color(0.90f, 0.55f, 0.35f),
        new Color(1.00f, 0.80f, 0.60f)
    };
    private List<Color> red = new List<Color>
    {
        new Color(1.00f, 0.00f, 0.00f),
        new Color(1.00f, 0.00f, 0.00f),
        new Color(1.00f, 0.00f, 0.00f),
        new Color(1.00f, 0.00f, 0.00f),
        new Color(1.00f, 0.00f, 0.00f)
    };
    private List<Color> rockyDesert = new List<Color>
    {
        new Color(0.25f, 0.18f, 0.14f),
        new Color(0.40f, 0.28f, 0.20f),
        new Color(0.55f, 0.38f, 0.25f),
        new Color(0.70f, 0.50f, 0.35f),
        new Color(0.85f, 0.70f, 0.55f)
    };
    private List<Color> iceWorld = new List<Color>
        {
            new Color(0.0f, 0.2f, 0.4f),
            new Color(0.2f, 0.4f, 0.7f),
            new Color(0.6f, 0.8f, 0.9f),
            new Color(0.8f, 0.9f, 1.0f),
            new Color(1.0f, 1.0f, 1.0f)
        };
    private List<Color> mars = new List<Color>
        {
            new Color(0.05f, 0.0f, 0.0f),
            new Color(0.3f, 0.1f, 0.0f),
            new Color(0.6f, 0.1f, 0.0f),
            new Color(0.8f, 0.4f, 0.1f),
            new Color(1.0f, 0.9f, 0.7f)
        };
    private List<Color> lava = new List<Color>
        {
            new Color(0.1f, 0.0f, 0.0f),
            new Color(0.4f, 0.0f, 0.0f),
            new Color(0.8f, 0.1f, 0.0f),
            new Color(1.0f, 0.4f, 0.0f),
            new Color(1.0f, 0.8f, 0.5f)
        };
    private List<Color> gas = new List<Color>
        {
            new Color(0.1f, 0.1f, 0.3f),
            new Color(0.3f, 0.3f, 0.6f),
            new Color(0.6f, 0.6f, 0.9f),
            new Color(0.9f, 0.9f, 1.0f),
            new Color(1.0f, 1.0f, 1.0f)
        };
    private List<Color> toxic = new List<Color>
        {
            new Color(0.0f, 0.2f, 0.0f),
            new Color(0.1f, 0.4f, 0.0f),
            new Color(0.5f, 0.6f, 0.0f),
            new Color(0.7f, 0.8f, 0.1f),
            new Color(1.0f, 1.0f, 0.3f)
        };
    private List<Color> blue = new List<Color>
        {
            new Color(0.0f, 0.1f, 0.2f),
            new Color(0.0f, 0.3f, 0.4f),
            new Color(0.0f, 0.3f, 0.6f),
            new Color(0.0f, 0.7f, 0.8f),
            new Color(0.0f, 0.5f, 1.0f)
        };
    private List<Color> jungle = new List<Color>
        {
            new Color(0.0f, 0.2f, 0.0f),
            new Color(0.0f, 0.4f, 0.1f),
            new Color(0.0f, 0.6f, 0.2f),
            new Color(0.2f, 0.7f, 0.3f),
            new Color(0.4f, 0.9f, 0.5f)
        };

    private Dictionary<double, List<List<Color>>> planetThemes;

    private static readonly Vector3 EARTH_LIKE = new Vector3(700, 530, 440);
    private static readonly Vector3 PURPLE_ISH = new Vector3(540, 700, 380);
    private static readonly Vector3 ORANGE = new Vector3(620, 800, 1000);
    private static readonly Vector3 GREEN = new Vector3(1000, 530, 800);


    private List<(List<Color> PlanetColors, Vector3 Atmosphere)> colorPairs;

    public PlanetThemeGenerator() {
        // Dictionary of themes, cooler at lower indicies
        planetThemes = new Dictionary<double, List<List<Color>>>()
        {
            { themeWarmths[0], new List<List<Color>> { iceWorld, blue, gas }},
            { themeWarmths[1], new List<List<Color>> { alien, pink }},
            { themeWarmths[2], new List<List<Color>> { earth, jungle, toxic }},
            { themeWarmths[3], new List<List<Color>> { mars, desert, rockyDesert, redDunes }},
            { themeWarmths[4], new List<List<Color>> { lava }}
        };
        GeneratePair();
    }

    private void GeneratePair()
    {
        GD.Randomize();
        var rnd = new Random();
        var randomI = 0;

        // Find the theme index with the closest warmth
        double bestKey = 0;
        double closestDiff = Math.Abs(themeWarmths[0] - _warmth);

        for (int i = 1; i < themeWarmths.Count; i++)
        {
            double diff = Math.Abs(themeWarmths[i] - _warmth);
            if (diff < closestDiff)
            {
                closestDiff = diff;

                // Randomize chosen key further to simulate thinner atmospheres etc.
                randomI = rnd.Next(-1, 2);  
                randomI = i + randomI;     
                randomI = Math.Max(0, randomI); 
                randomI = Math.Min(themeWarmths.Count - 1, randomI); 

                bestKey = themeWarmths[randomI];
            }
        }
        var selectedTemperatureThemes = planetThemes[bestKey];
        randomI = rnd.Next(selectedTemperatureThemes.Count);

        var planetColors = selectedTemperatureThemes[randomI];
        float[] positions = { 0.0f, 0.2f, 0.5f, 0.7f, 1.0f };

        Gradient = new Gradient();

        for (int i = 0; i < planetColors.Count; i++)
        {
            Gradient.AddPoint(positions[i], planetColors[i]);
        }
        // Remove default points from gradient that godot initializes the gradient with.
        Gradient.RemovePoint(0);
        Gradient.RemovePoint(0);
    }
}