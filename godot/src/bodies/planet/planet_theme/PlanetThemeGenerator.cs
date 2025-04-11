using Godot;
using System;
using System.Collections.Generic;

[Tool]
[GlobalClass]
public partial class PlanetThemeGenerator : Resource
{
    [Export]
    public Godot.Collections.Array<PlanetThemeSet> ThemeSets { get; set; } = new Godot.Collections.Array<PlanetThemeSet>();
    private const string ThemeDirectoryPath = "res://src/bodies/planet/planet_theme/theme_sets/";

    [Export]
    public int Seed { get; set; } = 1;

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
            GenerateTheme(); // Automatically regenerate the theme
        }
    }

    private Gradient gradient = new Gradient();
    private Vector3 atmosphereWavelengths;

    public PlanetThemeGenerator() {
        LoadThemeSets();
        GenerateTheme();
    }

    private void GenerateTheme()
    {
        if (ThemeSets == null || ThemeSets.Count == 0)
            return;

        var rnd = new Random(Seed);

        PlanetThemeSet closestSet = ThemeSets[0];
        float smallestDiff = Math.Abs(closestSet.Warmth - (float)_warmth);

        foreach (var set in ThemeSets)
        {
            float diff = Math.Abs(set.Warmth - (float)_warmth);
            if (diff < smallestDiff)
            {
                smallestDiff = diff;
                closestSet = set;
            }
        }

        if (closestSet.Themes.Count == 0)
            return;

        int randomIndex = rnd.Next(closestSet.Themes.Count);
        var selectedTheme = closestSet.Themes[randomIndex];

        var planetColors = selectedTheme.Colors;
        // positions in the gradient
        float[] positions = { 0.0f, 0.2f, 0.5f, 0.7f, 1.0f };

        Gradient = new Gradient();
        for (int i = 0; i < Math.Min(planetColors.Count, positions.Length); i++)
        {
            Gradient.AddPoint(positions[i], planetColors[i]);
        }

        // remove default Godot-initialized points
        Gradient.RemovePoint(0);
        Gradient.RemovePoint(0);

    }

    private void LoadThemeSets()
    {
        var dir = DirAccess.Open(ThemeDirectoryPath);

        if (dir == null)
        {
            GD.PrintErr($"Could not open theme directory at {ThemeDirectoryPath}");
            return;
        }

        dir.ListDirBegin();

        string fileName;
        while ((fileName = dir.GetNext()) != "")
        {
            if (dir.CurrentIsDir()) continue;
            if (!fileName.EndsWith(".tres")) continue;

            string fullPath = ThemeDirectoryPath + fileName;
            var themeSet = ResourceLoader.Load<PlanetThemeSet>(fullPath);

            if (themeSet != null)
            {
                ThemeSets.Add(themeSet);
            }
            else
            {
                GD.PrintErr($"Failed to load theme set at {fullPath}");
            }
        }

        dir.ListDirEnd();
    }

}