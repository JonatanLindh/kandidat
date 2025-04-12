using Godot;
using System;

public partial class ModularGalaxy : Node3D
{
	[Export] StarMultiMesh starMultiMesh;
	[Export] Mesh starMesh;
	[Export] ModularGalaxyDistribution distribution;
	[Export] uint seed;
	Vector3[] _stars;

	public override void _Ready()
	{
		// Sets a random seed if no seed is provided
		if (seed == 0) seed = (uint)new Random().Next();
		GD.Seed(seed);

		_stars = Generate();
		DrawStars(_stars);
	}

	private void DrawStars(Vector3[] stars)
	{
		if (starMultiMesh == null)
		{
			GD.PrintErr("ModularGalaxy: StarMultiMesh is not initialized.");
			return;
		}

		if (stars.Length == 0)
		{
			GD.PrintErr("ModularGalaxy: No stars to draw.");
			return;
		}

		starMultiMesh.DrawStars(stars, starMesh);
	}

	private Vector3[] Generate()
	{
		Vector3[] stars = new Vector3[distribution.starCount];

		for (int i = 0; i < distribution.starCount; i++)
		{
			Vector3 point = SamplePoint();
			stars[i] = point;
		}

		return stars;
	}

	private Vector3 SamplePoint()
	{
		Vector3 point = Vector3.Zero;
		double armAngle =  2 * Math.PI / distribution.armCount;

		// Pick a random arm
		int arm = GD.RandRange(0, distribution.armCount - 1);

		// Calculate the angle of the arm
		double armStartAngle = arm * armAngle;
		double armEndAngle = armStartAngle + armAngle * distribution.armWidth;

		double starAngle = GD.RandRange(armStartAngle, armEndAngle);
		double starDistance = GD.RandRange(0, distribution.galaxySize);

		// Calculate star position from polar coodinates
		double x = starDistance * Math.Cos(starAngle);
		double z = starDistance * Math.Sin(starAngle);

		return new Vector3((float) x, 0, (float) z);
	}

	public Vector3[] GetStars()
	{
		return _stars;
	}
}
