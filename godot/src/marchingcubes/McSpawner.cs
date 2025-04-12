using Godot;
using System;
using System.Numerics;
using Vector3 = Godot.Vector3;


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

    private int _maxHeight = 16;
	private int _size = 32;
	private MarchingCube _marchingCube;
	private MeshInstance3D _meshInstance3D;
	private MeshInstance3D _temporaryMeshInstance;
	private bool _useTemp = true;
	
	
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
		_meshInstance3D?.QueueFree();
		if(IsInstanceValid(_temporaryMeshInstance))
			_temporaryMeshInstance.QueueFree();

		celestialBody = CelestialBody as CelestialBodyNoise;
		if(celestialBody != null)
		{
			_meshInstance3D = new MeshInstance3D();
			
			// Disable backface culling
			StandardMaterial3D material = new StandardMaterial3D();
			Color o = new Color();
			material.AlbedoColor = Color.Color8((byte)152, (byte)102, (byte)0);
			material.ShadingMode = BaseMaterial3D.ShadingModeEnum.PerVertex;
			material.DisableReceiveShadows = true;
			_meshInstance3D.MaterialOverride = material;
			((StandardMaterial3D)_meshInstance3D.MaterialOverride).SetCullMode(BaseMaterial3D.CullModeEnum.Disabled);
			
			// Set up a temporary mesh instance that will disappear after the mesh is generated
			if (_useTemp)
			{
				_temporaryMeshInstance = new MeshInstance3D();
				_temporaryMeshInstance.Mesh = new SphereMesh
				{
					Radius = celestialBody.GetRadius(),
					Height = celestialBody.GetRadius() * 2
				};
				_temporaryMeshInstance.MaterialOverride = material;
				AddChild(_temporaryMeshInstance);
			}
			
			MarchingCubeRequest cubeRequest = new MarchingCubeRequest
			{
				PlanetDataPoints = celestialBody,
				Scale = 1,
				Offset = Vector3.Zero,
				Root = this,
				CustomMeshInstance = _meshInstance3D,
				TempNode = _useTemp ? _temporaryMeshInstance : null
			};
			MarchingCubeDispatch.Instance.AddToQueue(cubeRequest);
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
	
}
