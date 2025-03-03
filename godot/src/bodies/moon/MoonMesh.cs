using Godot;
using System;
using System.Threading.Tasks;

[Tool]
public partial class MoonMesh : Node
{
	[Export]
	public int Radius
	{
		get => _radius;
		set
		{
			_radius = value;
			OnResourceSet();
		}
	}

	[Export(PropertyHint.Range, "0,20,")]
	public float TestValue
	{
		get => _testValue;
		set
		{
			_testValue = value;
			OnResourceSet();
		}
	}

	[Export] public Texture2D NormalMap { get; set; }
	[Export] public Shader ShaderMaterial { get; set; }

	[Export]
	public float NormalScale
	{
		get => _normalScale;
		set
		{
			_normalScale = value;
			OnResourceSet();
		}

	}
	
	[Export]
	public float BlendSharpness
	{
		get => _blendSharpness;
		set
		{
			_blendSharpness = value;
			OnResourceSet();
		}
	}

	[Export]
	public float MappingScale
	{
		get => _mappingScale;
		set
		{
			_mappingScale = value;
			OnResourceSet();
		}
	}

	private float _testValue = 0.5f;
	private MarchingCube _marchingCube;
	private int _radius = 50;
	private MeshInstance3D _mesh;
	private float _normalScale = 1f;
	private float _blendSharpness = 1f;
	private float _mappingScale = 0.1f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_marchingCube = new MarchingCube();
		SpawnMesh();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private float[,,] GenerateDataPoints(int radius)
	{
		var dataPoints = new float[2 * radius + 1, 2 * radius + 1, 2 * radius + 1];

		for (var x = 0; x < dataPoints.GetLength(0); x++)
		{
			for (var y = 0; y < dataPoints.GetLength(1); y++)
			{
				for (var z = 0; z < dataPoints.GetLength(2); z++)
				{
					float value = -1;
					if (new Vector3(radius, radius, radius).DistanceTo(new Vector3(x, y, z)) < radius)
					{
						value = 1;
					}
					else
					{
						value = -1;
					}


					// if (y == 0)
					// {
					// 	value = 1;
					// }

					dataPoints[x, y, z] = value;
				}
			}
		}

		return dataPoints;
	}

	private void OnResourceSet()
	{
		SpawnMesh();
	}

	private void SpawnMesh()
	{
		if (_mesh != null)
		{
			RemoveChild(_mesh);
			_mesh.QueueFree();
			_mesh = null;
		}

		_marchingCube ??= new MarchingCube();
		var dataPoints = GenerateDataPoints(_radius);
		_mesh = _marchingCube.GenerateMesh(dataPoints);

		var shadermaterial = new ShaderMaterial();

		// Set material properties
		//material.SetCullMode(BaseMaterial3D.CullModeEnum.Disabled);
		if (NormalMap != null && ShaderMaterial != null)
		{
			shadermaterial.SetShader(ShaderMaterial);
			shadermaterial.SetShaderParameter("blend_sharpness", _blendSharpness);
			shadermaterial.SetShaderParameter("mapping_scale", _mappingScale);
			shadermaterial.SetShaderParameter("normal", NormalMap);

		}

		_mesh.MaterialOverlay = shadermaterial;
		AddChild(_mesh);
	}
}
	

