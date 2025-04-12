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

		bool valid = false;
		while(!valid)
		{
			// Sample a random point from within the galaxy size
			float x = (float)GD.RandRange(-distribution.galaxySize, distribution.galaxySize);
			float y = (float)GD.RandRange(-distribution.galaxySize, distribution.galaxySize);
			float z = (float)GD.RandRange(-distribution.galaxySize, distribution.galaxySize);
			point = new Vector3(x, y, z);

			double angle = Math.Atan2(point.Z, point.X);

			for (double i = -Math.PI; i < Math.PI; i += armAngle)
			{
				if(angle > i && angle < i + armAngle * distribution.armWidth)
				{
					valid = true;
					break;
				}

			}
		}

		point.Y = 0;



		


		return point;
	}

	public Vector3[] GetStars()
	{
		return _stars;
	}
}
