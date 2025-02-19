using Godot;
using System;
using System.ComponentModel;
using Godot.Collections;

// TODO:
// Add multithreading to the chunk loading (and unloading)

[Tool]
public partial class McSpawner : Node
{
	private MarchingCube mc;
	
	
	[ExportCategory("Chunk Settings")]
	[Export] public int CHUNK_SIZE
	{
		get => chunk_size;
		set
		{
			chunk_size = value;
			OnResourceSet();
		} 
		
	} 

	[Export] public int MAX_HEIGHT 
	{ 
		get => max_height; 
		set
		{
			max_height = value;
			OnResourceSet();
		}
	}
	[Export] int RENDER_DISTANCE { 
		get => render_distance;
		set
		{
			render_distance = value;
			OnResourceSet();
		} 
	}
	[Export] Node3D player { get; set; }
	
	
	[ExportCategory("Noise Settings")]
	[Export] FastNoiseLite Noise
	{
		get => _noise;
		set
		{
			_noise = value;
			OnResourceSet();
		}
	}

	[Export] float Amplitude
	{
		get => _amplitude;
		set
		{
			_amplitude = value;
			OnResourceSet();
		}
	}
	[Export] float Frequency
	{
		get => _frequency;
		set
		{
			_frequency = value;
			OnResourceSet();
		}
	}
	[Export] float Octaves
	{
		get => _octaves;
		set
		{
			_octaves = value;
			OnResourceSet();
		}
	}

	private float _amplitude = 1.0f;
	private float _frequency = 0.01f;
	private float _octaves = 4.0f;
	

	private FastNoiseLite _noise;

	private int chunk_size = 16;
	private int max_height = 16;
	private int render_distance = 1;
	private Vector3 playerposition;
	private Dictionary<String, MeshInstance3D> loaded_chunks;
	private Node3D editorCamera;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		loaded_chunks = new Dictionary<string, MeshInstance3D>();
		if (Engine.IsEditorHint())
		{
			editorCamera = EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D();
			playerposition = editorCamera.GlobalTransform.Origin;
		}
		else
		{
			playerposition = player.GlobalTransform.Origin;
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			if(editorCamera != null)
			{ 
				ChunkProcess();
			}
		}
		else
		{
			ChunkProcess();
		}

	}

	private void ChunkProcess()
	{
		if (Engine.IsEditorHint())
		{
			playerposition = editorCamera.GlobalTransform.Origin;
		}
		else
		{
			playerposition = player.GlobalTransform.Origin;
		}
		var player_chunk_x = (int)(playerposition.X / (chunk_size));
		var player_chunk_z = (int)(playerposition.Z / (chunk_size));

		for (int x = (player_chunk_x - render_distance); x < (player_chunk_x + render_distance) + 1; x++)
		{
			for (int z = (player_chunk_z - render_distance); z < (player_chunk_z + render_distance) + 1; z++)
			{
				var chunk_key = x.ToString() + "," + z.ToString();
				if (loaded_chunks.TryGetValue(chunk_key, out var chunk_instance))
				{
					chunk_instance.Position = new Vector3(x, 0, z) * (chunk_size - 1);
				}
				else
				{
					LoadChunk(x, z);
				}
			}
		}
		foreach (var key in loaded_chunks.Keys)
		{
			var coords = key.Split(",");
			var chunk_x = coords[0].ToInt();
			var chunk_z = coords[1].ToInt();
			if(Math.Abs(chunk_x - player_chunk_x) > render_distance || Math.Abs(chunk_z - player_chunk_z) > render_distance)
			{
				UnloadChunk(chunk_x, chunk_z);
			}
		}
	}
	

	private void LoadChunk(int x, int z)
	{
		var chunk_instance = GenerateChunkMesh(x, z);
		chunk_instance.Position = new Vector3(x, 0, z) * (chunk_size - 1);
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
			//GD.Print("x: " + x + " z: " + z + " unloaded");
		}
	}

	
	public float[,,] GenerateDataPoints(Vector3I resolution, Vector3 offset)
	{
		float[,,] dataPoints = new float[resolution.X, resolution.Y, resolution.Z];
		
		for (int x = 0; x < resolution.X; x++)
		{
			for (int y = 0; y < resolution.Y; y++)
			{
				for (int z = 0; z < resolution.Z; z++)
				{
					float value = 0;
					float amp = _amplitude;
					float freq = _frequency;
					for (int i = 0; i < _octaves; i++)
					{
						value += _noise.GetNoise3D(x + offset.X, y + offset.Y, z + offset.Z);
						amp *= 0.5f;
						freq *= 2.0f;
					}

					value = Mathf.Clamp(value, -1.0f, 1.0f);
					dataPoints[x, y, z] = value;
				}
			}
		}
		return dataPoints;
	}
	private MeshInstance3D GenerateChunkMesh(int x, int z)
	{
		var offset = new Vector3(x, 0, z) * (chunk_size - 1);
		var datapoints = GenerateDataPoints(new Vector3I(chunk_size, max_height, chunk_size), offset);
		
		mc = new MarchingCube(datapoints, 1);
		var meshinstance = mc.GenerateMesh();
		
		// disable backface culling
		meshinstance.MaterialOverride = new StandardMaterial3D();
		((StandardMaterial3D)meshinstance.MaterialOverride).SetCullMode(BaseMaterial3D.CullModeEnum.Disabled);
		
		return meshinstance;
	}

	private void OnResourceSet()
	{
		ReloadChunks();
	}

	private void ReloadChunks()
	{
		if(loaded_chunks == null) return;
		foreach (var key in loaded_chunks.Keys)
		{
			var coords = key.Split(",");
			var chunk_x = coords[0].ToInt();
			var chunk_z = coords[1].ToInt();
			UnloadChunk(chunk_x, chunk_z);
			LoadChunk(chunk_x, chunk_z);
		}
	}
	
	

	
}
