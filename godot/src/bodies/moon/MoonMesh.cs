using Godot;
using System;
using System.Threading.Tasks;

/// <summary>
/// Generates a mesh for a moon surface with craters using the Marching Cubes algorithm.
/// </summary>
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
	[Export]
	public int Resolution
	{
		get => _resolution;
		set
		{
			_resolution = value;
			OnResourceSet();
		}
	}
	
	[ExportCategory("Material Settings")]
	[Export] public Material MeshMaterial { get; set; }


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
	public float MinCraterRadius
	{
		get => _minCraterRadius;
		set
		{
			_minCraterRadius = value;
			OnResourceSet();
		}
	}
	[Export]
	public float MaxCraterRadius
	{
		get => _maxCraterRadius;
		set
		{
			_maxCraterRadius = value;
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

	private MarchingCube _marchingCube;
	private int _radius = 50;
	private MeshInstance3D _mesh;
	private int _resolution = 40;
	
	
	private int _amountOfCraters = 10;
	private float _rimWidth = 2f;
	private float _rimSteepness = 0.5f;	
	private float _floorHeight = -1f;
	private float _minCraterRadius = 1f;
	private float _maxCraterRadius = 3f;
	private float _smoothness = 0.1f;
	
	private Crater[] _craters;
	
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
		var diameter = radius * 2 + 1;
		var dataPoints = new float[diameter, diameter, diameter];
		Vector3 center = new Vector3(radius, radius, radius);

		for (var x = 0; x < dataPoints.GetLength(0); x++)
		{
			for (var y = 0; y < dataPoints.GetLength(1); y++)
			{
				for (var z = 0; z < dataPoints.GetLength(2); z++)
				{
					Vector3 point = new Vector3(x, y, z);
					float distance = center.DistanceTo(point);
                 
					// Adjust threshold to match radius more precisely
					// Using radius - 0.5 creates a more visually accurate sphere
					float value = (radius - 0.5f) - distance;
                 
					// Clamp to ensure values stay in [-1, 1] range
					value = Mathf.Clamp(value, -1.0f, 1.0f);
                 
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
		var dataPoints = GenerateDataPoints(_resolution);
		_marchingCube ??= new MarchingCube();
		var mesh = _marchingCube.GenerateMesh(dataPoints);
		_mesh = new MeshInstance3D();
		_mesh.Mesh = mesh;
		
		_craters = GenerateCraterCenters(_amountOfCraters);
		
		if(MeshMaterial != null)
			_mesh.MaterialOverride = MeshMaterial;
		GenerateCraters();
		_mesh.Scale = Vector3.One * (1 / (float)_resolution);
		_mesh.Scale *= _radius;
		AddChild(_mesh);
	}
	

	private void GenerateCraters()
	{
		if (_craters == null || _mesh?.Mesh == null) return;
		// Check if mesh is valid and has surfaces
		if (_mesh.Mesh.GetSurfaceCount() == 0)
		{
			//GD.PrintErr("Mesh has no surfaces to modify for craters");
			return;
		}
    
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

				positions[i] += positions[i].Normalized() * craterHeight;
				
				// Calculate the Normal
				normal[i] = positions[i].Normalized();

			}
		}
		
		meshData[(int)Mesh.ArrayType.Vertex] = positions;
		var newMesh = new ArrayMesh();
		newMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles,meshData);
		_mesh.Mesh = newMesh;
	}


	private Crater[] GenerateCraterCenters(int amount)
	{
		var craters = new Crater[amount];
		for(int i = 0; i < amount; i++)
		{
			// Randomize the Crater Radius
			var craterRadius = Mathf.Lerp(_minCraterRadius, _maxCraterRadius, (float)GD.RandRange(0f, 1f));
			var centre = RandomVector3(_resolution - 5, _resolution,_resolution * Vector3.One);
			craters[i] = new Crater(craterRadius, centre);
		}
		return craters;
	}
	
	private static float SmoothMax(float a, float b, float k)
	{
		return SmoothMin(a, b, -k);
	}

	private static float SmoothMin(float a, float b, float k)
	{
		var h = Mathf.Clamp((b - a + k) / (2.0f * k), 0.0f, 1.0f);
		return a * h + b * (1f - h) - k * h * (1.0f - h);
	}

	private static Vector3 RandomVector3(float minLength, float maxLength, Vector3 origin = default, int seed = -1)
	{
		var random = seed == -1 ? new Random() : new Random(seed);
        
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


	

