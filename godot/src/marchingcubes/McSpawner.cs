using Godot;
using System;

public partial class McSpawner : Node
{
	MarchingCube mc;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var datapoints = GenerateDataPoints(16);
		mc = new MarchingCube(datapoints);
		AddChild(mc.GenerateMesh());
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private float[,,] GenerateDataPoints(int resolution, int scale = 1)
	{
		float[,,] dataPoints = new float[resolution, resolution, resolution];
		
		FastNoiseLite noise = new FastNoiseLite();
		noise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);
		noise.SetSeed(31541);
		noise.SetFrequency(0.1f);
		noise.SetFractalType(FastNoiseLite.FractalTypeEnum.Fbm);
		
		for (int x = 0; x < resolution; x++)
		{
			for (int y = 0; y < resolution; y++)
			{
				for (int z = 0; z < resolution; z++)
				{
					float noi = (noise.GetNoise3D(x, y, z) + 1) * 0.5f;
					float value = noi;
					dataPoints[x, y, z] = value;
				}
			}
		}
		return dataPoints;
	}
	
	

	
	
}
