using Godot;
using System;
using System.ComponentModel;
using Godot.Collections;

// TODO:
// Add multithreading to the chunk loading (and unloading)

/// <summary>
/// Spawns a Marching Cube mesh in the scene with chunking
/// </summary>
[Tool]
public partial class McChunkSpawner : Node
{
	[ExportCategory("Chunk Settings")]
	[Export] public int ChunkSize
	{
		get => _chunkSize;
		set
		{
			_chunkSize = value;
			OnResourceSet();
		} 
		
	} 

	[Export] public int MaxHeight 
	{ 
		get => _maxHeight; 
		set
		{
			_maxHeight = value;
			OnResourceSet();
		}
	}
	[Export] public int RenderDistance { 
		get => _renderDistance;
		set
		{
			_renderDistance = value;
			OnResourceSet();
		} 
	}
	[Export] public Node3D Player { get; set; }
	
	
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
	
	private MarchingCube _marchingCube;
	private FastNoiseLite _noise;
	private int _chunkSize = 16;
	private int _maxHeight = 16;
	private int _renderDistance = 1;
	private Vector3 _playerPosition;
	private Dictionary<string, MeshInstance3D> _loadedChunks;
	private Node3D _editorCamera;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_marchingCube = new MarchingCube();
		
		_loadedChunks = new Dictionary<string, MeshInstance3D>();
		if (Engine.IsEditorHint())
		{
			_editorCamera = EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D();
			_playerPosition = _editorCamera.GlobalTransform.Origin;
		}
		else
		{
			_playerPosition = Player.GlobalTransform.Origin;
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			if(_editorCamera != null)
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
		_playerPosition = Engine.IsEditorHint() ? _editorCamera.GlobalTransform.Origin : Player.GlobalTransform.Origin;
		var playerChunkX = (int)(_playerPosition.X / (_chunkSize));
		var playerChunkZ = (int)(_playerPosition.Z / (_chunkSize));

		for (var x = playerChunkX - _renderDistance; x < playerChunkX + _renderDistance + 1; x++)
		{
			for (var z = playerChunkZ - _renderDistance; z < playerChunkZ + _renderDistance + 1; z++)
			{
				var chunkKey = x + "," + z;
				if (_loadedChunks.TryGetValue(chunkKey, out var chunkInstance))
				{
					chunkInstance.Position = new Vector3(x, 0, z) * (_chunkSize - 1);
				}
				else
				{
					LoadChunk(x, z);
				}
			}
		}
		foreach (var key in _loadedChunks.Keys)
		{
			var coords = key.Split(",");
			var chunkX = coords[0].ToInt();
			var chunkZ = coords[1].ToInt();
			if(Math.Abs(chunkX - playerChunkX) > _renderDistance || Math.Abs(chunkZ - playerChunkZ) > _renderDistance)
			{
				UnloadChunk(chunkX, chunkZ);
			}
		}
	}
	private void LoadChunk(int x, int z)
	{
		var chunkInstance = GenerateChunkMesh(x, z);
		chunkInstance.Position = new Vector3(x, 0, z) * (_chunkSize - 1);
		AddChild(chunkInstance);
		_loadedChunks.Add(x + "," + z, chunkInstance);
	}
	private void UnloadChunk(int x, int z)
	{
		var chunkKey = x + "," + z;
		if (_loadedChunks.TryGetValue(chunkKey, out var chunkInstance))
		{
			RemoveChild(chunkInstance);
			_loadedChunks.Remove(chunkKey);
		}
	}

	
	private float[,,] GenerateDataPoints(Vector3I resolution, Vector3 offset)
	{
		var dataPoints = new float[resolution.X, resolution.Y, resolution.Z];
		
		for (var x = 0; x < resolution.X; x++)
		{
			for (var y = 0; y < resolution.Y; y++)
			{
				for (var z = 0; z < resolution.Z; z++)
				{
					float value = 0;
					value += _noise.GetNoise3D(x + offset.X, y + offset.Y, z + offset.Z);
					value = Mathf.Clamp(value, -1.0f, 1.0f);
					dataPoints[x, y, z] = value;
				}
			}
		}
		return dataPoints;
	}
	private MeshInstance3D GenerateChunkMesh(int x, int z)
	{
		var offset = new Vector3(x, 0, z) * (_chunkSize - 1);
		var datapoints = GenerateDataPoints(new Vector3I(_chunkSize, _maxHeight, _chunkSize), offset);
		var meshInstance3D = _marchingCube.GenerateMesh(datapoints);
		
		// Disable backface culling
		meshInstance3D.MaterialOverride = new StandardMaterial3D();
		((StandardMaterial3D)meshInstance3D.MaterialOverride).SetCullMode(BaseMaterial3D.CullModeEnum.Disabled);
		
		return meshInstance3D;
	}

	private void OnResourceSet()
	{
		ReloadChunks();
	}

	private void ReloadChunks()
	{
		if(_loadedChunks == null) return;
		foreach (var key in _loadedChunks.Keys)
		{
			var coords = key.Split(",");
			var chunkX = coords[0].ToInt();
			var chunkZ = coords[1].ToInt();
			UnloadChunk(chunkX, chunkZ);
			LoadChunk(chunkX, chunkZ);
		}
	}
	
	

	
}
