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
	public float UVScale
	{
		get => _uvScale; 
		set
		{
			_uvScale = value;
			OnResourceSet();
		}
		
	}
	
	private float _testValue = 0.5f;
	private MarchingCube _marchingCube;
	private int _radius = 50;
	private MeshInstance3D _mesh;
	private float _normalScale = 1f;
	private float _uvScale = 0.1f;


	
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
					// if(new Vector3(radius, radius, radius).DistanceTo(new Vector3(x, y, z)) < radius)
					// {
					// 	value = 1;
					// }
					// else
					// {
					// 	value = -1;
					// }
					//

					if (y == 0)
					{
						value = 1;
					}

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
		_mesh = _marchingCube.GenerateMesh(dataPoints, _uvScale);
		
		var material = new StandardMaterial3D();
		
		// Set material properties
		//material.SetCullMode(BaseMaterial3D.CullModeEnum.Disabled);
		if (NormalMap != null)
		{
			material.NormalEnabled = true;
			material.NormalTexture = NormalMap;
			material.NormalScale = _normalScale;
		
		}
		_mesh.MaterialOverride = material;

		
		AddChild(_mesh);
	}




	

	
}
