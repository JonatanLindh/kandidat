using Godot;
using System;

public partial class ModularGalaxy : Node3D
{
	[Export] StarMultiMesh starMultiMesh;
	[Export] Mesh starMesh;
	[Export] ModularGalaxyDistribution distribution;
	[Export] uint seed;
	Vector3[] _stars;

	[ExportGroup("Debug")]
	[Export] bool debugPrint = false;
	[Export] bool debugDrawGravity = false;
	[Export] bool debugDrawCurve = false;
	DebugDraw debugDrawer;

	public override void _Ready()
	{
		debugDrawer = GetNode<DebugDraw>("%DebugDraw");

		// Sets a random seed if no seed is provided
		if (seed == 0) seed = (uint)new Random().Next();
		GD.Seed(seed);

		// Apply the galaxy axis rotation
		this.Rotate(distribution.axis.Normalized(), Mathf.Pi / 2);

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

		// Sample a random point in the galaxy, on one random arm
		int arm = GD.RandRange(0, distribution.armCount - 1);
		point = SamplePointOnArm(arm);

		// Apply galaxy-wide gravity offset to the point
		Vector3 gravityOffset = GetGravityOffset(point);
		if(debugDrawGravity) debugDrawer.DrawLine(point, point + gravityOffset);
		point += gravityOffset;

		// Central vertical offset
		// ...

		// Wave vertical offset
		// ...

		// Curve the star position
		Vector3 curveOffset = GetCurveOffset(point);
		if (debugDrawCurve) debugDrawer.DrawLine(point, point + curveOffset);
		point += curveOffset;

		return point;
	}

	private Vector3 SamplePointOnArm(int arm)
	{
		Vector3 point = Vector3.Zero;
		double armAngle = 2 * Math.PI / distribution.armCount;

		// Calculate the angle of the arm
		double armStartAngle = arm * armAngle;
		double armEndAngle = armStartAngle + armAngle * distribution.armFactor;

		double starAngle = GD.RandRange(armStartAngle, armEndAngle);
		double starDistance = GD.RandRange(0, distribution.galaxySize);

		// Calculate star position from polar coodinates
		double x = starDistance * Math.Cos(starAngle);
		double z = starDistance * Math.Sin(starAngle);

		return new Vector3((float)x, 0, (float)z);
	}

	private Vector3 GetGravityOffset(Vector3 startPos)
	{
		double distance = startPos.Length();
		double gravityStrengthFactor = Math.Max(0, 1 - Math.Pow((distance / distribution.galaxySize), 2));
		float gravityStrength = (float)(distribution.gravity * gravityStrengthFactor);
		Vector3 gravityDirection = -startPos.Normalized();

		Vector3 offset = gravityDirection * gravityStrength;
		return offset;
	}

	private Vector3 GetCurveOffset(Vector3 startPos)
	{
		double distance = startPos.Length();
		double rotationStrengthFactor = Math.Pow((distance / distribution.galaxySize), 2);
		float rotationStrength = (float)(distribution.curveStrength * rotationStrengthFactor);

		Vector3 cross = startPos.Cross(Vector3.Up).Normalized();
		Vector3 offset = cross * rotationStrength;
		return offset;
	}

	public Vector3[] GetStars()
	{
		return _stars;
	}
}
