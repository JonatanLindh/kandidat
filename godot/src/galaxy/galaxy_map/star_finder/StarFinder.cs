using Godot;
using System;

public partial class StarFinder : Node
{
	public InfiniteGalaxy galaxy { private get; set; }

    [Export(PropertyHint.Range, "1, 100, 1")] float maxRadius = 25;
    [Export(PropertyHint.Range, "0.1, 10, 0.1")] float initialRadius = 1.0f;
    [Export(PropertyHint.Range, "1.0, 2.0, 0.05")] float radiusGrowthRate = 1.1f;
    [Export(PropertyHint.Range, "0.1, 5.0, 0.1")] float intervalSizeRatio = 1.7f;

	/// <summary>
	/// Finds along a line from one point to another.
	/// <c>range</c> is the maximum distance to check, 0 for infinite (until chunks run out)
	/// </summary>
	public Star FindStar(Vector3 from, Vector3 dir, float range = 0)
	{
		IStarChunkData currentChunk = GetChunkData(from);
		Vector3 currentPos = from;

		float currentRadius = initialRadius;
		float currentInterval;

		while (range == 0 || from.DistanceTo(currentPos) < range)
		{
			currentChunk = GetChunkData(currentPos);

			if (currentChunk == null)
			{
				GD.Print("No star found & done checking chunks");
				return null;
			}

			foreach (Vector3 starPos in currentChunk.stars)
			{
				if ((starPos - currentPos).Length() < currentRadius)
				{
					Star star = CreateStar(starPos, galaxy.GetSeed());
					return star;
				}
			}

			currentRadius = Mathf.Min(currentRadius * radiusGrowthRate, maxRadius);
			currentInterval = currentRadius * intervalSizeRatio;

			currentPos += dir.Normalized() * currentInterval;
		}

		return null;
	}

	private Star CreateStar(Vector3 position, uint seed)
	{
		// TODO
		// String starName = ...
		Transform3D starTransform = new Transform3D(Basis.Identity, position);

		SeedGenerator seedGen = new SeedGenerator();
		uint starSeed = seedGen.GenerateSeed(galaxy.GetSeed(), position);

		Star star = new Star(starTransform, starSeed);
		return star;
	}

	private IStarChunkData GetChunkData(Vector3 position)
	{
		IStarChunkData[] chunks = galaxy.GetGeneratedChunks();

		if (chunks.Length == 0)
		{
			GD.PrintErr("No chunks found in galaxy");
			return null;
		}

		ChunkCoord chunkPos = ChunkCoord.ToChunkCoord(chunks[0].size, position);

		foreach (IStarChunkData chunk in chunks)
		{
			if (chunk.pos.Equals(chunkPos))
			{
				return chunk;
			}
		}

		return null;
	}
}
