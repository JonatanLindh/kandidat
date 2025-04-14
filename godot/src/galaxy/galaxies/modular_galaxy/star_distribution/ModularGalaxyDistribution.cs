using Godot;
using System;

[GlobalClass]
public partial class ModularGalaxyDistribution : Resource
{
	[Export] public Vector3 axis { get; private set; } = Vector3.Up;
	[Export] public int starCount { get; private set; }
	[Export] public int galaxySize { get; private set; }

	[Export] public float gravity { get; private set; }

	[Export] public int armCount { get; private set; }
	[Export] public float armFactor { get; private set; }

	[Export] public float curveStrength { get; private set; }
}
