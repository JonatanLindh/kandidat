using Godot;
using System;

public partial class StarFinder : Node
{
	[Export(PropertyHint.Range, "1, 100, 1")] float maxRadius = 15;
	[Export(PropertyHint.Range, "0.1, 10, 0.1")] float initialRadius = 1.0f;
	[Export(PropertyHint.Range, "1.0, 2.0, 0.05")] float radiusGrowthRate = 1.1f;
	[Export(PropertyHint.Range, "0.1, 5.0, 0.1")] float intervalSizeRatio = 1.7f;

	IStarChunkData[] chunks;

	[ExportGroup("Debug")]
	[Export] bool debugPrint = false;
	[Export] bool debugDraw = false;
	[Export] Color debugFoundStarSphereColor = new Color(0.858f, 0.438f, 0.26f, 0.651f);
	[Export] float debugLineMaxLength = 1000;
	DebugDraw debugDrawer;

	public override void _Ready()
	{
		debugDrawer = GetNode<DebugDraw>("%DebugDraw");
	}

	/// <summary>
	/// Finds along a line from one point to another.
	/// <c>range</c> is the maximum distance to check, 0 for infinite (until chunks run out)
	/// </summary>
	public Vector3 FindStar(Vector3 from, Vector3 dir, IStarChunkData[] chunks, float range = 0)
	{
		if (debugDraw)
		{
			debugDrawer.Clear();
			if(range > 0) debugDrawer.DrawLine(from, from + (dir.Normalized() * range));
			else debugDrawer.DrawLine(from, from + (dir.Normalized() * debugLineMaxLength));
		}

		this.chunks = chunks;

		if (chunks.Length == 0)
		{
			GD.PrintErr("StarFinder: No chunks found in galaxy");
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
				if(debugPrint) GD.Print("StarFinder: No star found. Exiting.");
				return Vector3.Zero;
			}

			foreach (Vector3 starPos in currentChunk.stars)
			{
				if ((starPos - currentPos).Length() < currentRadius)
				{
					if (debugPrint) GD.Print($"StarFinder: Found star at {starPos} in chunk {currentChunk.pos}");
					if (debugDraw) debugDrawer.DrawSphere(currentPos, currentRadius, debugFoundStarSphereColor);

					return starPos;
				}
			}

			if (debugDraw) debugDrawer.DrawSphere(currentPos, currentRadius);

			currentRadius = Mathf.Min(currentRadius * radiusGrowthRate, maxRadius);
			currentInterval = currentRadius * intervalSizeRatio;

			currentPos += dir.Normalized() * currentInterval;
		}

		return Vector3.Zero;
	}

	/// <summary>
	/// Checks if the chunk at the given position is generated.
	/// </summary>
	/// <param name="position"></param>
	/// <returns></returns>
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
