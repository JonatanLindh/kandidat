using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A spatial partitioning system for planet terrain using octree subdivision.
/// <para>
/// This class dynamically manages level-of-detail for planet terrain by creating
/// an octree structure that subdivides space around the player. Areas closer to 
/// the player are rendered at higher detail using recursive subdivision, while
/// distant areas use lower-detail representations to maintain performance.
/// </para>
/// <para>
/// Each octree cell manages its own mesh generation through the MarchingCubeDispatch
/// system, creating or merging terrain chunks as needed when the player moves.
/// Terrain generation happens asynchronously to avoid impacting frame rate.
/// </para>
/// </summary>
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
	
	/// <summary>
	/// Spawns a planet chunk using the OctreePlanetSpawner
	/// </summary>
	private void SpawnPlanetChunk()
	{
		switch (OctreePlanetSpawner)
		{
			// Cast to OctreePlanetSpawner
			case OctreePlanetSpawner spawner:
				if (IsInstanceValid(spawner))
					_planetMesh = spawner.SpawnChunk(this, _center, _size, _depth, _octId);
				break;
			case null:
				GD.PrintErr("OctreePlanetSpawner is null");
				return;
		}
	}

	/// <summary>
	/// Initializes the octree when the planet spawner is ready
	/// </summary>
	public void OnPlanetSpawnerReady(float size)
	{
		// Set up player position reference
		PlayerPosition ??= !Engine.IsEditorHint() 
			? GetNode("/root/PlayerVariables") 
			: EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D();
		
		_playerPosition = !Engine.IsEditorHint() ? PlayerPosition.Get("player_position").AsVector3() :
			EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D().Position;

		_size = size;
		_collisionSize = _size + _size * 1f / Mathf.Pow(2, _depth);
		
		// Create debug visualization
		SetupDebugVisualization();
		
		SpawnPlanetChunk();
	}
	
	private void SetupDebugVisualization()
	{
		_debugNode = DrawBoundingBox(_center, _size, new Color(1, 0, 0));
		if (IsInstanceValid(_debugNode))
		{
			AddChild(_debugNode);
			_debugNode.Visible = ShowOctree;
		}
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
			_debugNode.QueueFree();
		}
		_debugNode = null;

		// Clean up children references
		for (int i = 0; i < _children.Length; i++)
		{
			_children[i] = null;
		}
		
		_subdivisionQueue.Clear();
	}

	

	public override void _PhysicsProcess(double delta)
	{
		if (PlayerPosition == null) return;
		
		ProcessSubdivisionQueue();
		
		// If we have subdivided and all children are not processed
		// then we wait until the next frame and check again
		if (_subDivided && !HasAllChildrenBeenProcessed())
			return;
		
		if (!UpdatePlayerPosition())
			return;
		
		CheckAndHandleSubdivision();
	}	
	
	private bool HasAllChildrenBeenProcessed()
	{
		// Early return if already processed
		if (_allChildrenProcessed) return true;
		if (_subdivisionQueue.Count > 0) return false;

		// Count valid children and those still processing
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
		
		// If we have no valid children or any are still processing, we're not ready
		if (validChildCount == 0 || processingCount > 0) return false;

		// All children are processed, hide parent mesh and show children
		_allChildrenProcessed = true;
		HidePlanetMesh();

		// Show all the children
		foreach (var child in _children)
		{
			if (child != null && IsInstanceValid(child))
			{
				// Show the actual planet mesh of the child
				child.CallDeferred(Node3D.MethodName.Show);
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
					AddChild(cell);
					cell.OnPlanetSpawnerReady(_size / 2f);
					cell.Hide();
					//cell.CallDeferred(Node3D.MethodName.Hide);

				}
			}
		}
	private bool UpdatePlayerPosition()
		{ 
			var newPlayerPosition = GetCurrentPlayerPosition();

			// Only update if significant movement occurred
			if (_playerPosition.DistanceSquaredTo(newPlayerPosition) < _size * _size / 100f) 
				return false;
			
			_playerPosition = newPlayerPosition;
			return true;

		}
	
	private Vector3 GetCurrentPlayerPosition()
	{
		if (Engine.IsEditorHint())
			return EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D().Position;
        
		return PlayerPosition.Get("player_position").AsVector3();
	}

	private void CheckAndHandleSubdivision()
	{ 
		// Don't subdivide if mesh is still being processed
		if (MarchingCubeDispatch.Instance.IsTaskBeingProcessed(_octId)) 
			return;

		if (!CheckDistanceFromPlayer(_playerPosition, _size))
		{
			MergeSubdivision();
			return;
		}

		// Subdivide if needed
		if (!_subDivided && _depth < MaxDepth && IsValidForSubdivision())
		{
			SubDivide();
		}
	}
	
	private bool IsValidForSubdivision()
	{
		return _planetMesh != null && IsInstanceValid(_planetMesh) && _planetMesh.Mesh != null;
	}

	private void MergeSubdivision() 
	{
		if (!_subDivided) return;
		
		// Cancel any pending mesh generation request for this octree cell
		MarchingCubeDispatch.Instance.CancelRequest(_octId);
		
		// Clean up children
		foreach (var child in _children)
		{
			if(IsInstanceValid(child))
				child.QueueFree();
		}
		
		// Clear references to children that will be deleted
		Array.Clear(_children, 0, _children.Length);
		_subDivided = false;
		_allChildrenProcessed = false;

		CallDeferred(nameof(ShowPlanetMesh));
		
	}
	
	private void ShowPlanetMesh()
	{
		if (!IsInstanceValid(_planetMesh)) return;
        
		_planetMesh.Show();
        
		// Enable collision
		foreach (var child in _planetMesh.GetChildren())
		{
			if (child is StaticBody3D staticBody)
			{
				staticBody.ProcessMode = ProcessModeEnum.Inherit;
			}
		}
	}
	
	private void HidePlanetMesh()
	{
		if (!IsInstanceValid(_planetMesh)) return;
        
		_planetMesh.CallDeferred(Node3D.MethodName.Hide);
        
		// Disable collision
		foreach (var child in _planetMesh.GetChildren())
		{
			if (child is StaticBody3D staticBody)
			{
				staticBody.ProcessMode = ProcessModeEnum.Disabled;
			}
		}

	}

	private void SubDivide()
	{
		var newSize = _size / 4;

        // Create 8 children in the subdivision queue
		for (int i = 0; i < 8; i++)
		{
			_children[i] = new Octree();
			CreateChildForSubdivision(_children[i], _center + newSize * _octant[i]);

		}
		
		_subDivided = true;
	}
	
	
	private void CreateChildForSubdivision(Octree newCell, Vector3 newCenter)
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
	
	
	private bool CheckDistanceFromPlayer(Vector3 position, float size)
	{
		var newCenter = _center + GlobalPosition;
		var newSize = size * DistanceFactor;
		
		// Check if player is within range
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
