using Godot;
using System;

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
	/// Redraws the stars in the multimesh.
	/// Updates star transform and velocity.
	/// </summary>
	private void UpdateStars(Godot.Collections.Dictionary newStars)
	{
		if (!starsInitialized)
		{
			GD.PrintErr("TrueGalaxy: Stars not initialized yet.");
			return;
		}

		if (newStars.Count != stars.Length)
		{
			GD.PrintErr("TrueGalaxy: New transform array length does not match the existing stars array length.");
			return;
		}

		Godot.Collections.Array<Transform3D> transformArray = newStars["transform"].AsGodotArray<Transform3D>();
		Godot.Collections.Array<Vector3> velocityArray = newStars["velocity"].AsGodotArray<Vector3>();

		Transform3D[] transformArrayConverted = new Transform3D[transformArray.Count];
		for (int i = 0; i < stars.Length; i++)
		{
			stars[i].transform = transformArray[i];
			stars[i].velocity = velocityArray[i];

			transformArrayConverted[i] = stars[i].transform;
		}

		discGalaxy.RedrawStars(transformArrayConverted);
	}
}
