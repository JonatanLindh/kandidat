using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class Octree : Node3D
{
	[Export] public Node PlayerPosition { get; set; }
	[Export] public int MaxDepth { get; set; } = 2;
	[Export] public Node OctreePlanetSpawner { get; set; }
	[Export] public bool ShowOctree { get; set; }
	
	public bool PlanetSpawnerInitialized = false;

	private float _size = 64;
	private Vector3 _center = Vector3.Zero;
	private bool _subDivided  = false;
	private int _depth = 0;
	private float _collisionSize;
	private readonly Guid _octId = Guid.NewGuid();
	private Vector3 _playerPosition;

	private Octree[] _children = new Octree[8];
	private Octree[] _neighbours;
	private Queue<Octree> _subdivisionQueue = new Queue<Octree>();
	private MeshInstance3D _planetMesh;
	private Node3D _debugNode;

	
	private Node3D _rootTest;
	private readonly Vector3[] _octant =
	[
		new (1, 1, 1),
		new (-1, 1, 1),
		new (-1, 1, -1),
		new (1, 1, -1),
			
		new (-1, -1, -1),
		new (1, -1, 1),
		new (-1, -1, 1),
		new (1, -1, -1)
	];
	
	public override void _Ready()
	{
	}

	private void SpawnPlanetChunk()
	{
		switch (OctreePlanetSpawner)
		{
			// Cast to OctreePlanetSpawner
			case OctreePlanetSpawner spawner:
				_planetMesh = spawner.SpawnChunk(_center, _size, _depth, _octId);
				_planetMesh.SortingOffset = _depth * -1;
				break;
			case null:
				GD.PrintErr("OctreePlanetSpawner is null");
				return;
		}
	}

	public void OnPlanetSpawnerReady(float size)
	{
		// Hook into editor camera
		if (Engine.IsEditorHint())
		{
			//Node3D editorCamera = EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D();
			//PlayerPosition = editorCamera;
		}
		else
		{
			PlayerPosition = GetNode("/root/PlayerVariables");
		}
		
		_playerPosition = !Engine.IsEditorHint() ? PlayerPosition.Get("player_position").AsVector3() :
			EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D().Position;

		
		_size = size;
		_debugNode = DrawBoundingBox(_center, _size, new Color(1, 0, 0));
		AddChild(_debugNode);
		if (ShowOctree)
		{
			_debugNode.Show();
		}
		else
		{
			_debugNode.Hide();
		}
		_collisionSize = _size + _size * 1f / Mathf.Pow(2, _depth);
		//AddChild(DrawBoundingBox(_center, _collisionSize, new Color(0, 0, 1)));
		

		

		SpawnPlanetChunk();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationPredelete)
		{
			CleanupResources();
		}
	}
	
	private void CleanupResources()
	{
		if (IsInstanceValid(_planetMesh))
		{
			_planetMesh.QueueFree();
		}
		_planetMesh = null;
    
		// Clean up children
		for (int i = 0; i < _children.Length; i++)
		{
			_children[i] = null;
		}
    
		_subdivisionQueue.Clear();
	}


	// TODO: Change the subdividing to occur based on the distance to the player/camera instead

	public override void _PhysicsProcess(double delta)
	{
		if (PlayerPosition == null) return;
		
		// If subdivided then wait until all children are processed
		// then hide the mesh
		/*
		if (_subDivided && !_allChildrenProcessed)
		{
			foreach (var child in _children)
			{
				if (child != null && IsInstanceValid(child))
				{
					if (!child._subDivided) continue;
					if (child._planetMesh == null) continue;
					if (child._planetMesh.Mesh == null) continue;
					if (MarchingCubeDispatch.Instance.IsTaskBeingProcessed(child._octId)) return;
				}
				_planetMesh.Hide();
				_allChildrenProcessed = true;
			}
			return;
		}
		*/
		ProcessSubdivisionQueue();
		if (!UpdatePlayerPosition())
			return;
		CheckAndHandleSubdivision();
	}	
	
	private void ProcessSubdivisionQueue()
	{
		if (_subdivisionQueue.Count > 0)
		{
			var cell = _subdivisionQueue.Dequeue();
			if (IsInstanceValid(cell))
			{
				CallDeferred(Node.MethodName.AddChild, cell);
				cell.CallDeferred(nameof(OnPlanetSpawnerReady), _size / 2f);
			}
		}
	}
	private bool UpdatePlayerPosition()
	{
		var newPlayerPosition = !Engine.IsEditorHint() 
			? PlayerPosition.Get("player_position").AsVector3()
			: PlayerPosition.Get("position").AsVector3();
			//: EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D().Position;

		// Only update if significant movement occurred
		if (_playerPosition.DistanceSquaredTo(newPlayerPosition) < 10) return false;
		_playerPosition = newPlayerPosition;
		return true;

	}

	private void CheckAndHandleSubdivision()
	{
		// Don't subdivide until the planet mesh has been processed
		// if the mesh is still null after it has been processed, then
		// we do not need to subdivide
		
		// Early return conditions
		if (MarchingCubeDispatch.Instance.IsTaskBeingProcessed(_octId)) return;
    
		if (!CheckCollision(_playerPosition, _collisionSize))
		{
			MergeSubdivision();
			return;
		}

		if (!_subDivided && _depth < MaxDepth && IsValidForSubdivision())
		{
			SubDivide();
		}
	}
	
	private bool IsValidForSubdivision()
	{
		return _planetMesh != null && _planetMesh.Mesh != null;
	}

	private void MergeSubdivision() 
	{
		// Implementation for merging when out of collision range
		
		// Remove all Octree children
		if (!_subDivided) return;
		GD.Print("Left the node in octree");
		foreach (var child in _children)
		{
			if(IsInstanceValid(child))
				child.QueueFree();
		}
		_subDivided = false;

		if (IsInstanceValid(_planetMesh))
		{
			_planetMesh.CallDeferred(Node3D.MethodName.Show);
			// Re-enable collision
			foreach (var child in _planetMesh.GetChildren())
			{
				if (child is StaticBody3D staticBody)
				{
					staticBody.ProcessMode = Node.ProcessModeEnum.Inherit;
					// Or reset collision layers: staticBody.CollisionLayer = yourOriginalValue;
				}
			}
		}
		
	}

	private void SubDivide()
	{
		// Implementation for subdivision
		
		var newSize = _size / 4;
		_planetMesh.CallDeferred(Node3D.MethodName.Hide);
		// Find and disable the collision
		foreach (var child in _planetMesh.GetChildren())
		{
			if (child is StaticBody3D staticBody)
			{
				staticBody.ProcessMode = Node.ProcessModeEnum.Disabled;
				// Or use: staticBody.CollisionLayer = 0;
			}
		}
		// Subdivide the octree into 8 children
		for (int i = 0; i < 8; i++)
		{
			_children[i] = new Octree();
			CreateNewChildrenAsync(_children[i], _center + newSize * _octant[i]);
			/*
			SubDivide(_children[i], _center + newSize * _octant[i]);
			AddChild(_children[i]);
			_children[i].OnPlanetSpawnerReady(_size / 2f);
			*/
			/*
			CallDeferred(nameof(SubDivide), _children[i], _center + newSize * _octant[i]);
			CallDeferred(Node.MethodName.AddChild, _children[i]);
			_children[i].CallDeferred(nameof(OnPlanetSpawnerReady), _size / 2f);
			*/
		}
/*
		_children[0] = new Octree();
		SubDivide(_children[0], _center + newSize * _octant[0]);
		AddChild(_children[0]);
		_children[0].OnPlanetSpawnerReady(_size / 2f);
		*/
/*
		_planetMesh.Hide();
		_children[0] = new Octree();
		SubDivideAsync(_children[0], _center + newSize * _octant[0]);
		_children[1] = new Octree();
		SubDivideAsync(_children[1], _center + newSize * _octant[1]);
		*/
		
		_subDivided = true;
	}
	

	private void CreateNewChildren(Octree newCell, Vector3 newCenter)
	{
		//_planetMesh.CallDeferred(Node3D.MethodName.Hide);
		_planetMesh.Hide();
		newCell._center = newCenter;
		//newCell.Size = Size / 2;
		newCell._depth = _depth + 1;
		newCell.MaxDepth = MaxDepth;
		newCell._rootTest = _rootTest;
		newCell.PlayerPosition = PlayerPosition;
		newCell.OctreePlanetSpawner = OctreePlanetSpawner;
		newCell.ShowOctree = ShowOctree;
		//newCell.OnPlanetSpawnerReady(_size / 2f);
	}
	
	private void CreateNewChildrenAsync(Octree newCell, Vector3 newCenter)
	{
		newCell._center = newCenter;
		newCell._depth = _depth + 1;
		newCell.MaxDepth = MaxDepth;
		newCell._rootTest = _rootTest;
		newCell.PlayerPosition = PlayerPosition;
		newCell.OctreePlanetSpawner = OctreePlanetSpawner;
		newCell.ShowOctree = ShowOctree;

		_subdivisionQueue.Enqueue(newCell);

	}
	
	private void SetNeighbours()
	{
		// Set the neighbors for the first child
		
		var neighbours = new List<Octree>();
		var firstChild = _children[0];
		neighbours.Add(_children[1]);
		neighbours.Add(_children[2]);
		
		
		firstChild._neighbours = neighbours.ToArray();

	}

	private bool CheckCollision (Vector3 position, float size)
	{
		var newCenter = _center + GlobalPosition;
		
		return position.X > newCenter.X - size / 2 && position.X < newCenter.X + size / 2 &&
		       position.Y > newCenter.Y - size / 2 && position.Y < newCenter.Y + size / 2 &&
		       position.Z > newCenter.Z - size / 2 && position.Z < newCenter.Z + size / 2;
	}
	
	private MeshInstance3D DrawBoundingBox(Vector3 center, float size, Color clr)
	{
		var edges = new[]
		{
			new[] { 0, 1 }, new[] { 1, 2 }, new[] { 2, 3 }, new[] { 3, 0 },
			new[] { 4, 5 }, new[] { 5, 6 }, new[] { 6, 7 }, new[] { 7, 4 },
			new[] { 0, 4 }, new[] { 1, 5 }, new[] { 2, 6 }, new[] { 3, 7 }
		};
		var corners = new[]
		{
			new Vector3(center.X - size / 2, center.Y - size / 2, center.Z - size / 2),
			new Vector3(center.X + size / 2, center.Y - size / 2, center.Z - size / 2),
			new Vector3(center.X + size / 2, center.Y + size / 2, center.Z - size / 2),
			new Vector3(center.X - size / 2, center.Y + size / 2, center.Z - size / 2),
			new Vector3(center.X - size / 2, center.Y - size / 2, center.Z + size / 2),
			new Vector3(center.X + size / 2, center.Y - size / 2, center.Z + size / 2),
			new Vector3(center.X + size / 2, center.Y + size / 2, center.Z + size / 2),
			new Vector3(center.X - size / 2, center.Y + size / 2, center.Z + size / 2)
		};
		
		
		ImmediateMesh boundingBox = new ImmediateMesh();
		boundingBox.SurfaceBegin(Mesh.PrimitiveType.Lines);
		
		foreach (var edge in edges)
		{
			boundingBox.SurfaceSetColor(clr);
			boundingBox.SurfaceAddVertex(corners[edge[0]]);
			boundingBox.SurfaceSetColor(clr);
			boundingBox.SurfaceAddVertex(corners[edge[1]]);
		}
		boundingBox.SurfaceEnd();
		
		if (boundingBox.GetSurfaceCount() == 0)
		{
			GD.PrintErr("ImmediateMesh has no surfaces. Ensure geometry is added correctly.");
			return null;
		}
		
		MeshInstance3D boundingBoxInstance = new MeshInstance3D();
		boundingBoxInstance.Mesh = boundingBox;
		boundingBoxInstance.MaterialOverride = new StandardMaterial3D()
		{
			ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
			VertexColorUseAsAlbedo = true,
			DisableFog = true
		};
		return (boundingBoxInstance);
	}
}
