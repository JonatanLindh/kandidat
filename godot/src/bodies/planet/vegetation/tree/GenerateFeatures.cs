using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GenerateFeatures
{
	private readonly int _amountPerSide = 50;
	private readonly Mesh _treeMesh;
	private readonly SurfaceFeature[] _features;
	private AliasMethodVose _aliasMethodVose;

	public enum SamplingMethod
	{
		Uniform,
		Poisson
	} 

	public GenerateFeatures(int amountPerSide, SurfaceFeature[] features)
	{
		_treeMesh = GD.Load<Mesh>("res://src/bodies/planet/vegetation/tree/assets/meshes/tree1_lod0.res");
		_amountPerSide = amountPerSide;
		_features = features;
		_aliasMethodVose = new AliasMethodVose(_features.Select(f => f.Weight).ToArray());
	}

	public MultiMeshInstance3D[] SpawnTrees(SamplingMethod method ,PhysicsDirectSpaceState3D spaceState, 
		Aabb aabb, Vector3 offset = default, Vector3 size = default)
	{

		MultiMeshInstance3D[] trees = method switch
		{
			SamplingMethod.Uniform => SpawnTreesUniform(spaceState, aabb, offset, size),
			SamplingMethod.Poisson => SpawnTreesWithPoisson(spaceState, aabb, offset, size),
			_ => throw new ArgumentOutOfRangeException(nameof(method), method, "Invalid sampling method")
		};
		

		return trees;
		//return SpawnTreesWithPoisson(spaceState, aabb, offset, size);
	}
	
	private MultiMeshInstance3D[] SpawnTreesUniform(PhysicsDirectSpaceState3D spaceState, Aabb aabb, Vector3 offset = default, Vector3 size = default)
	{
		MultiMesh[] multiMeshes = new MultiMesh[_features.Length];
		for (int i = 0; i < _features.Length; i++)
		{
			multiMeshes[i] = new MultiMesh();
			multiMeshes[i].TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
			//multiMeshes[i].InstanceCount = Mathf.RoundToInt(totalCount * (_features[i].Weight / _features.Sum(f => f.Weight)));
			multiMeshes[i].InstanceCount = 6 * _amountPerSide;
			if (_features[i].FeatureMesh == null)
				throw new NullReferenceException("Feature mesh is null");
			multiMeshes[i].Mesh = _features[i].FeatureMesh;
		}

		int amountHit = 0;

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
			var v1 = (face[0] * size) + offset;
			var v2 = (face[1] * size) + offset; 
			var v3 = (face[2] * size) + offset; 
			var v4 = (face[3] * size) + offset; 
			
			for (int k = 0; k < _amountPerSide; k++)
			{
				var selectedFeature = _aliasMethodVose.Next();
				var feature = _features[selectedFeature];
				
				// Get a random point in the face
				float u = (float)GD.RandRange(0.0, 1.0);
				float v = (float)GD.RandRange(0.0, 1.0);
				// Bilinear interpolation
				Vector3 randomPoint = v1 + u * (v2 - v1) + v * (v4 - v1);
			
				Vector3 faceNormal = faceNormals[j];
				Vector3 rayVector = (randomPoint + faceNormal * -5f);
				var query = PhysicsRayQueryParameters3D.Create(randomPoint, rayVector);
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
					transform.Origin = (collisionPoint - offset) / size;
					
					multiMeshes[selectedFeature]
						.SetInstanceTransform(j + k * 6, transform.ScaledLocal(Vector3.One * feature.Scale));
					amountHit++;
				}
			}
			j++;
		}
		var randomPointInstance = new MultiMeshInstance3D[_features.Length];
		for (int i = 0; i < _features.Length; i++)
		{
			randomPointInstance[i] = new MultiMeshInstance3D();
			randomPointInstance[i].Multimesh = multiMeshes[i];
		}
		return randomPointInstance;
	}

	private MultiMeshInstance3D[] SpawnTreesWithPoisson(PhysicsDirectSpaceState3D spaceState, Aabb aabb, Vector3 offset = default,
		Vector3 size = default)
	{
		if (size == default)
			size = Vector3.One;


		
		var scaledAabb = aabb.Size * size;
		var faceNormals = new[]
		{
			new Vector3(0, 0, -1), // Front face
			new Vector3(0, 0, 1),  // Back face
			new Vector3(-1, 0, 0), // Left face
			new Vector3(1, 0, 0),  // Right face
			new Vector3(0, 1, 0),  // Top face
			new Vector3(0, -1, 0)  // Bottom face
		};


		var sampleRadius = scaledAabb.X / (_amountPerSide * 0.5f);
		var sampleTries = 100;
		var poissonPointsPerFace = new List<List<Vector3>>
		{
			PoissonDiscSampling3D.GeneratePoints(sampleRadius, new Vector3(scaledAabb.X, scaledAabb.Y , 0.5f), sampleTries)
				.Select(point => point - new Vector3(scaledAabb.X, scaledAabb.Y, scaledAabb.Z) * 0.5f).ToList(),
			PoissonDiscSampling3D.GeneratePoints(sampleRadius, new Vector3(scaledAabb.X, scaledAabb.Y , 0.5f), sampleTries)
				.Select(point => point - new Vector3(scaledAabb.X, scaledAabb.Y, -scaledAabb.Z) * 0.5f).ToList(),
			
			PoissonDiscSampling3D.GeneratePoints(sampleRadius, new Vector3(0.5f, scaledAabb.Y , scaledAabb.Z), sampleTries)
				.Select(point => point - new Vector3(scaledAabb.X, scaledAabb.Y, scaledAabb.Z) * 0.5f).ToList(),
			PoissonDiscSampling3D.GeneratePoints(sampleRadius, new Vector3(0.5f, scaledAabb.Y , scaledAabb.Z), sampleTries)
				.Select(point => point - new Vector3(-scaledAabb.X, scaledAabb.Y, scaledAabb.Z) * 0.5f).ToList(),
			
			PoissonDiscSampling3D.GeneratePoints(sampleRadius, new Vector3(scaledAabb.X, 0.5f , scaledAabb.Z), sampleTries)
				.Select(point => point - new Vector3(scaledAabb.X, -scaledAabb.Y, scaledAabb.Z) * 0.5f).ToList(),
			PoissonDiscSampling3D.GeneratePoints(sampleRadius, new Vector3(scaledAabb.X, 0.5f , scaledAabb.Z), sampleTries)
				.Select(point => point - new Vector3(scaledAabb.X, scaledAabb.Y, scaledAabb.Z) * 0.5f).ToList()
				
		};

		var totalCount = poissonPointsPerFace.Select(list => list.Count).ToList().Sum();
		MultiMesh[] multiMeshes = new MultiMesh[_features.Length];
		for (int i = 0; i < _features.Length; i++)
		{
			multiMeshes[i] = new MultiMesh();
			multiMeshes[i].TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
			//multiMeshes[i].InstanceCount = Mathf.RoundToInt(totalCount * (_features[i].Weight / _features.Sum(f => f.Weight)));
			multiMeshes[i].InstanceCount = totalCount;
			if (_features[i].FeatureMesh == null)
				return null;
			multiMeshes[i].Mesh = _features[i].FeatureMesh;
		}
		var instanceCount = 0;
		for (int i = 0; i < faceNormals.Length; i++)
		{
			var face = poissonPointsPerFace[i];
			for (int j = 0; j < face.Count; j++)
			{
				var selectedFeature = _aliasMethodVose.Next();
				var feature = _features[selectedFeature];
				
				
				var point = face[j] + offset;
				
				Vector3 faceNormal = faceNormals[i];
				Vector3 rayVector = (point - faceNormal * 20f);
				
				var query = PhysicsRayQueryParameters3D.Create(point, rayVector);
				var result = spaceState.IntersectRay(query);
				if (result.Count > 0)
				{
					// If the ray hits something, log the collision point
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
					transform.Origin = (collisionPoint - offset) / size;
					
					//multiMesh.SetInstanceTransform(instanceCount, transform.ScaledLocal(Vector3.One * feature.Scale));
					multiMeshes[selectedFeature].SetInstanceTransform
						(instanceCount, transform.ScaledLocal(Vector3.One * feature.Scale));
					instanceCount++;
				}

			}
		} 
		var randomPointInstance = new MultiMeshInstance3D[_features.Length];
		for (int i = 0; i < _features.Length; i++)
		{
			randomPointInstance[i] = new MultiMeshInstance3D();
			randomPointInstance[i].Multimesh = multiMeshes[i];
		}
		return randomPointInstance;
	}
}
