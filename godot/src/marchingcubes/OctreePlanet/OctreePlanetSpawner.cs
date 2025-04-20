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
	
	
	private CelestialBodyNoise celestialBody;
	private Node cb;
	[Export] private Node CelestialBody
	{
		get => cb;
		set
		{
			cb = value;
		}
	}

	
	
	private int _maxDepth = 8;
	private float _baseVoxelSize;
	private float _radius = 32;
	private int _resolution = 64;
	private MarchingCube _marchingCube;
	
	
	// Things from McSpawner
	private PlanetThemeGenerator _themeGenerator = new PlanetThemeGenerator();
	private ShaderMaterial _planetShader;
	public ShaderMaterial PlanetShader
	{
		get => _planetShader;
		set
		{
			_planetShader = value;
		}
	}

	
	public override void _Ready()
	{
		Init();

		// Initialize the Octree with a max depth
		// I assume I would call the octree by finding the octree node in the scene tree
		//Node octree = GetNode("Octree");
		// I assume I initialize the octree with the current instance of this class,
		// the max depth,
		// the base size of the octree
		// and also with the center of the octree
		//octree.Call("Init", this, _maxDepth, baseSize: (radius *2), center: Vector.Zero);
		
	}
	
	
	// Called from the Octree whenever it subdivides a chunk
	// Should give the center of the chunk, the size of the chunk and the current depth
	// depth = 0 would represent the root chunk
	// If possible the center should be in local space
	public void SpawnChunk(Vector3 center, float size, int depth)
	{
		
		// Give the bottom left (0,0,0) corner of the chunk
		var offset = center - (Vector3.One * size / 2);
		
		//var data = GenerateDataPoints(offset, depth);
		var scaleFactor = GetVoxelSize(depth);
		celestialBody.VoxelSize = scaleFactor;
		var data = celestialBody.GetNoise(offset);
		var mesh = _marchingCube.GenerateMesh(data, scale: scaleFactor, offset: Vector3.One * (size / 2));
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

		var requestInstance = new MeshInstance3D();
		requestInstance.Transform = transform3D;

		// Send the request to the MarchingCubeDispatch
		MarchingCubeRequest cubeRequest = new MarchingCubeRequest
		{
			PlanetDataPoints = celestialBody,
			Scale = scaleFactor,
			Offset = Vector3.One * (size / 2),
			Center = center,
			Root = _rootTest,	
			CustomMeshInstance = requestInstance,
			GeneratePlanetShader = GeneratePlanetShader
		};
		MarchingCubeDispatch.Instance.AddToQueue(cubeRequest);
		
		//_rootTest.AddChild(instance);
		
		// Maybe return the MeshInstance3D?
		// return instance;
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
		if (!IsInsideTree()) return;
		
		_rootTest?.QueueFree();
		_marchingCube ??= new MarchingCube();
		_baseVoxelSize = (_radius * 2) / (_resolution * Mathf.Pow(2, _maxDepth));
		_rootTest = new Node3D();
		var size = (1 /  Mathf.Pow(2, _depth)) * (_radius * 2);
		celestialBody = CelestialBody as CelestialBodyNoise;
		if(celestialBody == null)
		{
			GD.PrintErr("celestialBody is null");
		}
		AddChild(_rootTest);
		CallDeferred(nameof(SpawnChunk), _center, size, _depth);
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
	
	
	private ShaderMaterial GeneratePlanetShader(float minHeight, float maxHeight) {
		// Load the shader correctly
		Shader shader = ResourceLoader.Load<Shader>("res://src/bodies/planet/planet_shader.gdshader");
		ShaderMaterial shaderMaterial = new ShaderMaterial();
		shaderMaterial.Shader = shader;
		shaderMaterial.SetShaderParameter("min_height", minHeight);
		shaderMaterial.SetShaderParameter("max_height", maxHeight);


		// Access exported property (gradient)
		Gradient gradient = _themeGenerator.Gradient;
		GradientTexture1D gradientTexture = new GradientTexture1D();
		gradientTexture.Gradient = gradient;
		gradientTexture.Width = 256;

		shaderMaterial.SetShaderParameter("height_color", gradientTexture);
		shaderMaterial.SetShaderParameter("cliff_color", gradient.GetColor(3));

		return shaderMaterial;

	}
	
	private float[,,] GenerateDataPoints(Vector3 offset , int depth)
	{
		
		var radius = _radius;
		int size = _resolution + 1;
		var dataPoints = new float[size, size, size];
		float voxelSize = GetVoxelSize(depth);
		
		MultiMesh multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multiMesh.Mesh = new BoxMesh
		{
			Size = 0.1f * Vector3.One
		};
		multiMesh.SetInstanceCount(size * size * size);

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
					
					// Set the instance transform
					var instanceIndex = x * size * size + y * size + z;
					var instanceTransform = new Transform3D
					{
						Origin = worldPos,
						Basis = Basis.Identity
					};
					multiMesh.SetInstanceTransform(instanceIndex, instanceTransform);
				}
			}
		}
		// Add the MultiMesh to the scene
		var multiMeshInstance = new MultiMeshInstance3D();
		multiMeshInstance.Multimesh = multiMesh;
		//_rootTest.AddChild(multiMeshInstance);
		return dataPoints;
	}
	
	private static float Sphere(Vector3 worldPos, Vector3 origin, float radius) {
		return (worldPos - origin).Length() - radius;
	}
	
	
}
