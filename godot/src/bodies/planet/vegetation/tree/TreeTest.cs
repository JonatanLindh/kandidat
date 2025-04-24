using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class TreeTest : Node3D
{
	[Export]
	public MeshInstance3D MeshInstance { get; set; }
	
	[Export] public int AmountPerSide { get; set; } = 50;
	
	[Export] public SurfaceFeature[] SurfaceFeatures { get; set; }
	
	
	private Node3D _parentNode;

	


	public override void _Ready()
	{
		if (MeshInstance == null || SurfaceFeatures.Length == 0) return;
		GenerateFeatures generateFeatures = new GenerateFeatures(AmountPerSide, SurfaceFeatures);
		_parentNode?.QueueFree();
		_parentNode = new Node3D();
		AddChild(_parentNode);
		Aabb bounds = MeshInstance.GetAabb();
		DrawBoundingBox(bounds);

		var trees = generateFeatures.SpawnTrees(GenerateFeatures.SamplingMethod.Poisson, GetWorld3D().DirectSpaceState, bounds);
		foreach (var tree in trees)
		{
			_parentNode.AddChild(tree);
		}
		//DrawPoissonPoints(bounds);

	}

	private void DrawPoissonPoints(Aabb aabb)
	{
		//var points = PoissonDiscSampling2D.GeneratePoints
		//	(2f, new Vector2(10f, 10f), 30);
		
		var corners = new[]
		{
			new Vector3(aabb.Position.X, aabb.Position.Y, aabb.Position.Z),
			new Vector3(aabb.Position.X + aabb.Size.X, aabb.Position.Y, aabb.Position.Z),
			new Vector3(aabb.Position.X + aabb.Size.X, aabb.Position.Y + aabb.Size.Y, aabb.Position.Z),
			new Vector3(aabb.Position.X, aabb.Position.Y + aabb.Size.Y, aabb.Position.Z),
			new Vector3(aabb.Position.X, aabb.Position.Y, aabb.Position.Z + aabb.Size.Z),
			new Vector3(aabb.Position.X + aabb.Size.X, aabb.Position.Y, aabb.Position.Z + aabb.Size.Z),
			new Vector3(aabb.Position.X + aabb.Size.X, aabb.Position.Y + aabb.Size.Y, aabb.Position.Z + aabb.Size.Z),
			new Vector3(aabb.Position.X, aabb.Position.Y + aabb.Size.Y, aabb.Position.Z + aabb.Size.Z)
		};

		var aabbFaces = new List<Vector3[]>
		{
			new[] { corners[0], corners[1], corners[2], corners[3] }, // Front face
			new[] { corners[4], corners[5], corners[6], corners[7] }, // Back face
			new[] { corners[0], corners[3], corners[7], corners[4] }, // Left face
			new[] { corners[1], corners[2], corners[6], corners[5] }, // Right face
			new[] { corners[3], corners[2], corners[6], corners[7] }, // Top face
			new[] { corners[0], corners[1], corners[5], corners[4] }  // Bottom face
		};
		var sampleRadius = 2f;
		var sampleTries = 100;
		var poissonPointsPerFace = new List<List<Vector3>>
			{
				PoissonDiscSampling3D.GeneratePoints(sampleRadius, new Vector3(aabb.Size.X, aabb.Size.Y , 0.5f), sampleTries)
					.Select(point => point - new Vector3(aabb.Size.X, aabb.Size.Y, aabb.Size.Z) * 0.5f).ToList(),
				PoissonDiscSampling3D.GeneratePoints(sampleRadius, new Vector3(aabb.Size.X, aabb.Size.Y , 0.5f), sampleTries)
					.Select(point => point - new Vector3(aabb.Size.X, aabb.Size.Y, -aabb.Size.Z) * 0.5f).ToList(),
				PoissonDiscSampling3D.GeneratePoints(sampleRadius, new Vector3(0.5f, aabb.Size.Y , aabb.Size.Z), sampleTries)
					.Select(point => point - new Vector3(aabb.Size.X, aabb.Size.Y, aabb.Size.Z) * 0.5f).ToList(),
				PoissonDiscSampling3D.GeneratePoints(sampleRadius, new Vector3(0.5f, aabb.Size.Y , aabb.Size.Z), sampleTries)
					.Select(point => point - new Vector3(-aabb.Size.X, aabb.Size.Y, aabb.Size.Z) * 0.5f).ToList(),
				PoissonDiscSampling3D.GeneratePoints(sampleRadius, new Vector3(aabb.Size.X, 0.5f , aabb.Size.Z), sampleTries)
					.Select(point => point - new Vector3(aabb.Size.X, aabb.Size.Y, aabb.Size.Z) * 0.5f).ToList(),
				PoissonDiscSampling3D.GeneratePoints(sampleRadius, new Vector3(aabb.Size.X, 0.5f , aabb.Size.Z), sampleTries)
					.Select(point => point - new Vector3(aabb.Size.X, -aabb.Size.Y, aabb.Size.Z) * 0.5f).ToList()
			};

		var totalCount = poissonPointsPerFace.Select(list => list.Count).ToList().Sum();
		
		var faceNormals = new[]
		{
			new Vector3(0, 0, -1), // Front face
			new Vector3(0, 0, 1),  // Back face
			new Vector3(-1, 0, 0), // Left face
			new Vector3(1, 0, 0),  // Right face
			new Vector3(0, 1, 0),  // Top face
			new Vector3(0, -1, 0)  // Bottom face
		};
		
		// PoissonDiscSampling3D.GeneratePoints(1f, new Vector3(aabb.Size.X, aabb.Size.Y , 0.5f), 30) [1]
		// PoissonDiscSampling3D.GeneratePoints(1f, new Vector3(0.5f, aabb.Size.Y, aabb.Size.Z), 30) [3]
		
		List<Vector3> points3d = 
			PoissonDiscSampling3D.GeneratePoints(1f, new Vector3(aabb.Size.X, 0.5f , aabb.Size.Z), 30)
				.Select(point => point - new Vector3(aabb.Size.X, 0, aabb.Size.Z) * 0.5f).ToList();
		
		
		MultiMesh multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multiMesh.InstanceCount = totalCount;
		multiMesh.Mesh = new BoxMesh()
		{
			Size = 0.5f * Vector3.One
		};
		
		var instanceCount = 0;
		for (int i = 0; i < poissonPointsPerFace.Count; i++)
		{
			var face = poissonPointsPerFace[i];
			for (int j = 0; j < face.Count; j++)
			{
				var point = face[j];
				Transform3D transform = new Transform3D(Basis.Identity, point);
				multiMesh.SetInstanceTransform(instanceCount, transform);
				instanceCount++;
			}
		}

		/*
		for (int i = 0; i < points3d.Count; i++)
		{
			Vector3 point = points3d[i];
			var offset = faceNormals[4] * aabb.Size * 0.5f;
			point += offset;
			Transform3D transform = new Transform3D(Basis.Identity, point);
			multiMesh.SetInstanceTransform(i, transform);
		}
		*/

		MultiMeshInstance3D meshInstance = new MultiMeshInstance3D();
		meshInstance.Multimesh = multiMesh;
		AddChild(meshInstance);
		
	}

	private void DrawBoundingBox(Aabb aabb)
	{
		ImmediateMesh immediateMesh = new ImmediateMesh();
		immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
		
		MultiMesh multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multiMesh.InstanceCount = 6 * AmountPerSide;
		multiMesh.Mesh = new BoxMesh()
		{
			Size = 0.5f * Vector3.One
		};
		
		var corners = new[] {
			new Vector3(aabb.Position.X, aabb.Position.Y, aabb.Position.Z),
			new Vector3(aabb.Position.X + aabb.Size.X, aabb.Position.Y, aabb.Position.Z),
			new Vector3(aabb.Position.X + aabb.Size.X, aabb.Position.Y + aabb.Size.Y, aabb.Position.Z),
			new Vector3(aabb.Position.X, aabb.Position.Y + aabb.Size.Y, aabb.Position.Z),
			new Vector3(aabb.Position.X, aabb.Position.Y, aabb.Position.Z + aabb.Size.Z),
			new Vector3(aabb.Position.X + aabb.Size.X, aabb.Position.Y, aabb.Position.Z + aabb.Size.Z),
			new Vector3(aabb.Position.X + aabb.Size.X, aabb.Position.Y + aabb.Size.Y, aabb.Position.Z + aabb.Size.Z),
			new Vector3(aabb.Position.X, aabb.Position.Y + aabb.Size.Y, aabb.Position.Z + aabb.Size.Z)
		};
		
		var aabbFaces = new List<Vector3[]>
		{
			// Front face
			new[] { corners[0], corners[1], corners[2], corners[3] },
			// Back face
			new[] { corners[4], corners[5], corners[6], corners[7] },
			// Left face
			new[] { corners[0], corners[3], corners[7], corners[4] },
			// Right face
			new[] { corners[1], corners[2], corners[6], corners[5] },
			// Top face
			new[] { corners[3], corners[2], corners[6], corners[7] },
			// Bottom face
			new[] { corners[0], corners[1], corners[5], corners[4] }
		};
		
		// Define the normals for each face of the Aabb
		var faceNormals = new[]
		{
			new Vector3(0, 0, -1), // Front face
			new Vector3(0, 0, 1),  // Back face
			new Vector3(-1, 0, 0), // Left face
			new Vector3(1, 0, 0),  // Right face
			new Vector3(0, 1, 0),  // Top face
			new Vector3(0, -1, 0)  // Bottom face
		};
		
		int j = 0;
		foreach (var face in aabbFaces)
		{
			var v1 = face[0];
			var v2 = face[1];
			var v3 = face[2];
			var v4 = face[3];
			
			for (int i = 0; i < AmountPerSide; i++)
			{
				float u = (float)GD.RandRange(0.0, 1.0);
				float v = (float)GD.RandRange(0.0, 1.0);
				Vector3 randomPoint = v1 + u * (v2 - v1) + v * (v4 - v1);
				
				Transform3D transform = new Transform3D(Basis.Identity, randomPoint);

				multiMesh.SetInstanceTransform(j + i * 6, transform);
			}
			j++;
		}
		

		var edges = new[]
		{
			new[] { 0, 1 }, new[] { 1, 2 }, new[] { 2, 3 }, new[] { 3, 0 },
			new[] { 4, 5 }, new[] { 5, 6 }, new[] { 6, 7 }, new[] { 7, 4 },
			new[] { 0, 4 }, new[] { 1, 5 }, new[] { 2, 6 }, new[] { 3, 7 }
		};
		foreach (var edge in edges)
		{
			immediateMesh.SurfaceSetColor(new Color(1, 0, 0));
			immediateMesh.SurfaceAddVertex(corners[edge[0]]);
			immediateMesh.SurfaceSetColor(new Color(1, 0, 0));
			immediateMesh.SurfaceAddVertex(corners[edge[1]]);
		}
		immediateMesh.SurfaceEnd();
		MeshInstance3D meshInstance = new MeshInstance3D();
		MultiMeshInstance3D multiMeshInstance = new MultiMeshInstance3D();
		multiMeshInstance.Multimesh = multiMesh;
		meshInstance.Mesh = immediateMesh;
		Material material = new StandardMaterial3D()
		{
			ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
			VertexColorUseAsAlbedo = true,
			DisableFog = true
		};
		meshInstance.MaterialOverride = material;
		multiMeshInstance.MaterialOverride = material;
		AddChild(meshInstance);
		//AddChild(multiMeshInstance);
	}
}

