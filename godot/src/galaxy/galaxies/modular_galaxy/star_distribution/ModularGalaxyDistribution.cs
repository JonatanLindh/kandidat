using Godot;
using System;

[GlobalClass]
public partial class ModularGalaxyDistribution : Resource
{
	[Export] public int starCount { get; private set; }
	[Export] public int galaxySize { get; private set; }

	[Export] public float gravity { get; private set; }

	[Export] public int armCount { get; private set; }
	[Export] public float armFactor { get; private set; }
}
