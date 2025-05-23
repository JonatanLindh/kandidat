using Godot;
using System;

public partial class DiscGalaxy : Node3D
{
	[Export] FastNoiseLite noise;
	[Export] StarMultiMesh starMultiMesh;
	[Export] Mesh starMesh;

	[Export] uint seed;

	SeedGenerator seedGen = new SeedGenerator();

	TrueStar[] stars;

	int starCount = 10000;

	float baseISOLevel = 0.5f;
	float radius = 100000;

	public override void _Ready()
	{
		// Sets a random seed if no seed is provided
		if (seed == 0) seed = (uint)new Random().Next();

		GD.Seed(seed);

		stars = new TrueStar[starCount];

		Generate();
	}

	public override void _Process(double delta)
	{

	}

	private void Generate()
	{
		int starsGenerated = 0;
		float middle_mass = 10e6f;
		
		TrueStar white_hole = new TrueStar(
			new Transform3D(Basis.Identity, Vector3.Zero),
			0,
			middle_mass,
			Vector3.Zero,
			"Center"
		);

		stars[starsGenerated] = white_hole;
		starsGenerated++;

		while (starsGenerated < starCount)
		{
			Vector3 point = SamplePointInSphere();
			float noiseVal = noise.GetNoise3D(point.X, point.Y, point.Z);
			
			Vector3 v0 = Vector3.Up.Cross(-point).Normalized();
			v0 *= Mathf.Sqrt(middle_mass / point.Length());
			
			if (GetISOLevel(point) > noiseVal)
			{
				TrueStar star = new TrueStar(
					new Transform3D(Basis.Identity, point),
					seedGen.GenerateSeed(seed, point),
					1f,
					v0,
					"Star"
				);

				stars[starsGenerated] = star;
				starsGenerated++;
			}
		}
		
		Vector3[] starPositions = new Vector3[starCount];
		for (int i = 0; i < starCount; i++)
		{
			starPositions[i] = stars[i].transform.Origin;
		}

		starMultiMesh.DrawStars(starPositions, starMesh);
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

	public TrueStar[] GetStars()
	{
		return stars;
	}

	public void RedrawStars(Transform3D[] stars)
	{
		starMultiMesh.RedrawStars(stars);
	}

	public void ColorStars(Color[] colors)
	{
		starMultiMesh.ColorStar(colors);
	}
}
