using Godot;
using System;

[Tool]
public partial class GrassTest : Node
{
	[Export]
	public MeshInstance3D TargetMeshInstance
	{
		get => _targetMeshInstance;
		set
		{
			_targetMeshInstance = value;
			SpawnGrass();
		}
	}


	[Export]
	public int Density
	{
		get => _density;
		set
		{
			_density = value;
			SpawnGrass();
		}
	}

	private Node _grassParent;
	private Mesh _mesh;
	private int _density = 1000;
	private MeshInstance3D _targetMeshInstance;

	public override void _Ready()
	{
		SpawnGrass();
	}

	private void SpawnGrass()
	{
		// Early return if required components are null
		if (TargetMeshInstance == null)
		{
			return;
		}

		if (TargetMeshInstance.Mesh == null)
		{
			return;
		}
		_grassParent?.QueueFree();
		var grass = new NewGrass();
		_grassParent = new Node3D();
		
		// Extract vertices out of TargetMeshInstance
		var meshSurface = _targetMeshInstance.Mesh.SurfaceGetArrays(0);

		AddChild(_grassParent);
		_grassParent.AddChild(grass.PopulateMesh(meshSurface, _density));
	}
	



}
