using Godot;
using System;


/// <summary>
/// Spawns a Marching Cube mesh in the scene without chunking
/// </summary>
[Tool]
public partial class McSpawner : Node
{
	[Export] private Node CelestialBody;
	private CelestialBodyNoise celestialBody;

    private int _maxHeight = 16;
	private int _size = 32;
	private MarchingCube _marchingCube;
	private MeshInstance3D _meshInstance3D;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Ready");
		_marchingCube = new MarchingCube();
		SpawnMesh();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnResourceSet()
	{
		SpawnMesh();
	}

	private void SpawnMesh()
	{
		if(_meshInstance3D != null) RemoveChild(_meshInstance3D);
		_marchingCube ??= new MarchingCube();

        celestialBody = CelestialBody as CelestialBodyNoise;
		GD.Print(celestialBody);
		if(celestialBody != null)
		{
            GD.Print("is CelestialBodyNoise");

            float[,,] dataPoints = celestialBody.GetNoise(); ;
			_meshInstance3D = _marchingCube.GenerateMesh(dataPoints);

			// Disable backface culling
			_meshInstance3D.MaterialOverride = new StandardMaterial3D();
			((StandardMaterial3D)_meshInstance3D.MaterialOverride).SetCullMode(BaseMaterial3D.CullModeEnum.Disabled);

			AddChild(_meshInstance3D);
        }
	}

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
	
}
