using Godot;
using System;

public partial class StarFinder : Node
{
	[Export(PropertyHint.Range, "1, 100, 1")] float maxRadius = 15;
    [Export(PropertyHint.Range, "0.1, 10, 0.1")] float initialRadius = 1.0f;
    [Export(PropertyHint.Range, "1.0, 2.0, 0.05")] float radiusGrowthRate = 1.1f;
    [Export(PropertyHint.Range, "0.1, 5.0, 0.1")] float intervalSizeRatio = 1.7f;

	IStarChunkData[] chunks;

	/// <summary>
	/// Finds along a line from one point to another.
	/// <c>range</c> is the maximum distance to check, 0 for infinite (until chunks run out)
	/// </summary>
	public Vector3 FindStar(Vector3 from, Vector3 dir, IStarChunkData[] chunks, float range = 0)
	{
		this.chunks = chunks;

		if (chunks.Length == 0)
		{
			GD.PrintErr("No chunks found in galaxy");
			return Vector3.Zero;
		}

		IStarChunkData currentChunk = GetChunkData(from);
		Vector3 currentPos = from;

		float currentRadius = initialRadius;
		float currentInterval;

		while (range == 0 || from.DistanceTo(currentPos) < range)
		{
			currentChunk = GetChunkData(currentPos);

			if (currentChunk == null)
			{
				GD.Print("No star found, done checking chunks");
				return Vector3.Zero;
			}

			foreach (Vector3 starPos in currentChunk.stars)
			{
				if ((starPos - currentPos).Length() < currentRadius)
				{
					return starPos;
				}
			}

			currentRadius = Mathf.Min(currentRadius * radiusGrowthRate, maxRadius);
			currentInterval = currentRadius * intervalSizeRatio;

			currentPos += dir.Normalized() * currentInterval;
		}

		return Vector3.Zero;
	}

	private IStarChunkData GetChunkData(Vector3 position)
	{
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
