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
	[Export]
	public Vector3I Offset
	{
		get => _offset;
		set
		{
			if (value != _offset)
			{
				_offset = value;
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
	private MeshInstance3D _boundingBox;
	private Vector3I _offset = Vector3I.Zero;

	public override void _Ready()
	{
		SpawnMesh();
	}

	private void SpawnMesh()
	{
		_maxDepth = 8;
		_baseVoxelSize = (_radius * 2) / (64 * 256);
		GD.Print("Max depth: ", _maxDepth);
		GD.Print("Base Voxel Size: ", _baseVoxelSize);
		GD.Print("Current Voxel Size: ", GetVoxelSize(_depth));
		_instance?.QueueFree();
		_marchingCube ??= new MarchingCube();
		var data = GenerateDataPoints();
		var scaleFactor = GetVoxelSize(_depth);
		_instance = _marchingCube.GenerateMesh(data, scale: scaleFactor);
		var offsetNew = (Vector3)_offset * (2 * _radius) * (1 / (float)Math.Pow(2, _depth));
		_instance.Translate(offsetNew);
		AddChild(_instance);
		DrawBoundingBox(Vector3.Zero, 
			_radius * (1 / (float)Math.Pow(2, _depth)));
	}


	private float[,,] GenerateDataPoints()
	{
		var resolution = _resolution;
		var radius = _radius;
		int size = 64 + 1;
		var dataPoints = new float[size, size, size];

		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				for (int z = 0; z < size; z++)
				{
					var offsetNew = (Vector3)_offset * (2 * _radius) * (1 / (float)Math.Pow(2, _depth));
					Vector3 worldPos = offsetNew + new Vector3(x, y, z) * GetVoxelSize(_depth);
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

	private void DrawBoundingBox(Vector3 center, float radius)
	{
		var offsetNew = (Vector3)_offset * (2 * _radius) * (1 / (float)Math.Pow(2, _depth));
		center = Vector3.One * radius + offsetNew;
		_boundingBox?.QueueFree();
		GD.Print("Bounding box center: ", center);
		GD.Print("Bounding box radius: ", radius);
		
		var mesh = new ImmediateMesh();
		mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
		var edges = new []
		{
			new Vector3(-radius, -radius, -radius),
			new Vector3(radius, -radius, -radius),
			new Vector3(radius, radius, -radius),
			new Vector3(-radius, radius, -radius),
			new Vector3(-radius, -radius, radius),
			new Vector3(radius, -radius, radius),
			new Vector3(radius, radius, radius),
			new Vector3(-radius, radius, radius)
		};
		var lines = new int[,]
		{
			{0, 1}, {1, 2}, {2, 3}, {3, 0},
			{4, 5}, {5, 6}, {6, 7}, {7, 4},
			{0, 4}, {1, 5}, {2, 6}, {3, 7}
		};
		for (int i = 0; i < lines.GetLength(0); i++)
		{
			mesh.SurfaceSetColor(new Color(255, 0, 0));
			mesh.SurfaceAddVertex(center + edges[lines[i, 0]]);
			mesh.SurfaceSetColor(new Color(255, 0, 0));
			mesh.SurfaceAddVertex(center + edges[lines[i, 1]]);
			
		}
		mesh.SurfaceEnd();
		_boundingBox = new MeshInstance3D();
		_boundingBox.Mesh = mesh;
		_boundingBox.MaterialOverride = new StandardMaterial3D
		{
			ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
			VertexColorUseAsAlbedo = true,
			DisableFog = true
		};
		_boundingBox.Name = "BoundingBox";
		AddChild(_boundingBox);
		
	}
		
	
}
