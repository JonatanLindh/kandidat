using Godot;
using System;

public partial class ModularGalaxy : Node3D
{
	[Export] StarMultiMesh starMultiMesh;
	[Export] ModularGalaxyDistribution distribution;
	[Export] uint seed;

	public override void _Ready()
	{
		// Sets a random seed if no seed is provided
		if (seed == 0) seed = (uint)new Random().Next();
		GD.Seed(seed);

		Generate();
	}

	private void Generate()
	{

	}
}
