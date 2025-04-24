using Godot;
using System;


/// <summary>
/// Spawns a Marching Cube mesh in the scene without chunking
/// </summary>
[Tool]
public partial class McSpawner : Node3D
{
    private bool _reload;
    [Export]
    public bool reload
    {
        get => _reload;
        set
        {
            _reload = !value;
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
	
	[Export] public SurfaceFeature[] SurfaceFeatures { get; set; } = [];
	[Export] public int SurfaceAmount { get; set; } = 10;

	private double _warmth;
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

    private ShaderMaterial _planetShader;
	public ShaderMaterial PlanetShader
	{
		get => _planetShader;
		set
		{
			_planetShader = value;
		}
	}

	private PlanetThemeGenerator _themeGenerator = new PlanetThemeGenerator();

	private int _maxHeight = 16;
	private int _size = 32;
	private MarchingCube _marchingCube;
	private MeshInstance3D _meshInstance3D;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_marchingCube = new MarchingCube();
		CallDeferred(nameof(SpawnMesh));
	}
	
	private void OnResourceSet()
	{
		SpawnMesh();
	}

	public void RegenerateMesh()
	{
		SpawnMesh();
	}

	private void SpawnMesh()
	{
		if(_meshInstance3D != null) RemoveChild(_meshInstance3D);
		_marchingCube ??= new MarchingCube();

		celestialBody = CelestialBody as CelestialBodyNoise;
		if(celestialBody == null)
		{
			GD.PrintErr("celestialBody is null");
		}
		float[,,] dataPoints = celestialBody.GetNoise();
		_meshInstance3D = _marchingCube.GenerateMesh(dataPoints);
		//_meshInstance3D.MaterialOverride = GeneratePlanetShader();

		this.AddChild(_meshInstance3D);

		var grass = new NewGrass();
		var meshSurface = _meshInstance3D.Mesh.SurfaceGetArrays(0);
		AddChild(grass.PopulateMesh(meshSurface, 500000));
		
		SpawnTrees();
		
	}

	private void SpawnTrees()
	{
		
		if (celestialBody is not Node3D celestialBodyNode3D)
		{
			GD.PrintErr("celestialBodyNode3D is null");
			return;
		}
		var scale = celestialBodyNode3D.GetScale();
		var genTree = new GenerateFeatures(SurfaceAmount, SurfaceFeatures);
		var aabb = _meshInstance3D.GetAabb();
		var offset = GetGlobalPosition();
		var trees = genTree.SpawnTrees(GenerateFeatures.SamplingMethod.Poisson, GetWorld3D().DirectSpaceState, aabb, offset, scale);
		foreach (var tree in trees)
		{
			AddChild(tree);
		}
	}

	// Old test method for generating datapoints
	private float[,,] GenerateDataPoints()
	{
		var dataPoints = new float[_size, _maxHeight, _size];
		
		for (var x = 0; x < _size; x++)
		{
			for (var y = 0; y < _maxHeight; y++)
			{
				for (var z = 0; z < _size; z++)
				{
					float value = 0;
					
					if(new Vector3(10, 10, 10).DistanceTo(new Vector3(x, y, z)) < 10)
					{
						value = 1;
					}
					else
					{
						value = -1;
					}
					dataPoints[x, y, z] = value;
				}
			}
		}
		return dataPoints;
	}

	private ShaderMaterial GeneratePlanetShader() {
		// Load the shader correctly
		Shader shader = ResourceLoader.Load<Shader>("res://src/bodies/planet/planet_shader.gdshader");
		ShaderMaterial shaderMaterial = new ShaderMaterial();
		shaderMaterial.Shader = shader;
		shaderMaterial.SetShaderParameter("min_height", _marchingCube.MinHeight);
		shaderMaterial.SetShaderParameter("max_height", _marchingCube.MaxHeight);


		// Access exported property (gradient)
		Gradient gradient = _themeGenerator.Gradient;
		GradientTexture1D gradientTexture = new GradientTexture1D();
		gradientTexture.Gradient = gradient;
		gradientTexture.Width = 256;

		shaderMaterial.SetShaderParameter("height_color", gradientTexture);
		shaderMaterial.SetShaderParameter("cliff_color", gradient.GetColor(3));

		return shaderMaterial;

	}
	
}
