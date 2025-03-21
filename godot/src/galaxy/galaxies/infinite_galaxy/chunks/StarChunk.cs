using Godot;
using System;
using System.Collections.Generic;

public partial class StarChunk : Node3D
{
	public FastNoiseLite galaxyNoise { private get; set; }
	public Mesh starMesh { private get; set; }

	[Export] StarChunkMultiMesh chunkMultiMesh;
	List<Transform3D> starTransforms = new List<Transform3D>();

	int chunkSize;
	int starCount;
	float ISOlevel;
	public ChunkCoord chunkPos { get; private set; }

	SeedGenerator seedGen = new SeedGenerator();

	public void Generate(uint galaxySeed, int chunkSize, int starCount, float ISOlevel, ChunkCoord pos)
	{
		galaxyNoise.Seed = (int)galaxySeed;
		this.chunkSize = chunkSize;
		this.starCount = starCount;
		this.ISOlevel = ISOlevel;
		this.chunkPos = pos;

		uint placementSeed = seedGen.GenerateSeed(galaxySeed, chunkPos);
		GD.Seed(placementSeed);

		int starIndex = 0;
		for (int i = 0; i < starCount; i++)
		{
			Vector3 localPoint = new Vector3(
				(float)GD.RandRange(0f, chunkSize),
				(float)GD.RandRange(0f, chunkSize),
				(float)GD.RandRange(0f, chunkSize)
			);

			Vector3 globalPoint = localPoint + ChunkPositionOffset();
			float noiseVal = galaxyNoise.GetNoise3Dv(globalPoint);

			if (ISOlevel < noiseVal)
			{
				Transform3D starTransform = new Transform3D(Basis.Identity, localPoint + ChunkPositionOffset());
				starTransforms.Add(starTransform);
				starIndex++;

				/*
				if (star is SelectableStar)
				{
					SelectableStar selectableStar = (SelectableStar)star;
					uint starSeed = seedGen.GenerateSeed(galaxySeed, globalPoint);
					selectableStar.SetSeed(starSeed);
				}

				//AddChild(star);
				*/
			}
		}

		chunkMultiMesh.DrawStars(starTransforms, starIndex, starMesh);
	}

	private Vector3 ChunkPositionOffset()
	{
		return new Vector3(
			this.chunkPos.x * chunkSize,
			this.chunkPos.y * chunkSize,
			this.chunkPos.z * chunkSize
		);
	}
}
