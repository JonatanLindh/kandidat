using Godot;
using System;
using System.Collections.Generic;

public partial class StarFinder : Node
{
	[Export(PropertyHint.Range, "1, 500, 1")] float maxRadius = 15;
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
	public Vector3 FindStarInLine(Vector3 from, Vector3 dir, IStarChunkData[] chunks, float range = 0)
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
	/// Finds all stars in a sphere around the given (player) position of the current chunk.
	/// </summary>
	/// <param name="at"></param>
	/// <param name="radius"></param>
	/// <param name="chunk"></param>
	/// <returns></returns>
	public List<Vector3> FindAllStarsInSphere(Vector3 at, float radius, IStarChunkData[] chunks)
	{
		if (chunks == null || chunks.Length == 0)
		{
			GD.PrintErr("StarFinder: FindAllStarsInSphere(). Chunks array is null or empty. Exiting.");
			return null;
		}

		List<Vector3> foundStars = new List<Vector3>();

		foreach (IStarChunkData chunk in chunks)
		{
			Vector3 chunkMin = new Vector3(
				chunk.pos.x * chunk.size,
				chunk.pos.y * chunk.size,
				chunk.pos.z * chunk.size
			);

			Vector3 chunkMax = chunkMin + new Vector3(chunk.size, chunk.size, chunk.size);

			if (SphereIntersectsCube(at, radius, chunkMin, chunkMax))
			{
				foreach (Vector3 starPos in chunk.stars)
				{
					if ((starPos - at).Length() < radius)
					{
						if (debugPrint) GD.Print($"StarFinder: Found proximity star at {starPos}, {at.DistanceTo(starPos)} LY away from player");
						foundStars.Add(starPos);
					}
				}
			}
		}

		return foundStars;
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

	/// <summary>
	/// Checks if a sphere intersects a cube defined by its min and max corners.
	/// </summary>
	/// <param name="sphereCenter"></param>
	/// <param name="sphereRadius"></param>
	/// <param name="cubeMin"></param>
	/// <param name="cubeMax"></param>
	/// <returns></returns>
	private bool SphereIntersectsCube(Vector3 sphereCenter, float sphereRadius, Vector3 cubeMin, Vector3 cubeMax)
	{
		// Closest point, from sphere center to the cube
		float closestX = Mathf.Clamp(sphereCenter.X, cubeMin.X, cubeMax.X);
		float closestY = Mathf.Clamp(sphereCenter.Y, cubeMin.Y, cubeMax.Y);
		float closestZ = Mathf.Clamp(sphereCenter.Z, cubeMin.Z, cubeMax.Z);

		// Distance from cloest point to sphere center
		float distanceSquared = 
			(sphereCenter.X - closestX) * (sphereCenter.X - closestX) +
			(sphereCenter.Y - closestY) * (sphereCenter.Y - closestY) +
			(sphereCenter.Z - closestZ) * (sphereCenter.Z - closestZ);

		// If the distance is less than or equal to the sphere's radius squared, they intersect
		bool intersects = distanceSquared <= (sphereRadius * sphereRadius);
		return intersects;
	}
}
