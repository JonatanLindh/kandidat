using Godot;
using System;

[Tool]
public partial class Octree : Node
{
	[Export] public Node3D PlayerPosition { get; set; }
	[Export] public int maxDepth { get; set; } = 2;
	[Export] public Node OctreePlanetSpawner { get; set; }
	
	public bool PlanetSpawnerInitialized = false;

	private float _size = 64;
	private Vector3 _center = Vector3.Zero;

	private Node3D _rootTest;

	private bool _subDivided  = false;

	private int _depth = 0;

	private float _collisionSize;
	
	
	private Guid _octId = Guid.NewGuid();
	
	
	//Octree topLeft = null;
	//Octree topRight = null;
	
	private Octree[] _children = new Octree[8];

	private MeshInstance3D _planetMesh;
	
	//public OctreePlanetSpawner OctreePlanetSpawner;
	
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
				_planetMesh = spawner.SpawnChunk(_center, _size, _depth);
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
		
		
		_size = size;
		//AddChild(DrawBoundingBox(_center, _size, new Color(1, 0, 0)));
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
		}
	}



	public override void _PhysicsProcess(double delta)
	{
		if (PlayerPosition == null) return;

		if (_planetMesh is { Mesh: null })
		{
			//GD.Print("Planet mesh is null");
		}
		
		if (!CheckCollision(PlayerPosition.Position, _collisionSize))
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
		
		if (_subDivided || _depth >= maxDepth) return;
		
		var newSize = _size / 4;
		
		// Subdivide the octree into 8 children
		for (int i = 0; i < 8; i++)
		{
			_children[i] = new Octree();
			SubDivide(_children[i], _center + newSize * _octant[i]);
			AddChild(_children[i]);
		}
		_subDivided = true;


	}	

	private void SubDivide(Octree newCell, Vector3 newCenter)
	{
		_planetMesh.Hide();
		newCell._center = newCenter;
		//newCell.Size = Size / 2;
		newCell._depth = _depth + 1;
		newCell.maxDepth = maxDepth;
		newCell._rootTest = _rootTest;
		newCell.PlayerPosition = PlayerPosition;
		newCell.OctreePlanetSpawner = OctreePlanetSpawner;
		newCell.OnPlanetSpawnerReady(_size / 2f);
	}

	private bool CheckCollision (Vector3 position, float size)
	{
		return position.X > _center.X - size / 2 && position.X < _center.X + size / 2 &&
		       position.Y > _center.Y - size / 2 && position.Y < _center.Y + size / 2 &&
		       position.Z > _center.Z - size / 2 && position.Z < _center.Z + size / 2;
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
