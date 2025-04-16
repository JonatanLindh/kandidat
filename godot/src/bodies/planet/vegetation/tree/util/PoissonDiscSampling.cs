using Godot;
using System;
using System.Collections.Generic;

public static class PoissonDiscSampling2D
{

	public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
	{
		float cellSize = radius / Mathf.Sqrt(2);
		
		int[,] grid = new int[
			Mathf.CeilToInt(sampleRegionSize.X / cellSize), 
			Mathf.CeilToInt(sampleRegionSize.Y / cellSize)
		];
		List<Vector2> points = new List<Vector2>();
		List<Vector2> spawnPoints = new List<Vector2>();
		
		Vector2 firstPoint = new Vector2(
			GD.Randf() * sampleRegionSize.X, 
			GD.Randf() * sampleRegionSize.Y
		);
		spawnPoints.Add(firstPoint);
		
		while (spawnPoints.Count > 0)
		{
			int spawnIndex = GD.RandRange(0, spawnPoints.Count - 1);
			Vector2 spawnCenter = spawnPoints[spawnIndex];
			bool pointFound = false;
			
			for (int i = 0; i < numSamplesBeforeRejection; i++)
			{
				float angle = GD.Randf() * Mathf.Pi * 2;
				Vector2 dir = new Vector2(
					Mathf.Sin(angle), 
					Mathf.Cos(angle)
				);
				Vector2 newPoint = spawnCenter + dir * (float)GD.RandRange(radius, 2 * radius);
				
				if (IsValidPoint(newPoint, sampleRegionSize, cellSize, radius, points, grid))
				{
					points.Add(newPoint);
					spawnPoints.Add(newPoint);
					grid[
						Mathf.FloorToInt(newPoint.X / cellSize), 
						Mathf.FloorToInt(newPoint.Y / cellSize)
					] = points.Count;
					pointFound = true;
					break;
				}
			}
			if (!pointFound)
			{
				spawnPoints.RemoveAt(spawnIndex);
			}
		}
		return points;
	}

	private static bool IsValidPoint(Vector2 newPoint, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
	{
		if (newPoint.X >= 0 && newPoint.X < sampleRegionSize.X &&
			newPoint.Y >= 0 && newPoint.Y < sampleRegionSize.Y)
		{
			int cellX = Mathf.FloorToInt(newPoint.X / cellSize);
			int cellY = Mathf.FloorToInt(newPoint.Y / cellSize);
			
			int searchStartX = Mathf.Max(cellX - 2, 0);
			int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
			
			int searchStartY = Mathf.Max(cellY - 2, 0);
			int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);
			
			for (int x = searchStartX; x <= searchEndX; x++)
			{
				for (int y = searchStartY; y <= searchEndY; y++)
				{
					int pointIndex = grid[x, y] - 1;
					if (pointIndex != -1)
					{
						float distanceSquared = newPoint.DistanceSquaredTo(points[pointIndex]);
						if (distanceSquared < (radius * radius))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		return false;
	}
}

public static class PoissonDiscSampling3D
{ 
	public static List<Vector3> GeneratePoints(float radius, Vector3 sampleRegionSize, int numSamplesBeforeRejection = 30)
    {
        float cellSize = radius / Mathf.Sqrt(3);

        int[,,] grid = new int[
            Mathf.CeilToInt(sampleRegionSize.X / cellSize),
            Mathf.CeilToInt(sampleRegionSize.Y / cellSize),
            Mathf.CeilToInt(sampleRegionSize.Z / cellSize)
        ];

        List<Vector3> points = new List<Vector3>();
        List<Vector3> spawnPoints = new List<Vector3>();

        Vector3 firstPoint = new Vector3(
            GD.Randf() * sampleRegionSize.X,
            GD.Randf() * sampleRegionSize.Y,
            GD.Randf() * sampleRegionSize.Z
        );
        spawnPoints.Add(firstPoint);


        while (spawnPoints.Count > 0)
        {
	        int spawnIndex = GD.RandRange(0, spawnPoints.Count - 1);
            Vector3 spawnCenter = spawnPoints[spawnIndex];
            bool pointFound = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
	            float angle1 = Mathf.Acos(2 * GD.Randf() - 1); 
	            float angle2 = GD.Randf() * Mathf.Pi * 2; 
	            Vector3 dir = new Vector3(
		            Mathf.Sin(angle1) * Mathf.Cos(angle2),
		            Mathf.Sin(angle1) * Mathf.Sin(angle2),
		            Mathf.Cos(angle1)
	            );
                Vector3 newPoint = spawnCenter + dir * (float)GD.RandRange(radius, 2 * radius);
                if (IsValidPoint(newPoint, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(newPoint);
                    spawnPoints.Add(newPoint);
                    grid[
                        Mathf.FloorToInt(newPoint.X / cellSize),
                        Mathf.FloorToInt(newPoint.Y / cellSize),
                        Mathf.FloorToInt(newPoint.Z / cellSize)
                    ] = points.Count;
                    pointFound = true;
                    break;
                }
            }
            if (!pointFound)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }
        return points;
    }

    private static bool IsValidPoint(Vector3 newPoint, Vector3 sampleRegionSize, float cellSize, float radius, List<Vector3> points, int[,,] grid)
    {
        if (newPoint.X >= 0 && newPoint.X < sampleRegionSize.X &&
            newPoint.Y >= 0 && newPoint.Y < sampleRegionSize.Y &&
            newPoint.Z >= 0 && newPoint.Z < sampleRegionSize.Z)
        {
            int cellX = Mathf.FloorToInt(newPoint.X / cellSize);
            int cellY = Mathf.FloorToInt(newPoint.Y / cellSize);
            int cellZ = Mathf.FloorToInt(newPoint.Z / cellSize);
            
            
            int searchStartX = Mathf.Max(cellX - 2, 0);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);

            int searchStartY = Mathf.Max(cellY - 2, 0);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            int searchStartZ = Mathf.Max(cellZ - 2, 0);
            int searchEndZ = Mathf.Min(cellZ + 2, grid.GetLength(2) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    for (int z = searchStartZ; z <= searchEndZ; z++)
                    {
                        int pointIndex = grid[x, y, z] - 1;
                        if (pointIndex != -1)
                        {
                            float distanceSquared = newPoint.DistanceSquaredTo(points[pointIndex]);
                            if (distanceSquared < (radius * radius))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }
}
