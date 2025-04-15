using Godot;
using System;

[GlobalClass]
public partial class TrueGalaxy : Node3D
{
	DiscGalaxy discGalaxy;
	TrueStar[] stars;
	bool starsInitialized = false;

	TrueStarAdapter starAdapter = new TrueStarAdapter();

	public override void _Ready()
	{
		discGalaxy = GetNode<DiscGalaxy>("%DiscGalaxy");
		InitializeStars();
	}

	private void InitializeStars()
	{
		stars = discGalaxy.GetStars();
		starsInitialized = true;
	}

	public override void _Process(double delta)
	{

	}

	/// <summary>
	/// To get the stars of the galaxy.
	/// </summary>
	/// <returns></returns>
	public Godot.Collections.Dictionary GetStars()
	{
		if (!starsInitialized)
		{
			GD.PrintErr("TrueGalaxy: Stars not initialized yet.");
			return null;
		}

		return starAdapter.StarsToPhysicsData(stars);
	}

	/// <summary>
	/// Redraws the stars in the multimesh. Updates star transform taking into account the velocity.
	/// </summary>
	private void ApplyVelocities(Vector3[] newVelocities)
	{
		if (!starsInitialized)
		{
			GD.PrintErr("TrueGalaxy: Stars not initialized yet.");
			return;
		}

		if (newVelocities.Length != stars.Length)
		{
			GD.PrintErr("TrueGalaxy: New transform array length does not match the existing stars array length.");
			return;
		}

		Transform3D[] newTransforms = new Transform3D[newVelocities.Length];
		for (int i = 0; i < stars.Length; i++)
		{
			Transform3D newTransform = new Transform3D(
				stars[i].transform.Basis,
				stars[i].transform.Origin + newVelocities[i]
			);

			newTransforms[i] = newTransform;
			stars[i].transform = newTransform;
		}

		discGalaxy.RedrawStars(newTransforms);
	}
}
