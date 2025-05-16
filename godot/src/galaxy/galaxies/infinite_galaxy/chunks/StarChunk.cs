using Godot;
using System;
using System.Collections.Generic;

public partial class StarChunk : Node3D, IStarChunkData
{
	public FastNoiseLite galaxyNoise { private get; set; }
	public Mesh starMesh { private get; set; }

	[Export] StarMultiMesh starMultiMesh;
	StarFactory starFactory;

	[Export] Color baseStarColor = new Color(1, 1, 1);

	public Vector3[] stars { get; private set; }
	Vector3[] localStars;

	public int size { get; private set; }
	int starCount;
	float ISOlevel;
	public ChunkCoord pos { get; private set; }

	SeedGenerator seedGen = new SeedGenerator();

	Color[] colors = new Color[]
	{
		new Color(1, 0.14f, 0),   // Red (Red dwarf or red giant)
		new Color(1, 0.5f, 0),    // Orange (Orange dwarf)
		new Color(1, 1, 1),       // White (White star)
		new Color(0.5f, 0.5f, 1), // Light blue (A-type star)
		new Color(0.2f, 0.2f, 1), // Blue (Hot B-type star)
		new Color(0.1f, 0.1f, 1), // Very hot blue (O-type star)
		new Color(0.8f, 0.8f, 1), // Pale blue-white (F-type star)
		new Color(0.9f, 0.8f, 0), // Yellow-orange (K-type star)
		new Color(0.8f, 0.6f, 0.4f) // Yellow-brownish (G-type star, slightly more red
	};

	public void Generate(uint galaxySeed, int chunkSize, int starCount, float ISOlevel, ChunkCoord pos, bool colorStars, float minimumDistance = 0)
	{
		galaxyNoise.Seed = (int)galaxySeed;
		this.size = chunkSize;
		this.starCount = starCount;
		this.ISOlevel = ISOlevel;
		this.pos = pos;

		starFactory = new StarFactory();

		localStars = new Vector3[starCount];

		uint placementSeed = seedGen.GenerateSeed(galaxySeed, pos);
		GD.Seed(placementSeed);

		int starIndex = 0;
		for (int i = 0; i < starCount; i++)
		{
			Vector3 localPoint = new Vector3(
				(float)GD.RandRange(0f, size),
				(float)GD.RandRange(0f, size),
				(float)GD.RandRange(0f, size)
			);

			Vector3 globalPoint = localPoint + ChunkPositionOffset();
			float noiseVal = galaxyNoise.GetNoise3Dv(globalPoint);

			if (ISOlevel < noiseVal)
			{
				bool isTooClose = false;
				for (int j = 0; j < starIndex; j++)
				{
					if (localStars[j].DistanceSquaredTo(globalPoint) < minimumDistance * minimumDistance)
					{
						isTooClose = true;
						break;
					}
				}

				if (isTooClose) continue;

				localStars[starIndex] = globalPoint;
				starIndex++;
			}
		}

		Array.Resize(ref localStars, starIndex);
		stars = new Vector3[localStars.Length];
		stars = localStars;

		starMultiMesh.DrawStars(stars, starMesh);

		if(colorStars) // Use actual system star color
		{
			Color[] newColors = new Color[stars.Length];
			for (int i = 0; i < stars.Length; i++)
			{
				Vector3 starPos = stars[i];
				Star star = starFactory.CreateStar(starPos, galaxySeed);

				int colIndex = (int)(star.seed % (uint)colors.Length);
				newColors[i] = colors[colIndex];
			}
			starMultiMesh.ColorStar(newColors);
		}

		else // Use base color  
		{
			Color[] colors = new Color[stars.Length];
			Array.Fill(colors, baseStarColor);
			starMultiMesh.ColorStar(colors);
		}
	}

	private Vector3 ChunkPositionOffset()
	{
		return new Vector3(
			this.pos.x * size,
			this.pos.y * size,
			this.pos.z * size
		);
	}
}
