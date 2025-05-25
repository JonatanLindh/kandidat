using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class NewGrass
{

	private readonly Mesh _mesh;
	private readonly ShaderMaterial _material;

	public struct Face
	{
		public Vector3 v1;
		public Vector3 v2;
		public Vector3 v3;

		public Face(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
		}
	}

	public NewGrass()
	{
		_mesh = GD.Load<Mesh>("res://src/bodies/planet/vegetation/grass/assets/grass_high.obj");
		var shader = GD.Load<Shader>("res://src/bodies/planet/vegetation/grass/assets/grass.gdshader");
		_material = new ShaderMaterial();
		_material.Shader = shader;
	}
	
	public MultiMeshInstance3D PopulateMesh(Godot.Collections.Array meshData, float density = 1.0f)
	{
		var vertices = meshData[(int)Mesh.ArrayType.Vertex].AsVector3Array();
		var indices = meshData[(int)Mesh.ArrayType.Index].AsInt32Array();
		List<Transform3D> multiMeshTransforms = new List<Transform3D>();
		

		int instanceIndex = 0;

		float totalArea = 0f;
		
		// Calculate area for each face
		List<(int faceStart, float area)> faceAreas = new List<(int, float)>();
  
		for (int i = 0; i < indices.Length; i += 3)
		{
			Vector3 v1 = vertices[indices[i]];
			Vector3 v2 = vertices[indices[i + 1]];
			Vector3 v3 = vertices[indices[i + 2]];
    
			// Calculate face area
			Vector3 cross = (v2 - v1).Cross(v3 - v1);
			float area = 0.5f * cross.Length();
    
			faceAreas.Add((i, area));
			totalArea += area;
		}

		// Calculate target total instances based on total area and density
		int totalInstanceTarget = Mathf.Max(1, Mathf.FloorToInt(totalArea * density));
		
		// Distribute instances based on area ratio
		foreach (var (faceStartIndex, faceArea) in faceAreas)
		{
			// Calculate proportional instances for this face
			float faceAreaRatio = faceArea / totalArea;
			float idealInstanceCount = totalInstanceTarget * faceAreaRatio;
    
			// For very small faces with less than 1 ideal instance,
			// use probability to determine if they get an instance
			int faceInstances;
			if (idealInstanceCount < 1.0f)
			{
				// Probabilistic approach - chance equals the fractional ideal count
				faceInstances = GD.Randf() < idealInstanceCount ? 1 : 0;
			}
			else
			{
				// Round to nearest integer instead of floor for better distribution
				faceInstances = Mathf.RoundToInt(idealInstanceCount);
			}
			
			
			
			// Calculate proportional instances for this face
			//float faceAreaRatio = faceArea / totalArea;
			//int faceInstances = Mathf.FloorToInt(totalInstanceCount * faceAreaRatio);
			for (int j = 0; j < faceInstances; j++)
			{
				Vector3 v1 = vertices[indices[faceStartIndex]];
				Vector3 v2 = vertices[indices[faceStartIndex + 1]];
				Vector3 v3 = vertices[indices[faceStartIndex + 2]];

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
				
				multiMeshTransforms.Add(transform);
				
				/*

				// Add random value as custom data (stable across movements)
				float randomValue1 = (float)GD.RandRange(0, 1000) / 1000f;
				float randomValue2 = (float)GD.RandRange(0, 1000) / 1000f;
				float randomValue3 = (float)GD.RandRange(0, 1000) / 1000f;
				multiMesh.SetInstanceCustomData(instanceIndex, 
					new Color
						(
							randomValue1, 
							randomValue2,
							randomValue3, 
							0)
					);
				multiMesh.SetInstanceTransform(instanceIndex, transform);
				*/
			}
		}
		
		MultiMesh multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multiMesh.Mesh = _mesh;
		multiMesh.UseCustomData = true; // Enable custom data
		
		// Set the total instance count
		multiMesh.InstanceCount =  multiMeshTransforms.Count;

		foreach (var transform in multiMeshTransforms)
		{
			// Add random value as custom data (stable across movements)
			float randomValue1 = (float)GD.RandRange(0, 1000) / 1000f;
			float randomValue2 = (float)GD.RandRange(0, 1000) / 1000f;
			float randomValue3 = (float)GD.RandRange(0, 1000) / 1000f;
			multiMesh.SetInstanceCustomData(instanceIndex, 
				new Color
				(
					randomValue1, 
					randomValue2,
					randomValue3, 
					0)
			);
			multiMesh.SetInstanceTransform(instanceIndex, transform);
			instanceIndex++;
		}

		
		// Create a new instance of the grass mesh
		MultiMeshInstance3D instance = new MultiMeshInstance3D();
		instance.Multimesh = multiMesh;
		instance.MaterialOverride = _material;
		instance.CastShadow = GeometryInstance3D.ShadowCastingSetting.On;

		return instance;
	}
}
