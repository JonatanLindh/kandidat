using Godot;
using System;

public partial class DiscGalaxy : Node3D
{
	[Export] FastNoiseLite noise;
	[Export] StarMultiMesh starMultiMesh;
	[Export] Mesh starMesh;

	[Export] uint seed;

	Vector3[] starPos;

	int starCount = 10000;

	float baseISOLevel = 0.5f;
	float radius = 1000;

	public override void _Ready()
	{
		// Sets a random seed if no seed is provided
		if (seed == 0) seed = (uint)new Random().Next();

		GD.Seed(seed);

		starPos = new Vector3[starCount];

		Generate();
	}

	public override void _Process(double delta)
	{

	}

	private void Generate()
	{
		int starsGenerated = 0;

		while (starsGenerated < starCount)
		{
			Vector3 point = SamplePointInSphere();
			float noiseVal = noise.GetNoise3D(point.X, point.Y, point.Z);

			if (GetISOLevel(point) > noiseVal)
			{
				starPos[starsGenerated] = point;
				starsGenerated++;
			}
		}

		starMultiMesh.DrawStars(starPos, starMesh);
	}

	private Vector3 SamplePointInSphere()
	{
		double u = GD.RandRange(0f, 1f);
		double v = GD.RandRange(0f, 1f);
		double w = GD.RandRange(0f, 1f);

		double theta = 2 * Math.PI * u;
		double phi = Math.Acos(2 * v - 1);
		double r = radius * w;
		//double r = radius * Math.Cbrt(w); // uniform distribution

		double x = r * Math.Sin(phi) * Math.Cos(theta);
		double y = r * Math.Sin(phi) * Math.Sin(theta);
		double z = r * Math.Cos(phi);

		return new Vector3((float)x, (float)y, (float)z);
	}

	private float GetISOLevel(Vector3 pos)
	{
		float yDistFromCenter = Math.Abs(pos.Y);
		float iso = baseISOLevel - yDistFromCenter * 0.005f;
		return iso;
	}
}
