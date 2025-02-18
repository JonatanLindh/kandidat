using Godot;
using System;
using Godot.Collections;

public partial class McSpawner : Node
{
	private MarchingCube mc;
	[Export] public int CHUNK_SIZE { get; set; } = 16;
	[Export] public int scale { get; set; } = 1;
	[Export] int RENDER_DISTANCE { get; set; } = 1;
	[Export] Node3D player { get; set; }

	
	private Vector3 playerposition;
	private FastNoiseLite noise;
	private Dictionary<String, MeshInstance3D> loaded_chunks;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		loaded_chunks = new Dictionary<string, MeshInstance3D>();
		
		noise = new FastNoiseLite();
		noise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);
		noise.SetSeed(31541);
		noise.SetFrequency(0.1f);
		noise.SetFractalType(FastNoiseLite.FractalTypeEnum.Fbm);
		
		LoadChunk(0,0);
		LoadChunk(1,0);
		
		playerposition = player.GlobalTransform.Origin;
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		playerposition = player.GlobalTransform.Origin;
		
		var player_chunk_x = (int)(playerposition.X / (CHUNK_SIZE));
		var player_chunk_z = (int)(playerposition.Z / (CHUNK_SIZE));

		for (int x = (player_chunk_x - RENDER_DISTANCE); x < (player_chunk_x + RENDER_DISTANCE) + 1; x++)
		{
			for (int z = (player_chunk_z - RENDER_DISTANCE); z < (player_chunk_z + RENDER_DISTANCE) + 1; z++)
			{
				var chunk_key = x.ToString() + "," + z.ToString();
				if (loaded_chunks.TryGetValue(chunk_key, out var chunk_instance))
				{
					//GD.Print("x: " + x + " z: " + z + "already loaded");
					chunk_instance.Position = new Vector3(x, 0, z) * (CHUNK_SIZE - 1);
				}
				else
				{
					//GD.Print("x: " + x + " z: " + z + " loaded");
					LoadChunk(x, z);
				}
			}
		}

		
		foreach (var key in loaded_chunks.Keys)
		{
			var coords = key.Split(",");
			var chunk_x = coords[0].ToInt();
			var chunk_z = coords[1].ToInt();
			if(Math.Abs(chunk_x - player_chunk_x) > RENDER_DISTANCE || Math.Abs(chunk_z - player_chunk_z) > RENDER_DISTANCE)
			{
				UnloadChunk(chunk_x, chunk_z);
			}
		}
		
	}

	private float[,,] GenerateDataPoints(int resolution, Vector3 offset)
	{
		float[,,] dataPoints = new float[resolution, resolution, resolution];
		
		for (int x = 0; x < resolution; x++)
		{
			for (int y = 0; y < resolution; y++)
			{
				for (int z = 0; z < resolution; z++)
				{
					float noi = noise.GetNoise3D(x + offset.X, y + offset.Y, z + offset.Z);
					float value = noi;
					dataPoints[x, y, z] = value;
				}
			}
		}
		return dataPoints;
	}

	private void LoadChunk(int x, int z)
	{
		var chunk_instance = GenerateChunkMesh(x, z);
		chunk_instance.Position = new Vector3(x, 0, z) * (CHUNK_SIZE - 1);
		AddChild(chunk_instance);
		loaded_chunks.Add(x + "," + z, chunk_instance);
	}
	private void UnloadChunk(int x, int z)
	{
		var chunk_key = x + "," + z;
		if (loaded_chunks.TryGetValue(chunk_key, out var chunk_instance))
		{
			RemoveChild(chunk_instance);
			loaded_chunks.Remove(chunk_key);
			GD.Print("x: " + x + " z: " + z + " unloaded");
		}
	}

	private MeshInstance3D GenerateChunkMesh(int x, int z)
	{
		var datapoints = GenerateDataPoints(CHUNK_SIZE, new Vector3(x, 0, z) * (CHUNK_SIZE - 1));	
		mc = new MarchingCube(datapoints, scale);
		return mc.GenerateMesh();
	}
	
	// For debugging
	private void RenderGrid(float[,,] datapoints, Vector3 offset)
	{
		Node3D instance = new Node3D();
		AddChild(instance);
		// Spawn spheres at each voxel
		for (int x = 0; x < CHUNK_SIZE; x++)
		{
			for (int y = 0; y < CHUNK_SIZE; y++)
			{
				for (int z = 0; z < CHUNK_SIZE; z++)
				{
					// Create a new MeshInstance3D
					MeshInstance3D sphereInstance = new MeshInstance3D();

					// Create a new SphereMesh
					SphereMesh sphereMesh = new SphereMesh();
					sphereMesh.Radius = 1.0f;
					sphereMesh.Height = 2.0f;

					if (datapoints[x, y, z] >= 0)
					{
						// Set the color to red
						sphereInstance.MaterialOverride = new StandardMaterial3D();
						((StandardMaterial3D)sphereInstance.MaterialOverride).AlbedoColor = new Color(1, 0, 0);
					}
					
					// Assign the SphereMesh to the MeshInstance3D
					sphereInstance.Mesh = sphereMesh;
					Vector3 position = new Vector3(x * scale, y * scale, z * scale);
					sphereInstance.Position = position + offset;
					sphereInstance.Scale = new Vector3(0.1f * scale, 0.1f * scale, 0.1f * scale);
					
					// Add the MeshInstance3D to the scene tree
					instance.AddChild(sphereInstance);
				}
			}
		}
	}
	
}
