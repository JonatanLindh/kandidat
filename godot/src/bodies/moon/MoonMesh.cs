using Godot;
using System;
using System.Threading.Tasks;

[Tool]
public partial class MoonMesh : Node
{
	private struct Crater
	{
		public float Radius { get; }
		public Vector3 Centre { get; }
	
		public Crater(float radius, Vector3 centre)
		{
			Radius = radius;
			Centre = centre;
		}
	}
	
	[ExportCategory("Mesh Settings")]
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
	
	[ExportCategory("Material Settings")]
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

	[ExportCategory("Crater Settings")]
	[Export]
	public int AmountOfCraters
	{
		get => _amountOfCraters; 
		set
		{
			_amountOfCraters = value;
			OnResourceSet();
		} 
		
	}
	[Export]
	public float RimWidth
	{
		get => _rimWidth;
		set
		{
			_rimWidth = value;
			OnResourceSet();
		}
	}
	[Export]
	public float RimSteepness
	{
		get => _rimSteepness;
		set
		{
			_rimSteepness = value;
			OnResourceSet();
		}
	}
	[Export]
	public float FloorHeight
	{
		get => _floorHeight;
		set
		{
			_floorHeight = value;
			OnResourceSet();
		}
	}
	
	[Export]
	public float CraterRadius
	{
		get => _craterRadius;
		set
		{
			_craterRadius = value;
			OnResourceSet();
		}
	}
	[Export]
	public Vector3 CraterCentre
	{
		get => _craterCentre;
		set
		{
			_craterCentre = value;
			OnResourceSet();
		}
	}
	[Export]
	public float Smoothness
	{
		get => _smoothness;
		set
		{
			_smoothness = value;
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
	
	private int _amountOfCraters = 10;
	private float _rimWidth = 2f;
	private float _rimSteepness = 0.5f;	
	private float _floorHeight = -1f;
	private float _craterRadius = 3f;
	private float _smoothness = 0.1f;
	private Vector3 _craterCentre = new Vector3(20, 20, 10);
	private Crater[] _craters;

	private Node3D _craterList;
	
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
		var dataPoints = GenerateDataPoints(_radius);
		_marchingCube ??= new MarchingCube();
		_mesh = _marchingCube.GenerateMesh(dataPoints);
		
		_craters = GenerateCraterCenters(_amountOfCraters);
		GenerateCraters();
		
		ApplyMaterial();
		AddChild(_mesh);
	}

	private void ApplyMaterial()
	{
		var shaderMaterial = new ShaderMaterial();

		// Set material properties
		//material.SetCullMode(BaseMaterial3D.CullModeEnum.Disabled);
		if (NormalMap != null && ShaderMaterial != null)
		{
			shaderMaterial.SetShader(ShaderMaterial);
			shaderMaterial.SetShaderParameter("blend_sharpness", _blendSharpness);
			shaderMaterial.SetShaderParameter("mapping_scale", _mappingScale);
			shaderMaterial.SetShaderParameter("normal", NormalMap);

		}

		_mesh.MaterialOverlay = shaderMaterial;
	}

	private void GenerateCraters()
	{
		if (_craters == null) return;
		var meshData = _mesh.Mesh.SurfaceGetArrays(0);
		var positions = meshData[(int)Mesh.ArrayType.Vertex].AsVector3Array();
		var normal = meshData[(int)Mesh.ArrayType.Normal].AsVector3Array();
		foreach (var crater in _craters)
		{
			for (int i = 0; i < positions.Length; i++)
			{
				var craterHeight = 0f;
				var x = positions[i].DistanceTo(crater.Centre) / crater.Radius;
			
				// Cavity Calculation
				var cavity = x * x - 1f;
			
				// Rim Calculation
				var rimX = Mathf.Min(x - 1 - _rimWidth, 0);
				var rim = _rimSteepness * rimX * rimX;

				// Crater Shape Calculation
				float craterShape = SmoothMax(cavity, _floorHeight, _smoothness);
				craterShape = SmoothMin(craterShape, rim, _smoothness);
			
				// Modify the Crater Height
				craterHeight += craterShape * crater.Radius;

				positions[i] += normal[i] * craterHeight;
				

			}
		}
		for(int i = 0; i < positions.Length; i++)
		{
			foreach (var crater in _craters)
			{
			}
		}
		meshData[(int)Mesh.ArrayType.Vertex] = positions;
		var newMesh = new ArrayMesh();
		newMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles,meshData);
		_mesh.Mesh = newMesh;
	}


	private Crater[] GenerateCraterCenters(int amount, bool renderCrates = false)
	{
		if (_craterList != null)
		{
			RemoveChild(_craterList);
			_craterList.QueueFree();
			_craterList = null;
		}

		if(renderCrates)
			_craterList = new Node3D();
		
		
		var craters = new Crater[amount];
		for(int i = 0; i < amount; i++)
		{
			var centre = RandomVector3(_radius - 5, _radius,_radius * Vector3.One);
			craters[i] = new Crater(_craterRadius, centre);
			
			
			if (renderCrates)
			{
				var meshInstance = new MeshInstance3D();
				var mesh = new SphereMesh();
				meshInstance.Mesh = mesh;
				meshInstance.Scale = 2 * _craterRadius * Vector3.One;
				meshInstance.Transform = new Transform3D(meshInstance.Transform.Basis, craters[i].Centre);
				_craterList.AddChild(meshInstance);
			}
		}
		
		if(renderCrates)
			AddChild(_craterList);
		
		return craters;
	}
	
	private static float SmoothMax(float a, float b, float k)
	{
		//float h = Mathf.Clamp((b - a + k) / (2.0f * k), 0.0f, 1.0f);
		//return Mathf.Lerp(b, a, h) + k * h * (1.0f - h);
		return SmoothMin(a, b, -k);
	}

	private static float SmoothMin(float a, float b, float k)
	{
		//float h = Mathf.Clamp((a - b + k) / (2.0f * k), 0.0f, 1.0f);
		//return Mathf.Lerp(a, b, h) - k * h * (1.0f - h);
		var h = Mathf.Clamp((b - a + k) / (2.0f * k), 0.0f, 1.0f);
		return a * h + b * (1f - h) - k * h * (1.0f - h);
	}

	private static Vector3 RandomVector3(float minLength, float maxLength, Vector3 origin = default)
	{
		var random = new Random();
        
		// Generate a random direction
		float x = (float)(random.NextDouble() * 2.0 - 1.0);
		float y = (float)(random.NextDouble() * 2.0 - 1.0);
		float z = (float)(random.NextDouble() * 2.0 - 1.0);
		Vector3 direction = new Vector3(x, y, z).Normalized();
        
		// Generate a random length within the specified range
		float length = (float)(random.NextDouble() * (maxLength - minLength) + minLength);
        
		// Scale the direction by the length
		return direction * length + origin;
	}
	
}


	

