using Godot;
using System;
using System.Collections.Generic;

public class StarFactory
{
	private string[] starCatalog = {
		"HD",
		"HR",
		"SAO",
		"GSC",
		"BD"
	};

	/// <summary>
	/// Creates a star at the given position with a new seed based on the galaxy seed and position.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="galaxySeed"></param>
	/// <returns></returns>
	public Star CreateStar(Vector3 position, uint galaxySeed)
	{
		Transform3D starTransform = new Transform3D(Basis.Identity, position);

		uint starSeed = GenerateSeed(galaxySeed, position);
		string starName = GenerateName(starSeed);

		Star star = new Star(starTransform, starSeed, starName);
		return star;
	}

	/// <summary>
	/// Generates a unique seed for the star based on the galaxy seed and its position.
	/// </summary>
	/// <param name="galaxySeed"></param>
	/// <param name="position"></param>
	/// <returns></returns>
	private uint GenerateSeed(uint galaxySeed, Vector3 position)
	{
		SeedGenerator seedGen = new SeedGenerator();
		return seedGen.GenerateSeed(galaxySeed, position);
	}

	/// <summary>
	/// Generates a name for the star based on its seed, 
	/// and a randomly selected catalog acronym.
	/// </summary>
	/// <param name="seed"></param>
	/// <returns></returns>
	private string GenerateName(uint seed)
	{
		Random rnd = new Random((int)seed);
		int index = rnd.Next(0, starCatalog.Length);
		string starAcronym = starCatalog[index];
		return starAcronym + " " + seed;
	}
}
