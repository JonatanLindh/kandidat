using Godot;
using System;


/// <summary>
/// Spawns a Marching Cube mesh in the scene without chunking
/// </summary>
[Tool]
public partial class McSpawner : Node
{
	private bool _reload;
	[Export]
	public bool reload
	{
		get => _reload;
		set
		{
			_reload = !value;
			//OnResourceSet();
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
			// OnResourceSet(); TODO THIS MAKES THE SpawnMesh() FUNCTION RUN TWICE!
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
		if(celestialBody != null)
		{
			float[,,] dataPoints = celestialBody.GetNoise();
			_meshInstance3D = _marchingCube.GenerateMesh(dataPoints);
			_meshInstance3D.MaterialOverride = GeneratePlanetShader();

			// Disable backface culling
			// StandardMaterial3D material = new StandardMaterial3D();
			// Color o = new Color();
			// material.AlbedoColor = Color.Color8((byte)152, (byte)102, (byte)0);
			// material.ShadingMode = BaseMaterial3D.ShadingModeEnum.PerVertex;
			// material.DisableReceiveShadows = true;
			// _meshInstance3D.MaterialOverride = material;
			// ((StandardMaterial3D)_meshInstance3D.MaterialOverride).SetCullMode(BaseMaterial3D.CullModeEnum.Disabled);

			this.AddChild(_meshInstance3D);
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
		var shader = ResourceLoader.Load<Shader>("res://src/bodies/planet/planet_shader.gdshader");
		var shaderMaterial = new ShaderMaterial();
		shaderMaterial.Shader = shader;
		shaderMaterial.SetShaderParameter("min_height", _marchingCube.MinHeight);
		shaderMaterial.SetShaderParameter("max_height", _marchingCube.MaxHeight);

		// _themeGenerator = new PlanetThemeGenerator();

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
