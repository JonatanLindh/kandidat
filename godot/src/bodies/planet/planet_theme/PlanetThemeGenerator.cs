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

	private int seed = 0;
	[Export]
	public int Seed { 
		get => seed; 
		set
		{
			seed = value;
		} 
	}

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
        }
	}

	private Gradient gradient = new Gradient();
	private Vector3 atmosphereWavelengths;

	public PlanetThemeGenerator()
	{
	}

	public void GenerateTheme()
	{
        LoadThemeSets();
        if (ThemeSets == null || ThemeSets.Count == 0)
			return;

		var rnd = new Random(seed);

		PlanetThemeSet closestSet = ThemeSets[0];
		int closestIndex = 0;
		float smallestDiff = Math.Abs(closestSet.Warmth - (float)_warmth);


		for (int i = 0; i < ThemeSets.Count; i++)
		{
			float diff = Math.Abs(ThemeSets[i].Warmth - (float)_warmth);
			if(diff < smallestDiff)
			{
				smallestDiff = diff;
				closestIndex = i;
			}
		}

		closestSet = ThemeSets[closestIndex];

		if (closestSet.Themes.Count == 0)
			return;

		bool switchToNeighbor = rnd.Next(2) == 0;

		if(switchToNeighbor)
		{
			if (closestIndex == 0)
			{
				closestSet = ThemeSets[1];
			}
			else if (closestIndex == ThemeSets.Count - 1)
			{
				closestSet = ThemeSets[ThemeSets.Count - 2];
			}
			else
			{
				int randomInd = rnd.Next(2) == 0 ? -1 : 1;
				closestSet = ThemeSets[closestIndex + randomInd];
			}
		}

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

	public void LoadThemeSets()
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
