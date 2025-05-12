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
	[Export] public float DistanceFactor { get; set; } = 2f;
	
	public bool PlanetSpawnerInitialized = false;

	private float _size = 64;
	private Vector3 _center = Vector3.Zero;
	private bool _subDivided  = false;
	private int _depth = 0;
	private float _collisionSize;
	private readonly Guid _octId = Guid.NewGuid();
	private Vector3 _playerPosition;

	private Octree[] _children = new Octree[8];
	private bool _allChildrenProcessed = false;
	private Octree[] _neighbours;
	private Queue<Octree> _subdivisionQueue = new Queue<Octree>();
	private MeshInstance3D _planetMesh;
	private Node3D _debugNode;

	
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
				_planetMesh = spawner.SpawnChunk(this, _center, _size, _depth, _octId);
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
		if (IsInstanceValid(_debugNode))
		{
			AddChild(_debugNode);
			
			if (ShowOctree)
			{
				_debugNode.Show();
			}
			else
			{
				_debugNode.Hide();
			}
		}


		_collisionSize = _size + _size * 1f / Mathf.Pow(2, _depth);
		//AddChild(DrawBoundingBox(_center, _collisionSize, new Color(0, 0, 1)));
		

		

		SpawnPlanetChunk();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationPredelete || what == NotificationExitTree)
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
		
		if (IsInstanceValid(_debugNode))
		{
			// The parent node will handle freeing this normally,
			// but ensure the reference is cleared
			_debugNode.QueueFree();
		}
		_debugNode = null;
    
		// Clean up children references
		for (int i = 0; i < _children.Length; i++)
		{
			if (IsInstanceValid(_children[i]))
			{
				// Don't call QueueFree here if parent is being freed
				// as children will be freed automatically
				_children[i] = null;
			}
		}
		
		// Clear neighbors
		_neighbours = null;
		_subdivisionQueue.Clear();
	}


	// TODO: Change the subdividing to occur based on the distance to the player/camera instead
	
	// TODO: Add a function that disables and enables all children (show/hide)
	// TODO: Add a function hides and show current octree cell (and disables/enables collision/physics)
	// TODO: When I show/hide I'll use those functions
	// TODO: I'll hide the children until all children are processed then I show them
	
	// TODO: Fix subdivisions when radius is really small (around 1-5)
	
	// TODO: Remove the request for mesh creation when leaving leaving the leaf octree
	// TODO: by removing it from the dictionary and then when we check the queue we check if the request
	// TODO: is in the dictionary, if it is not then we skip that request

	public override void _PhysicsProcess(double delta)
	{
		if (PlayerPosition == null) return;
		
		ProcessSubdivisionQueue();
		
		// If we have subdivided and all children are not processed
		// then we wait until the next frame and check again
		
		if (_subDivided && !HasAllChildrenProcessed())
			return;
		
		if (!UpdatePlayerPosition())
			return;
		
		CheckAndHandleSubdivision();
	}	
	
	private bool HasAllChildrenProcessed()
	{
		// Early return if already processed
		if (_allChildrenProcessed) return true;
		if (_subdivisionQueue.Count > 0) return false;

		// Verify we have valid children - count how many valid children we have
		int validChildCount = 0;
		int processingCount = 0;

		foreach (var child in _children)
		{
			// Skip null or invalid children
			if (child == null || !IsInstanceValid(child)) continue;

			validChildCount++;
			
			// Count how many are still processing
			if (MarchingCubeDispatch.Instance.IsTaskBeingProcessed(child._octId))
			{
				processingCount++;
			}
		}

		//GD.Print($"Valid children: {validChildCount}, Still processing: {processingCount}");
  
		// If we have no valid children or any are still processing, we're not ready
		if (validChildCount == 0 || processingCount > 0) return false;

		// All checks passed, mark as processed and hide parent mesh
		_allChildrenProcessed = true;
		//GD.Print("All children processed, showing all children meshes");

		// Use CallDeferred to ensure UI operations happen on the main thread
		if (IsInstanceValid(_planetMesh))
		{
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
			
			// Show all the children
			foreach (var child in _children)
			{
				if (child != null && IsInstanceValid(child))
				{
					// Show the actual planet mesh of the child
					child.CallDeferred(Node3D.MethodName.Show);

				}
			}
		}

		return true;
		}

		private void ProcessSubdivisionQueue()
		{
			if (_subdivisionQueue.Count > 0)
			{
				var cell = _subdivisionQueue.Dequeue();
				if (IsInstanceValid(cell))
				{
					//CallDeferred(Node.MethodName.AddChild, cell);
					AddChild(cell);

					//cell.CallDeferred(nameof(OnPlanetSpawnerReady), _size / 2f);
					cell.OnPlanetSpawnerReady(_size / 2f);

					// Hide the child
					cell.CallDeferred(Node3D.MethodName.Hide);
					//cell.Hide();

				}
			}
		}
		private bool UpdatePlayerPosition()
		{
			var newPlayerPosition = !Engine.IsEditorHint()
				? PlayerPosition.Get("player_position").AsVector3()
				//: PlayerPosition.Get("position").AsVector3();
				: EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D().Position;

			// Only update if significant movement occurred
			if (_playerPosition.DistanceSquaredTo(newPlayerPosition) < 10) return false;
			_playerPosition = newPlayerPosition;
			return true;

		}

		private void CheckAndHandleSubdivision()
		{
			// Don't subdivide until the planet mesh has been processed
			// if the mesh is still null after it has been processed
			// then we do not need to subdivide

			// Early return conditions
			if (MarchingCubeDispatch.Instance.IsTaskBeingProcessed(_octId)) return;

			/*
			if (!CheckCollision(_playerPosition, _collisionSize))
			{
				MergeSubdivision();
				return;
			}
			*/
		if (!CheckDistanceFromPlayer(_playerPosition, _size))
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
		foreach (var child in _children)
		{
			if(IsInstanceValid(child))
				child.QueueFree();
		}
		// Clear references to children that will be deleted
		Array.Clear(_children, 0, _children.Length);
		_subDivided = false;
		_allChildrenProcessed = false;

		// Use CallDeferred to avoid accessing during tree traversal
		if (IsInstanceValid(_planetMesh))
		{
			CallDeferred(nameof(ShowPlanetMesh));
		}
		
	}
	
	private void ShowPlanetMesh()
	{
		if (IsInstanceValid(_planetMesh))
		{
			_planetMesh.Show();
			foreach (var child in _planetMesh.GetChildren())
			{
				if (child is StaticBody3D staticBody)
				{
					staticBody.ProcessMode = ProcessModeEnum.Inherit;
				}
			}
		}
	}

	private void SubDivide()
	{
		// Implementation for subdivision
		
		var newSize = _size / 4;
		//_planetMesh.CallDeferred(Node3D.MethodName.Hide);
		// Find and disable the collision
		/*
		foreach (var child in _planetMesh.GetChildren())
		{
			if (child is StaticBody3D staticBody)
			{
				staticBody.ProcessMode = Node.ProcessModeEnum.Disabled;
				// Or use: staticBody.CollisionLayer = 0;
			}
		}
		*/
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
		newCell.PlayerPosition = PlayerPosition;
		newCell.OctreePlanetSpawner = OctreePlanetSpawner;
		newCell.ShowOctree = ShowOctree;
		newCell.DistanceFactor = DistanceFactor;
		//newCell.OnPlanetSpawnerReady(_size / 2f);
	}
	
	
	private void CreateNewChildrenAsync(Octree newCell, Vector3 newCenter)
	{
		newCell._center = newCenter;
		newCell._depth = _depth + 1;
		newCell.MaxDepth = MaxDepth;
		newCell.PlayerPosition = PlayerPosition;
		newCell.OctreePlanetSpawner = OctreePlanetSpawner;
		newCell.ShowOctree = ShowOctree;
		newCell.DistanceFactor = DistanceFactor;

		_subdivisionQueue.Enqueue(newCell);

	}
	
	private bool CheckCollision (Vector3 position, float size)
	{
		var newCenter = _center + GlobalPosition;
		
		return position.X > newCenter.X - size / 2 && position.X < newCenter.X + size / 2 &&
		       position.Y > newCenter.Y - size / 2 && position.Y < newCenter.Y + size / 2 &&
		       position.Z > newCenter.Z - size / 2 && position.Z < newCenter.Z + size / 2;
	}
	
	private bool CheckDistanceFromPlayer(Vector3 position, float size)
	{
		var newCenter = _center + GlobalPosition;

		var renderDistance = size + DistanceFactor;

		var newSize = size * DistanceFactor;
		
		// Check if the distance from the player to the center of the octree is less than the size of the octree
		return position.DistanceSquaredTo(newCenter) < newSize * newSize;
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
