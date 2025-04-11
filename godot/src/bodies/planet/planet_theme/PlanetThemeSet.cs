// PlanetThemeSet.cs
using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class PlanetThemeSet : Resource
{
    [Export]
    public float Warmth = 0.5f;

    [Export]
    public Godot.Collections.Array<PlanetTheme> Themes { get; set; } = new Godot.Collections.Array<PlanetTheme>();
}