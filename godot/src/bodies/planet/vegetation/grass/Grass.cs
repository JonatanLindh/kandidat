using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Array = Godot.Collections.Array;

[Tool]
public partial class Grass : Node
{
	private struct Faces
	{
		public Vector3[] Vertices;
		public Vector3[] Normals;
		public Color[] Colors;
		public int[] Indices;
	}


	[ExportToolButton("Generate Grass")] private Callable ClickMeButton => Callable.From(SpawnGrass);
	[Export] public MeshInstance3D TargetMesh { get; set; }

	[Export]
	public Mesh GrassMesh
	{
		get => _grassMesh;
		set
		{
			_grassMesh = value;
			CallDeferred(nameof(SpawnGrass));
		}
	}

	[Export]
	public float Density
	{
		get => _density;
		set
		{
			_density = value;
			CallDeferred(nameof(SpawnGrass));
		}
	}

	[Export]
	public float TileSize
	{
		get => _tileSize;
		set
		{
			_tileSize = value;
			CallDeferred(nameof(SpawnGrass));
		}
	}

	[Export]
	public int MapRadius
	{
		get => _mapRadius;
		set
		{
			_mapRadius = value;
			CallDeferred(nameof(SpawnGrass));
		}
	}

	[Export] public Material GrassMaterial { get; set; }

	[Export]
	public float GroupTargetArea
	{
		get => _groupTargetArea;
		set
		{
			_groupTargetArea = value;
			CallDeferred(nameof(SpawnGrass));
		}
	}

	private Node3D _grassParent;
	private Mesh _grassMesh;
	private float _density = 1f;
	private float _tileSize = 5f;
	private int _mapRadius = 1;
	private float _groupTargetArea = 1.0f;

	private readonly List<object[]> _grassInstances = [];
	private readonly List<Faces> _meshFaces = new List<Faces>();

	public override void _Ready()
	{
		SpawnGrass();
	}

	private Array ExtractMeshSurface(out Color[] colors)
	{
		var meshData = TargetMesh.Mesh;

		var meshSurface = meshData.SurfaceGetArrays(0);
		var positions = meshSurface[(int)Mesh.ArrayType.Vertex].AsVector3Array();
		var normal = meshSurface[(int)Mesh.ArrayType.Normal].AsVector3Array();
		var indices = meshSurface[(int)Mesh.ArrayType.Index].AsInt32Array();
		colors = new Color[positions.Length];

		_meshFaces.Clear();

		// Parse all faces first
		for (int idx = 0; idx < indices.Length; idx += 3)
		{
			int v1 = indices[idx];
			int v2 = indices[idx + 1];
			int v3 = indices[idx + 2];

			var face = new Faces
			{
				Vertices = [positions[v1], positions[v2], positions[v3]],
				Normals = [normal[v1], normal[v2], normal[v3]],
				Indices = [v1, v2, v3]
			};
			_meshFaces.Add(face);
		}

		return meshSurface;
	}

	private void SpawnGrass()
	{
		//_grassMaterial ??= GD.Load<ShaderMaterial>("res://src/bodies/planet/vegetation/grass/assets/grass.gdshader");
		_grassMesh ??= GD.Load<Mesh>("res://src/bodies/planet/vegetation/grass/assets/grass_high.obj");


		_grassParent?.QueueFree();
		_grassInstances.Clear();
		_grassParent = new Node3D();

		// Extract mesh faces if a mesh is available
		if (TargetMesh is not { Mesh: not null }) return;
		//var meshSurface = ExtractMeshSurface(out var colors);

		PopulateGrassOnMesh();

		// Group faces and assign colors
		//var groups = GroupFaces(_meshFaces, _groupTargetArea);
		//ColorGroup(groups, colors, meshSurface);
		// Create grass instance for each group
		//CreateGrassInstances(groups);

		//AddChild(_grassParent);
		//SetupGrassInstances();
		//GenerateGrassMultiMeshes();
	}


	/// <summary>
	/// Servers the same functionality as the MultMesh function "Populate Surface" which is used to populate the surface of a mesh with a mesh.
	/// Because there is no function to populate the surface of a mesh with a mesh, we had to implement it ourselves.
	/// Based on: https://github.com/godotengine/godot/blob/master/editor/plugins/multimesh_editor_plugin.cpp
	/// </summary>
	private void PopulateGrassOnMesh()
	{
		if (TargetMesh?.Mesh == null)
		{
			GD.PrintErr("No target mesh specified.");
			return;
		}

		Mesh mesh = _grassMesh;
		if (mesh == null)
		{
			GD.PrintErr("No grass mesh specified.");
			return;
		}

		// Create parent if it doesn't exist
		_grassParent?.QueueFree();
		_grassParent = new Node3D();
		AddChild(_grassParent);

		// Get surface data from target mesh
		var meshSurface = TargetMesh.Mesh.SurfaceGetArrays(0);
		var positions = meshSurface[(int)Mesh.ArrayType.Vertex].AsVector3Array();
		var indices = meshSurface[(int)Mesh.ArrayType.Index].AsInt32Array();
		var normals = meshSurface[(int)Mesh.ArrayType.Normal].AsVector3Array();

		// Calculate total area and build area-to-face mapping
		float totalArea = 0;
		var faceAreas = new Dictionary<float, int>();

		for (int i = 0; i < indices.Length; i += 3)
		{
			int v1Idx = indices[i];
			int v2Idx = indices[i + 1];
			int v3Idx = indices[i + 2];

			Vector3 v1 = positions[v1Idx];
			Vector3 v2 = positions[v2Idx];
			Vector3 v3 = positions[v3Idx];

			float area = CalculateTriangleArea(v1, v2, v3);
			if (area < 0.0001f) continue;

			faceAreas[totalArea] = i / 3;
			totalArea += area;
		}

		if (totalArea <= 0 || faceAreas.Count == 0)
		{
			GD.PrintErr("Target mesh has no valid faces.");
			return;
		}

		// Create MultiMesh with correct order of initialization
		var multimesh = new MultiMesh();
		// IMPORTANT: Set transform format before setting instance count
		multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multimesh.InstanceCount = Mathf.CeilToInt(_density * 100);
		multimesh.Mesh = mesh;

		// Populate instances
		for (int i = 0; i < multimesh.InstanceCount; i++)
		{
			// Pick a random position based on area
			float areaPos = (float)GD.RandRange(0, totalArea);

			// Find the face containing this position
			int faceIndex = -1;
			foreach (var entry in faceAreas)
			{
				if (areaPos <= entry.Key)
				{
					faceIndex = entry.Value;
					break;
				}
			}

			if (faceIndex == -1)
				faceIndex = faceAreas.Values.Last();

			// Get face vertices
			int baseIndex = faceIndex * 3;
			int v1Idx = indices[baseIndex];
			int v2Idx = indices[baseIndex + 1];
			int v3Idx = indices[baseIndex + 2];

			Vector3 v1 = positions[v1Idx];
			Vector3 v2 = positions[v2Idx];
			Vector3 v3 = positions[v3Idx];

			// Get a random point inside the face using barycentric coordinates
			float r1 = Mathf.Sqrt(GD.Randf());
			float r2 = GD.Randf();
			Vector3 pos = (1 - r1) * v1 + (r1 * (1 - r2)) * v2 + (r1 * r2) * v3;

			// Get face normal - FLIP the normal to make it point outward from the sphere
			Vector3 normal = ((v2 - v1).Cross(v3 - v1)).Normalized();
			normal = -normal; // Flip the normal for correct orientation on a sphere

			// Create transform with normal as up direction
			var transform = new Transform3D();
			Vector3 upVector = normal; // Using the flipped normal

			// Find perpendicular vectors to create an orthogonal basis
			Vector3 xVector;
			if (Mathf.Abs(upVector.Y) < 0.99f)
				xVector = new Vector3(0, 1, 0).Cross(upVector).Normalized();
			else
				xVector = new Vector3(1, 0, 0).Cross(upVector).Normalized();

			// Get Z vector using cross product
			Vector3 zVector = upVector.Cross(xVector).Normalized();

			// Set the basis with these orthogonal vectors
			transform.Basis = new Basis(xVector, upVector, zVector);
			transform.Origin = pos;

			// Apply random rotations
			//transform.Basis = transform.Basis.Rotated(transform.Basis.Y, (float)GD.RandRange(-rotateRandom, rotateRandom));
			//transform.Basis = transform.Basis.Rotated(transform.Basis.X, (float)GD.RandRange(-tiltRandom, tiltRandom));
			//transform.Basis = transform.Basis.Rotated(transform.Basis.Z, (float)GD.RandRange(-tiltRandom, tiltRandom));

			// Apply random scale
			//float scale = baseScale + (float)GD.RandRange(-scaleRandom, scaleRandom);
			//transform.Basis = transform.Basis.Scaled(new Vector3(scale, scale, scale));

			multimesh.SetInstanceTransform(i, transform);
		}

		// Create multimesh instance
		var grassInstance = new MultiMeshInstance3D();
		grassInstance.Multimesh = multimesh;
		grassInstance.MaterialOverride = GrassMaterial;
		grassInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.On;
		_grassParent.AddChild(grassInstance);
	}

	private float CalculateTriangleArea(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		Vector3 cross = (v2 - v1).Cross(v3 - v1);
		return 0.5f * cross.Length();
	}

	private void ColorGroup(List<List<int>> groups, Color[] colors, Array meshSurface)
	{
		// Apply colors based on groups
		foreach (var group in groups)
		{
			// Generate a unique color for this group
			var groupColor = new Color(
				GD.Randf(),
				GD.Randf(),
				GD.Randf()
			);

			// Apply the color to all faces in this group
			foreach (var faceIndex in group)
			{
				var face = _meshFaces[faceIndex];
				face.Colors = [groupColor, groupColor, groupColor];
				_meshFaces[faceIndex] = face; // Update the face with colors

				// Assign colors to the vertices
				colors[face.Indices[0]] = groupColor;
				colors[face.Indices[1]] = groupColor;
				colors[face.Indices[2]] = groupColor;
			}
		}

		// Add colors to the mesh data
		meshSurface[(int)Mesh.ArrayType.Color] = colors;

		// Create a new mesh with the updated data
		var newMesh = new ArrayMesh();
		newMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, meshSurface);

		// Update the target mesh
		TargetMesh.Mesh = newMesh;
		StandardMaterial3D material = new StandardMaterial3D();
		material.VertexColorUseAsAlbedo = true;
		TargetMesh.MaterialOverride = material;
	}

	private void SetupGrassInstances()
	{
		for (float i = -_mapRadius; i <= _mapRadius; i += _tileSize)
		{
			for (float j = -_mapRadius; j <= _mapRadius; j += _tileSize)
			{
				var grassInstance = GetNodeOrNull<MultiMeshInstance3D>($"GrassInstance_{i}_{j}");
				if (grassInstance != null) RemoveChild(grassInstance);
				var instance = new MultiMeshInstance3D();
				instance.CastShadow = GeometryInstance3D.ShadowCastingSetting.On;
				instance.Position = new Vector3(i, 0f, j);
				instance.Name = $"GrassInstance_{i}_{j}";
				_grassParent.AddChild(instance);
				_grassInstances.Add([instance, instance.Position]);
			}
		}


		/*
		if (_grassInstance != null) RemoveChild(_grassInstance);
		var instance = new MultiMeshInstance3D();
		instance.Position = new Vector3(0f, 0f, 0f);
		AddChild(instance);
		_grassInstance = instance;
		*/
	}

	private void GenerateGrassMultiMeshes()
	{
		// Create a new material and disable backface culling
		StandardMaterial3D material = new StandardMaterial3D();
		//material.SetCullMode(BaseMaterial3D.CullModeEnum.Disabled);
		//material.SetFlag(BaseMaterial3D.Flags.Do, true); 

		foreach (var grassInstance in _grassInstances)
		{
			var instance = (MultiMeshInstance3D)grassInstance[0];
			instance.Multimesh = CreateMeshMultiMesh(_density, _grassMesh);
			if (GrassMaterial != null)
				instance.MaterialOverride = GrassMaterial;
		}
	}

	private MultiMesh CreateMeshMultiMesh(float density, Mesh mesh)
	{
		var rowSize = (int)Math.Ceiling(_tileSize * Mathf.Lerp(0.0f, 10.0f, density));
		var multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multiMesh.Mesh = mesh;
		multiMesh.InstanceCount = rowSize * rowSize;

		var jitterOffset = _tileSize / (float)rowSize * 0.5 * 0.9;
		for (int i = 0; i < rowSize; i++)
		{
			for (int j = 0; j < rowSize; j++)
			{
				var grassPosition = new Vector3(
					(float)((float)i / rowSize - 0.5),
					0,
					(float)((float)j / rowSize - 0.5)
				) * _tileSize;

				var grassOffset = new Vector3(
					(float)GD.RandRange(-jitterOffset, jitterOffset),
					0,
					(float)GD.RandRange(-jitterOffset, jitterOffset)
				);

				multiMesh.SetInstanceTransform(i + j * rowSize,
					new Transform3D(Basis.Identity, grassPosition + grassOffset));
			}
		}

		return multiMesh;
	}


	private List<List<int>> GroupFaces(List<Faces> faces, float targetGroupArea)
	{
		var groups = new List<List<int>>();
		var remainingFaces = new HashSet<int>(Enumerable.Range(0, faces.Count));

		while (remainingFaces.Count > 0)
		{
			var group = new List<int>();
			var startFace = remainingFaces.First();
			group.Add(startFace);
			remainingFaces.Remove(startFace);

			// Get center of first face as initial group center
			Vector3 groupCenter = GetFaceCenter(faces[startFace]);

			// Calculate initial group area
			float currentGroupArea = CalculateFaceArea(faces[startFace]);

			// Add spatially close faces to this group until target area is reached
			while (currentGroupArea < targetGroupArea && remainingFaces.Count > 0)
			{
				// Find the closest face to the group center
				int closestFace = -1;
				float closestDistance = float.MaxValue;

				foreach (int faceIdx in remainingFaces)
				{
					Vector3 faceCenter = GetFaceCenter(faces[faceIdx]);
					float distance = faceCenter.DistanceTo(groupCenter);

					if (distance < closestDistance)
					{
						closestDistance = distance;
						closestFace = faceIdx;
					}
				}

				if (closestFace >= 0)
				{
					group.Add(closestFace);
					remainingFaces.Remove(closestFace);

					// Add area of the new face
					currentGroupArea += CalculateFaceArea(faces[closestFace]);

					// Recalculate group center
					groupCenter = Vector3.Zero;
					foreach (int idx in group)
					{
						groupCenter += GetFaceCenter(faces[idx]);
					}

					groupCenter /= group.Count;
				}
				else
				{
					break;
				}
			}

			groups.Add(group);
		}

		return groups;
	}

	private float CalculateFaceArea(Faces face)
	{
		// Calculate face area (triangle area)
		Vector3 v1 = face.Vertices[0];
		Vector3 v2 = face.Vertices[1];
		Vector3 v3 = face.Vertices[2];

		// Area = 0.5 * |cross product of two sides|
		Vector3 cross = (v2 - v1).Cross(v3 - v1);
		return 0.5f * cross.Length();
	}

	private static Vector3 GetFaceCenter(Faces face)
	{
		return (face.Vertices[0] + face.Vertices[1] + face.Vertices[2]) / 3f;
	}
}