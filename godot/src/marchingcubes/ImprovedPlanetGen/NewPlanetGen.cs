using Godot;
using System;
using System.Numerics;
using Vector3 = Godot.Vector3;

[Tool]
public partial class NewPlanetGen : Node3D
{
	[Export]
	public float Radius
	{
		get => _radius;
		set
		{
			if (value != _radius)
			{
				_radius = value;
				SpawnMesh();
			}
		}
	}
	[Export]
	public int Resolution
	{
		get => _resolution;
		set
		{
			if (value != _resolution)
			{
				_resolution = value;
				SpawnMesh();
			}
		}
	}
	[Export]
	public int Depth
	{
		get => _depth;
		set
		{
			if (value != _depth)
			{
				_depth = value;
				SpawnMesh();
			}
		}
	}
	
	private float _radius = 32;
	private int _resolution = 64;
	private MarchingCube _marchingCube;
	private MeshInstance3D _instance;
	
	CelestialBodyInput _input;
	private float _voxelSize = 1f;
	private int _depth = 0;
	private int _maxDepth;
	private float _baseVoxelSize;

	public override void _Ready()
	{
		SpawnMesh();
	}

	private void SpawnMesh()
	{
		//_maxDepth = (int)Math.Log2(_radius);
		_maxDepth = 8;
		_baseVoxelSize = (_radius * 2) / (64 * 256);
		GD.Print("Max depth: ", _maxDepth);
		GD.Print("Base Voxel Size: ", _baseVoxelSize);
		GD.Print("Current Voxel Size: ", GetVoxelSize(_depth));
		_instance?.QueueFree();
		_marchingCube ??= new MarchingCube();
		var data = GenerateDataPoints();
		//var scaleFactor = (_radius * 2) / _resolution;
		var scaleFactor = GetVoxelSize(_depth);
		_instance = _marchingCube.GenerateMesh(data, scale: scaleFactor);
		AddChild(_instance);
	}


	private float[,,] GenerateDataPoints()
	{
		var resolution = _resolution;
		var radius = _radius;
		int size = 64 + 5;
		var dataPoints = new float[size, size, size];

		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				for (int z = 0; z < size; z++)
				{
					Vector3 worldPos = new Vector3(x, y, z) * GetVoxelSize(_depth);
					var value = -Sphere(worldPos, Vector3.One * radius, radius);
					value = Mathf.Clamp(value, -1.0f, 1.0f);
					dataPoints[x, y, z] = value;
				}
			}
		}
		return dataPoints;
	}
	
	private static float Sphere(Vector3 worldPos, Vector3 origin, float radius) {
		return (worldPos - origin).Length() - radius;
	}
	
	private float GetVoxelSize(int depth) {
		int MAX_DEPTH = 8;
		float BASE_VOXEL_SIZE = _baseVoxelSize;
		return Mathf.Pow(2, (MAX_DEPTH - depth)) * BASE_VOXEL_SIZE;
	}
	
}
