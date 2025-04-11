using Godot;
using System;

public partial class TrueGalaxy : Node3D
{
	DiscGalaxy discGalaxy;
	TrueStar[] stars;
	bool starsInitialized = false;

	public override void _Ready()
	{
		discGalaxy = GetNode<DiscGalaxy>("%DiscGalaxy");
		CallDeferred(nameof(InitializeStars));
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
	public TrueStar[] GetStars()
	{
		if (!starsInitialized)
		{
			GD.PrintErr("Stars not initialized yet.");
			return null;
		}

		return stars;
	}

	/// <summary>
	/// To update the stars to a new array of stars.
	/// Redraws the multimesh as well.
	/// </summary>
	/// <param name="newStars"></param>
	public void SetStars(TrueStar[] newStars)
	{
		if (!starsInitialized)
		{
			GD.PrintErr("Stars not initialized yet.");
			return;
		}

		if (newStars.Length != stars.Length)
		{
			GD.PrintErr("New stars array length does not match the existing stars array length.");
			return;
		}

		stars = newStars;

		RedrawStars();
	}

	/// <summary>
	/// Redraws the stars in the multimesh.
	/// </summary>
	public void RedrawStars()
	{
		if (!starsInitialized)
		{
			GD.PrintErr("Stars not initialized yet.");
			return;
		}

		Transform3D[] starTransforms = new Transform3D[stars.Length];
		for (int i = 0; i < stars.Length; i++)
		{
			starTransforms[i] = stars[i].transform;
		}

		discGalaxy.RedrawStars(starTransforms);
	}
}
