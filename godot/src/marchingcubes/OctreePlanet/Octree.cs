using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class Octree : Node3D
{
	[Export] public Node PlayerPosition { get; set; }
	[Export] public int maxDepth { get; set; } = 2;
	[Export] public Node OctreePlanetSpawner { get; set; }

	[Export]
	public bool ShowOctree
	{
		get;
		set;
	}
	
	public bool PlanetSpawnerInitialized = false;

	private float _size = 64;
	private Vector3 _center = Vector3.Zero;

	private Node3D _rootTest;

	private bool _subDivided  = false;
	private bool _allChildrenProcessed = false;

	private int _depth = 0;

	private float _collisionSize;
	
	
	private Guid _octId = Guid.NewGuid();
	
	
	//Octree topLeft = null;
	//Octree topRight = null;
	
	private Octree[] _children = new Octree[8];

	private MeshInstance3D _planetMesh;
	
	//public OctreePlanetSpawner OctreePlanetSpawner;

	private Node3D debugNode;
	
	private Vector3 _playerPosition;
	
	private Queue<Octree> _subdivisionQueue = new Queue<Octree>();
	
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
		//playerPosition ??= new Node3D();

		//if (OctreePlanetSpawner != null) 
		//CallDeferred(Node.MethodName.AddChild, OctreePlanetSpawner.SpawnChunk(_center, Size, _depth));
	}

	private void SpawnPlanetChunk()
	{
		switch (OctreePlanetSpawner)
		{
			// Cast to OctreePlanetSpawner
			case OctreePlanetSpawner spawner:
				_planetMesh = spawner.SpawnChunk(_center, _size, _depth, _octId);
				//AddChild(spawner.SpawnChunk(_center, Size, _depth));
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
			Node3D editorCamera = EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D();
			PlayerPosition = editorCamera;
		}
		else
		{
			PlayerPosition = GetNode("/root/PlayerVariables");
		}
		
		_playerPosition = !Engine.IsEditorHint() ? PlayerPosition.Get("player_position").AsVector3() :
			EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D().Position;

		
		_size = size;
		debugNode = DrawBoundingBox(_center, _size, new Color(1, 0, 0));
		AddChild(debugNode);
		if (ShowOctree)
		{
			debugNode.Show();
		}
		else
		{
			debugNode.Hide();
		}
		_collisionSize = _size + _size * 1f / Mathf.Pow(2, _depth);
		//AddChild(DrawBoundingBox(_center, _collisionSize, new Color(0, 0, 1)));

		

		SpawnPlanetChunk();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationPredelete)
		{
			if (IsInstanceValid(_planetMesh))
			{
				_planetMesh.QueueFree();
			}
			_planetMesh = null; // Clear the reference to avoid further access
			_children = null;
			_subdivisionQueue.Clear();
			
		}
	}



	// TODO: Change the subdividing to occur based on the distance to the player/camera instead

	public override void _PhysicsProcess(double delta)
	{
		
		// Process one subdivision per frame
		if (_subdivisionQueue.Count > 0)
		{
			var cell = _subdivisionQueue.Dequeue();
			// Ensure the cell is valid before adding it to the scene tree
			if (IsInstanceValid(cell))
			{
				CallDeferred(Node.MethodName.AddChild, cell);
				cell.CallDeferred(nameof(OnPlanetSpawnerReady), _size / 2f);
				/*
				AddChild(cell);
				cell.OnPlanetSpawnerReady(_size / 2f);
				*/
			}
			else
			{
				GD.PrintErr("Invalid Octree instance in subdivision queue.");
			}
		}
		
		
		
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
		
		
		
		var newPlayerPosition = !Engine.IsEditorHint() ? PlayerPosition.Get("player_position").AsVector3() :
			EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D().Position;
		
		// If the change in player position has not been large enough, do not check for collision
		if (_playerPosition.DistanceSquaredTo(newPlayerPosition) < 10) return;
		_playerPosition = newPlayerPosition;
		
		
		// Don't subdivide until the planet mesh has been processed
		// if the mesh is still null after it has been processed, then
		// we do not need to subdivide

		if (MarchingCubeDispatch.Instance.IsTaskBeingProcessed(_octId)) return;
		
		if (!CheckCollision(_playerPosition, _collisionSize))
		{
			// Remove all children
			if (!_subDivided) return;
			foreach (var child in _children)
			{
				if(IsInstanceValid(child))
					child.QueueFree();
			}
			_subDivided = false;
			
			if (IsInstanceValid(_planetMesh))
				_planetMesh.Show();
			else
				SpawnPlanetChunk();
			
			return;
		}
		
		// Collision has occurred, so we need to subdivide
		
		
		// If the max depth has been reached, do not subdivide anymore
		if (_subDivided || _depth >= maxDepth) return;
		
		
		// The planet mesh is null, so no need to subdivide
		if (_planetMesh is { Mesh: null })
		{
			//GD.Print("Planet mesh is null");
			return;
		}
		

		
		
		var newSize = _size / 4;
		_planetMesh.Hide();
		// Subdivide the octree into 8 children
		for (int i = 0; i < 8; i++)
		{
			_children[i] = new Octree();
			SubDivideAsync(_children[i], _center + newSize * _octant[i]);
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

	private void SubDivide(Octree newCell, Vector3 newCenter)
	{
		//_planetMesh.CallDeferred(Node3D.MethodName.Hide);
		_planetMesh.Hide();
		newCell._center = newCenter;
		//newCell.Size = Size / 2;
		newCell._depth = _depth + 1;
		newCell.maxDepth = maxDepth;
		newCell._rootTest = _rootTest;
		newCell.PlayerPosition = PlayerPosition;
		newCell.OctreePlanetSpawner = OctreePlanetSpawner;
		newCell.ShowOctree = ShowOctree;
		//newCell.OnPlanetSpawnerReady(_size / 2f);
	}
	
	private void SubDivideAsync(Octree newCell, Vector3 newCenter)
	{
		newCell._center = newCenter;
		newCell._depth = _depth + 1;
		newCell.maxDepth = maxDepth;
		newCell._rootTest = _rootTest;
		newCell.PlayerPosition = PlayerPosition;
		newCell.OctreePlanetSpawner = OctreePlanetSpawner;
		newCell.ShowOctree = ShowOctree;

		_subdivisionQueue.Enqueue(newCell);

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
