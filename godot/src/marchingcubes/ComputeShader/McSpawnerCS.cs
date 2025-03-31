using Godot;
using System;

[Tool]
public partial class McSpawnerCS : Node
{
	FastNoiseLite _noise = new FastNoiseLite();
	private int _radius = 128;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var data = GenerateDataPoints();
		var sw = new System.Diagnostics.Stopwatch();
		MarchingCube marchingCube = new MarchingCube(method:MarchingCube.GenerationMethod.Cpu);
		MarchingCube marchingCube1 = new MarchingCube(method:MarchingCube.GenerationMethod.Gpu); 
		
		sw.Start();
		var meshInstance = marchingCube.GenerateMesh(data);
		sw.Stop();
		GD.Print("Time to generate vertices (CPU): ", sw.ElapsedMilliseconds, "ms");

		sw.Reset();
		sw.Start();
		var meshInstance2 = marchingCube1.GenerateMesh(data);
		sw.Stop();
		GD.Print("Time to generate vertices (GPU): ", sw.ElapsedMilliseconds, "ms");

		meshInstance2.Translate(new Vector3(0, 0, _radius * 2));
		AddChild(meshInstance);
		AddChild(meshInstance2);
		

	}
	private float[,,] GenerateDataPoints()
	{
		var radius = _radius;
		var diameter = radius * 2 + 1;
		var dataPoints = new float[diameter, diameter, diameter];
		
		Vector3 center = new Vector3(radius, radius, radius);
		
		for (var x = 0; x < diameter; x++)
		{
			for (var y = 0; y < diameter; y++)
			{
				for (var z = 0; z < diameter; z++)
				{
					Vector3 point = new Vector3(x, y, z);
					float distance = center.DistanceTo(point);
                
					// Adjust threshold to match radius more precisely
					// Using radius - 0.5 creates a more visually accurate sphere
					float value = (radius - 0.5f) - distance;
                
					// Clamp to ensure values stay in [-1, 1] range
					value = Mathf.Clamp(value, -1.0f, 1.0f);
                
					dataPoints[x, y, z] = value;
				}
			}
		}
		return dataPoints;
	}
	
}
