using Godot;
using System;

public class StarFactory
{
	public Star CreateStar(Vector3 position, uint galaxySeed)
	{
		Transform3D starTransform = new Transform3D(Basis.Identity, position);

		uint starSeed = GenerateSeed(galaxySeed, position);
		string starName = GenerateName();

		Star star = new Star(starTransform, starSeed, starName);
		return star;
	}

	private uint GenerateSeed(uint galaxySeed, Vector3 position)
	{
		SeedGenerator seedGen = new SeedGenerator();
		return seedGen.GenerateSeed(galaxySeed, position);
	}

	private string GenerateName()
	{
		return "Star";
	}
}
