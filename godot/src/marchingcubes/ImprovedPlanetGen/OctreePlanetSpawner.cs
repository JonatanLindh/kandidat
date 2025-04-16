using Godot;
using System;

[Tool]
public partial class OctreePlanetSpawner : Node
{
	
	// Testing variables
	private Node3D _rootTest;
	private Vector3 _center = Vector3.Zero;
	private int _depth = 0;
	[Export]
	public Vector3 Center
	{
		get => _center;
		set
		{
			if (value != _center)
			{
				_center = value;
				Init();
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
				Init();
			}
		}
	}

	
	
	private int _maxDepth = 8;
	private float _baseVoxelSize;
	private float _radius = 32;
	private int _resolution = 64;
	private MarchingCube _marchingCube;
	
	public override void _Ready()
	{
		Init();
		/*
		_marchingCube = new MarchingCube();
		_baseVoxelSize = (_radius * 2) / (_resolution * Mathf.Pow(2, _maxDepth));
		*/
		
		// Initialize the Octree with a max depth
		// Ex: Octree octree = new Octree(max depth = 8)
		
		
		// This is just testing
		//var depth = 2;
		//var size = (1 /  Mathf.Pow(2, depth)) * (_radius * 2);
		//SpawnChunk(new Vector3(_radius / 2, _radius / 2, _radius  / 2), size, depth);
	}


	
	
	// Called from the Octree whenever it subdivides a chunk
	// Should give the center of the chunk, the size of the chunk and the current depth
	// depth = 0 would represent the root chunk
	// If possible the center should be in local space
	public void SpawnChunk(Vector3 center, float size, int depth)
	{
		
		// Give the bottom left corner of the chunk
		var offset = center - (Vector3.One * size / 2);
		
		var data = GenerateDataPoints(offset, depth);
		var scaleFactor = GetVoxelSize(depth);
		var mesh = _marchingCube.GenerateMesh(data, scale: scaleFactor);
		var instance = new MeshInstance3D();
		instance.Mesh = mesh;
		instance.MaterialOverride = new StandardMaterial3D()
		{
			CullMode = BaseMaterial3D.CullModeEnum.Disabled
		};
		Transform3D transform3D = new Transform3D();
		transform3D.Origin = center;
		transform3D.Basis = Basis.Identity;
		instance.Transform = transform3D;
		
		_rootTest.AddChild(instance);
	}

	// Either the octree handles mesh instances and remove them when we unload the chunk
	// Or this class can handle the mesh instances and remove them when we unload the chunk
	public void UnloadChunk()
	{
		
	}
	
	
	private float GetVoxelSize(int depth) {
		int maxDepth = _maxDepth;
		float baseVoxelSize = _baseVoxelSize;
		return Mathf.Pow(2, (maxDepth - depth)) * baseVoxelSize;
	}
	
	
	// Testing functions
	private void Init()
	{
		_rootTest?.QueueFree();
		_marchingCube ??= new MarchingCube();
		_baseVoxelSize = (_radius * 2) / (_resolution * Mathf.Pow(2, _maxDepth));
		_rootTest = new Node3D();
		var size = (1 /  Mathf.Pow(2, _depth)) * (_radius * 2);
		AddChild(_rootTest);
		SpawnChunk(_center, size, _depth);
		DrawBoundingBox(_center, size);
	}
	
	private void DrawBoundingBox(Vector3 center, float size)
	{
		var edges = new[]
		{
			new[] { 0, 1 }, new[] { 1, 2 }, new[] { 2, 3 }, new[] { 3, 0 },
			new[] { 4, 5 }, new[] { 5, 6 }, new[] { 6, 7 }, new[] { 7, 4 },
			new[] { 0, 4 }, new[] { 1, 5 }, new[] { 2, 6 }, new[] { 3, 7 }
		};
		var corners = new[]
		{
			new Vector3(center.X - size / 2, center.Y - size / 2, center.Z - size / 2),
			new Vector3(center.X + size / 2, center.Y - size / 2, center.Z - size / 2),
			new Vector3(center.X + size / 2, center.Y + size / 2, center.Z - size / 2),
			new Vector3(center.X - size / 2, center.Y + size / 2, center.Z - size / 2),
			new Vector3(center.X - size / 2, center.Y - size / 2, center.Z + size / 2),
			new Vector3(center.X + size / 2, center.Y - size / 2, center.Z + size / 2),
			new Vector3(center.X + size / 2, center.Y + size / 2, center.Z + size / 2),
			new Vector3(center.X - size / 2, center.Y + size / 2, center.Z + size / 2)
		};
		
		
		ImmediateMesh boundingBox = new ImmediateMesh();
		boundingBox.SurfaceBegin(Mesh.PrimitiveType.Lines);
		
		foreach (var edge in edges)
		{
			boundingBox.SurfaceSetColor(new Color(1, 0, 0));
			boundingBox.SurfaceAddVertex(corners[edge[0]]);
			boundingBox.SurfaceSetColor(new Color(1, 0, 0));
			boundingBox.SurfaceAddVertex(corners[edge[1]]);
		}
		boundingBox.SurfaceEnd();
		MeshInstance3D boundingBoxInstance = new MeshInstance3D();
		boundingBoxInstance.Mesh = boundingBox;
		boundingBoxInstance.MaterialOverride = new StandardMaterial3D()
		{
			ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
			VertexColorUseAsAlbedo = true,
			DisableFog = true
		};
		_rootTest.AddChild(boundingBoxInstance);
	}
	
	private float[,,] GenerateDataPoints(Vector3 offset , int depth)
	{
		var radius = _radius;
		int size = _resolution + 1;
		var dataPoints = new float[size, size, size];
		float voxelSize = GetVoxelSize(depth);

		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				for (int z = 0; z < size; z++)
				{
					Vector3 worldPos = new Vector3(x, y, z) * voxelSize;
					worldPos += offset;
					var value = -Sphere(worldPos, Vector3.Zero, radius);
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
	
	
}
