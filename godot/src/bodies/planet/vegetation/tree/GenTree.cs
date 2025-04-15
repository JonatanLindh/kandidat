using Godot;
using System;
using System.Collections.Generic;

public class GenTree
{
	private readonly int _amountPerSide = 50;
	private readonly Mesh _treeMesh;
	private readonly float _scale = 1f;

	public GenTree(int amountPerSide, float scale)
	{
		_treeMesh = GD.Load<Mesh>("res://src/bodies/planet/vegetation/tree/assets/meshes/tree1_lod0.res");
		_amountPerSide = amountPerSide;
		_scale = scale;
	}
	
	public MultiMeshInstance3D SpawnTrees(Aabb aabb, PhysicsDirectSpaceState3D spaceState)
	{
		MultiMesh multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multiMesh.Mesh = _treeMesh;
		multiMesh.InstanceCount = 6 * _amountPerSide;

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

			
			for (int k = 0; k < _amountPerSide; k++)
			{
				// Get a random point in the face
				float u = (float)GD.RandRange(0.0, 1.0);
				float v = (float)GD.RandRange(0.0, 1.0);
				// Bilinear interpolation
				Vector3 randomPoint = v1 + u * (v2 - v1) + v * (v4 - v1);
				//multiMesh.SetInstanceTransform(j, new Transform3D(Basis.Identity, randomPoint));
			
				Vector3 faceNormal = faceNormals[j];

				var query = PhysicsRayQueryParameters3D.Create(randomPoint, faceNormal * -5f);
				var result = spaceState.IntersectRay(query);
				if (result.Count > 0)
				{
					// If the ray hits something, log the collision point
					//GD.Print($"Raycast hit at: {result["position"]}");
					
					// Set the instance transform to the hit points
					var collisionPoint = result["position"].AsVector3();
					var transform = new Transform3D();
					
					Vector3 upVector = result["normal"].AsVector3(); 
					
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
					transform.Origin = collisionPoint;
					
					multiMesh.SetInstanceTransform(j + k * 6, transform.ScaledLocal(Vector3.One * _scale));
				
				}
			}
			j++;
		}
		var randomPointInstance = new MultiMeshInstance3D();
		randomPointInstance.Multimesh = multiMesh;
		return randomPointInstance;
	}
	
}
