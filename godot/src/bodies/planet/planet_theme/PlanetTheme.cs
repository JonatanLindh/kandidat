// PlanetTheme.cs
using Godot;
using System.Collections.Generic;

[Tool]
[GlobalClass]
public partial class PlanetTheme : Resource
{
    [Export]
    public Godot.Collections.Array<Color> Colors { get; set; } = new Godot.Collections.Array<Color>();

    [Export]
    public Vector3 AtmosphereWavelength { get; set; } = Vector3.Zero;
}