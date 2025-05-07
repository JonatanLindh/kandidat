using Godot;
using System;
using System.Collections.Generic;

public partial class StarChunk : Node3D, IStarChunkData
{
	public FastNoiseLite galaxyNoise { private get; set; }
	public Mesh starMesh { private get; set; }

	[Export] StarMultiMesh starMultiMesh;

	public Vector3[] stars { get; private set; }
	Vector3[] localStars;

	public int size { get; private set; }
	int starCount;
	float ISOlevel;
	public ChunkCoord pos { get; private set; }

	SeedGenerator seedGen = new SeedGenerator();

	public void Generate(uint galaxySeed, int chunkSize, int starCount, float ISOlevel, ChunkCoord pos)
	{
		galaxyNoise.Seed = (int)galaxySeed;
		this.size = chunkSize;
		this.starCount = starCount;
		this.ISOlevel = ISOlevel;
		this.pos = pos;

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
				Vector3 starPos = localPoint + ChunkPositionOffset();
				localStars[starIndex] = starPos;
				starIndex++;
			}
		}

		Array.Resize(ref localStars, starIndex);
		stars = new Vector3[localStars.Length];
		stars = localStars;

		starMultiMesh.DrawStars(stars, starMesh);
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
