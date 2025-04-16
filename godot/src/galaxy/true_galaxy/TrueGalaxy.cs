using Godot;
using System;

[GlobalClass]
public partial class TrueGalaxy : Node3D
{
	[Export] Node3D _galaxyNode;
	IGalaxy galaxy;

	PhysicsStar[] stars;
	bool starsInitialized = false;

	StarFactory starFactory;
	TrueStarAdapter starAdapter = new TrueStarAdapter();

	[ExportCategory("Star Properties")]
	[Export] Vector3 initialStarVelocity = new Vector3(0, 0, 0);
	[Export] float starMass = 10.0f;

	public override void _Ready()
	{
		if (_galaxyNode == null)
		{
			GD.PrintErr("TrueGalaxy: Galaxy node is not initialized.");
			return;
		}

		else if(!(_galaxyNode is IGalaxy))
		{
			GD.PrintErr("TrueGalaxy: Galaxy node is not of type IGalaxy.");
			return;
		}

		starFactory = new StarFactory();

		galaxy = (IGalaxy) _galaxyNode;
		InitializeStars();
	}

	private void InitializeStars()
	{
		Vector3[] starPositions = galaxy.GetStarPositions();
		stars = new PhysicsStar[starPositions.Length];

		// star factory to create stars...
		for (int i = 0; i < starPositions.Length; i++)
		{
			stars[i] = starFactory.CreatePhysicsStar(starPositions[i], galaxy.GetSeed(), starMass, initialStarVelocity);
		}

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
	public void ApplyVelocities(Vector3[] newVelocities)
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

		galaxy.RedrawStars(newTransforms);
	}
}
