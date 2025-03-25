using Godot;
using System;

[Tool]
public partial class McSpawnerCS : Node
{
	FastNoiseLite _noise = new FastNoiseLite();
	private int _radius = 32;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var data = GetNoise();
		var sw = new System.Diagnostics.Stopwatch();
		MarchingCube marchingCube = new MarchingCube(method:MarchingCube.GenerationMethod.Gpu);
		MarchingCube marchingCube1 = new MarchingCube(method:MarchingCube.GenerationMethod.Cpu); 
		
		sw.Start();
		var meshInstance = marchingCube.GenerateMesh(data);
		sw.Stop();
		GD.Print("Time to generate vertices (GPU): ", sw.ElapsedMilliseconds, "ms");

		sw.Start();
		var meshInstance2 = marchingCube1.GenerateMesh(data);
		sw.Stop();
		GD.Print("Time to generate vertices (CPU): ", sw.ElapsedMilliseconds, "ms");

		meshInstance2.Translate(new Vector3(0, 0, _radius * 2));
		AddChild(meshInstance);
		AddChild(meshInstance2);
		

	}
	private float[,,] GetNoise()
	{
		int radius = _radius;
		int diameter = radius + radius;
		float[,,] points = new float[diameter, diameter, diameter];

		Vector3 centerPoint = new Vector3I(diameter, diameter, diameter) / 2;

		Random random = new Random();
		Vector3 offset = new Vector3(random.Next(diameter), random.Next(diameter), random.Next(diameter));

		// creates a cube of points
		for (var x = 0; x < diameter; x++)
		{
			for (var y = 0; y < diameter; y++)
			{
				for (var z = 0; z < diameter; z++)
				{
					Vector3 currentPosition = new Vector3I(x, y, z);
					float distanceToCenter = (centerPoint - currentPosition).Length();

					// see if the point inside or outside the sphere
					if (distanceToCenter < radius)
					{
						// noise-value between 0-1 - will be closer to 1 when the point is close to the surface 
						float noiseValue = GetNoise3Dv(currentPosition + offset); //* (distanceToCenter / ((float) radius) + 1);
						points[x, y, z] = noiseValue;
					}
					else
					{
						// if not inside the sphere, just set noise-value to -1 to discard it later
						points[x, y, z] = -1.0f;
					}

				}
			}
		}
		return points;
	}
	public float GetNoise3Dv(Vector3 pos)
	{
		// FastNoiseLite.GetNoise returns a value between [-1, 1]. It feels more natural with [0, 1], hence the absolute value.
		// The distribution of values should still be the same
		return Mathf.Abs(_noise.GetNoise3Dv(pos));
	}

	
	
}
