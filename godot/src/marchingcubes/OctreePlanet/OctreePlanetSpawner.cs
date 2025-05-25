using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages the procedural generation of planetary terrain using octree-based level of detail (LOD).
/// </summary>
/// <remarks>
/// This class is responsible for:
/// <list type="bullet">
///   <item>Dynamically spawning and managing planet chunks based on octree subdivision</item>
///   <item>Coordinating with the marching cubes algorithm for mesh generation</item>
///   <item>Generating appropriate shader materials for planetary surfaces</item>
///   <item>Managing planet theme and appearance via the theme generator</item>
/// </list>
/// The spawner uses a resolution and depth parameter to control the detail level of the generated planet,
/// and works with a CelestialBodyNoise component to generate the actual planetary terrain data.
/// </remarks>
[Tool]
public partial class OctreePlanetSpawner : Node3D
{
	
	[Signal]
	public delegate void SpawnedEventHandler(float radius);
    
	private const int DefaultResolution = 32;
	private const int DefaultMaxDepth = 4;
	
	[Export] public Node CelestialBody
	{
		get => _celestialBodyNode;
		set
		{
			_celestialBodyNode = value;
		}
	}
	
	public PlanetThemeGenerator ThemeGenerator
	{
		get => _themeGenerator;
		set
		{
			_themeGenerator = value;
		}
	}
	
	public ShaderMaterial PlanetShader
	{
		get => _planetShader;
		set
		{
			_planetShader = value;
		}
	}
	
	public double Warmth
	{
		get => _warmth;
		set
		{
			_warmth = value;

			if (_themeGenerator != null)
			{
				_themeGenerator.Warmth = value; // Tell it to pick a new theme
			}
		}
	}

	[Export] public int Resolution { get; set; } = DefaultResolution;
	[Export] public float GrassDensity { get; set; } = 50f;
	
	
	private Vector3 _center = Vector3.Zero;
	private int _depth = 0;
	private CelestialBodyNoise _celestialBody;
	private Node _celestialBodyNode;
	
	// Planet generation parameters
	private float _baseVoxelSize;
	private float _radius = 32;
	private int _maxDepth = DefaultMaxDepth;
	private int _resolution = 32;
	private float _minHeight = 0;
	private float _maxHeight = 0;
	private bool _heightsInitialized = false;
	
	// Theme generation
	private PlanetThemeGenerator _themeGenerator;
	private ShaderMaterial _planetShader;
	private double _warmth;

	private Shader _shader;
	private Gradient _gradient;
	
	public override void _Ready()
	{
		Init();
		EmitSignal(SignalName.Spawned, _radius * 2f);
		_shader = ResourceLoader.Load<Shader>("res://src/bodies/planet/planet_shader.gdshader");
		//_gradient = _themeGenerator.Gradient;

	}
	
	/// <summary>
	/// Spawns a new planetary chunk at the specified location.
	/// </summary>
	/// <param name="chunk">The parent node to attach the chunk to</param>
	/// <param name="center">The center position of the chunk</param>
	/// <param name="size">The size of the chunk</param>
	/// <param name="depth">The depth level in the octree</param>
	/// <param name="id">Unique identifier for the chunk request (optional)</param>
	/// <returns>A MeshInstance3D representing the chunk, or null if generation fails</returns>
	public MeshInstance3D SpawnChunk(Node chunk, Vector3 center, float size, int depth, Guid id = new Guid())
	{
		if (_celestialBody == null)
		{
			GD.Print("CELESTIAL BODY IS NULL");
			return null;
		}
		
		var scaleFactor = GetVoxelSize(depth);
		
		var requestInstance = new MeshInstance3D();
		Transform3D transform = new Transform3D
		{
			Origin = center,
			Basis = Basis.Identity
		};
		requestInstance.Transform = transform;	
		
		
		chunk.AddChild(requestInstance);
		
		// Send the request to the MarchingCubeDispatch
		MarchingCubeRequest cubeRequest = new MarchingCubeRequest
		{
			PlanetDataPoints = _celestialBody,
			Scale = scaleFactor,
			Offset = Vector3.One * (size / 2),
			Center = center,
			Root = this,	
			CustomMeshInstance = requestInstance,
			GeneratePlanetShader = GeneratePlanetShader,
			Id = id,
			GenerateGrass = depth == _maxDepth,
			GrassRequest = new GrassRequest
			{
				GenerateGrass = depth == _maxDepth,
				GrassDensity = GrassDensity
			}
		}; 
		MarchingCubeDispatch.Instance.AddToQueue(cubeRequest, id);
		
		return requestInstance;
	}
	
	public void GenFeatures(SurfaceFeature[] features, Node chunk, List<List<(Vector3, int)>> rayPositions, Vector3 chunkPosition, Vector3 center, float size)
	{
		GenerateFeatures generateFeatures = new GenerateFeatures(20, null);
		Aabb aabb = new Aabb(center - Vector3.One * (size / 2) + chunkPosition, Vector3.One * size);
		var raycastHits =
			GenerateFeatures.PerformRayCastsWithBounds(rayPositions, GetWorld3D().DirectSpaceState, aabb, GlobalPosition);
		
		var multiMeshes = generateFeatures.GenFeatures
			(raycastHits, features, offset: - GlobalPosition - Vector3.One * center, seed: _celestialBody.Seed);
		foreach (var multiMesh in multiMeshes)
		{
			chunk.AddChild(multiMesh);
		}
		
	}
	
	private ShaderMaterial GeneratePlanetShader(float minHeight, float maxHeight, Vector3 chunkCenter) {
		
		if(_themeGenerator == null)
		{
			return null;
		}

		// Reset min/max height values when generating a new planet
		if (!_heightsInitialized) {
			_minHeight = minHeight; 
			_maxHeight = maxHeight;
			_heightsInitialized = true;
		}
		
		_themeGenerator.LoadAndGenerateThemes();
		
		// Load the shader correctly
		Shader shader = ResourceLoader.Load<Shader>("res://src/bodies/planet/planet_shader.gdshader");
		ShaderMaterial shaderMaterial = new ShaderMaterial();
		shaderMaterial.Shader = _shader;
		shaderMaterial.SetShaderParameter("min_height", _minHeight);
		shaderMaterial.SetShaderParameter("max_height", _maxHeight);
		shaderMaterial.SetShaderParameter("chunk_center", chunkCenter);


		// Access exported property (gradient)
		Gradient gradient = _themeGenerator.Gradient;
		_gradient ??= _themeGenerator.Gradient;
		GradientTexture1D gradientTexture = new GradientTexture1D();
		gradientTexture.Gradient = _gradient;
		gradientTexture.Width = 256;

		shaderMaterial.SetShaderParameter("height_color", gradientTexture);
		if(gradient.GetPointCount() >= 3)
		{
			shaderMaterial.SetShaderParameter("cliff_color", gradient.GetColor(3));
		}
		else
		{
			GD.Print("NO cliff color for you!");
		}
		
		return shaderMaterial;

	}
	
	private void Init()
	{
		if (!IsInsideTree()) return;
		
		_heightsInitialized = false; // Reset this flag
		
		_celestialBody = CelestialBody as CelestialBodyNoise;
		if(_celestialBody == null)
		{
			GD.PrintErr("celestialBody is null");
		}

		if (_celestialBody != null)
		{
			_radius = _celestialBody.GetRadius();
			_celestialBody.Resolution = Resolution;
		}
		_baseVoxelSize = (_radius * 2) / (Resolution * Mathf.Pow(2, _maxDepth));

	}
	
	private float GetVoxelSize(int depth) {
		int maxDepth = _maxDepth;
		float baseVoxelSize = _baseVoxelSize;
		return Mathf.Pow(2, (maxDepth - depth)) * baseVoxelSize;
	}
	
}
