using Godot;
using System;

[GlobalClass]
public partial class ModularGalaxyDistribution : Resource
{
	[Export] public int starCount { get; private set; }
	[Export] public int galaxySize { get; private set; }

	[Export] public int armCount { get; private set; }
	[Export] public float armWidth { get; private set; }
}
