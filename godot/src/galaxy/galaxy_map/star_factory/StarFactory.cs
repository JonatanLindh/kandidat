using Godot;
using System;

public class StarFactory
{
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
		string starName = GenerateName();

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
	/// 
	/// </summary>
	/// <returns></returns>
	private string GenerateName()
	{
		return "Star";
	}
}
