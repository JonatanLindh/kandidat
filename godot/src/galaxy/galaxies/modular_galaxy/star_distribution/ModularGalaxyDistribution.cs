using Godot;
using System;

[GlobalClass]
public partial class ModularGalaxyDistribution : Resource
{
	[Export] public int starCount { get; private set; }
}
