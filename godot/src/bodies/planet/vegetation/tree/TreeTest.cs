using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class TreeTest : Node3D
{
	[Export]
	public MeshInstance3D MeshInstance { get; set; }

	[Export] public float Scale { get; set; } = 10f;
	
	[Export] public int AmountPerSide { get; set; } = 50;
	
	
	private Node3D _parentNode;


	public override void _Ready()
	{
		if (MeshInstance == null) return;
		GenTree genTree = new GenTree(AmountPerSide, Scale);
		_parentNode?.QueueFree();
		_parentNode = new Node3D();
		AddChild(_parentNode);
		Aabb bounds = MeshInstance.GetAabb();
		DrawBoundingBox(bounds);
		DrawPossionPoints(bounds);
		
	}

	private void DrawPossionPoints(Aabb aabb)
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
		
		var poissonPointsPerFace = new List<List<Vector3>>();
		//poissonPointsPerFace.Add(PoissonDiscSampling3D.GeneratePoints(1f, new Vector3(10f, 1f, 10f), 30));
		var totalCount = 0;
		
		var faceNormals = new[]
		{
			new Vector3(0, 0, -1), // Front face
			new Vector3(0, 0, 1),  // Back face
			new Vector3(-1, 0, 0), // Left face
			new Vector3(1, 0, 0),  // Right face
			new Vector3(0, 1, 0),  // Top face
			new Vector3(0, -1, 0)  // Bottom face
		};
		
		
		poissonPointsPerFace.Add(
			PoissonDiscSampling3D.GeneratePoints(1f, new Vector3(aabb.Size.X, 1f, aabb.Size.Z), 30)
			);
		
		poissonPointsPerFace[0].ForEach(delegate(Vector3 point)
		{
			point += faceNormals[0] * aabb.Size.Z;
		});
		
		
		


		
		
		MultiMesh multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multiMesh.InstanceCount = totalCount;
		multiMesh.Mesh = new BoxMesh()
		{
			Size = 0.5f * Vector3.One
		};
		
		int instanceIndex = 0;
		for (int i = 0; i < aabbFaces.Count; i++)
		{
			var points = poissonPointsPerFace[i];
			
			for (int j = 0; j < points.Count; j++)
			{
				Vector3 point = points[j];
				// Offset the point to the correct side of the face
				points[j] = new Vector3(
					point.X,
					point.Y,
					point.Z
				);
				
				Transform3D transform = new Transform3D(Basis.Identity, point);
				multiMesh.SetInstanceTransform(instanceIndex, transform);
				instanceIndex++;
			}
		}
		
		/*
		for (int i = 0; i < points.Count; i++)
		{
			Vector3 point = new Vector3(points[i].X, 0, points[i].Y);
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
