using Godot;
using System;
using System.Collections.Generic;

public partial class StarChunkMultiMesh : MultiMeshInstance3D
{
	public void DrawStars(Vector3[] positions, Mesh mesh)
	{
		MultiMesh multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multiMesh.Mesh = mesh;
		multiMesh.InstanceCount = positions.Length;
		SetMultimesh(multiMesh);

		for (int i = 0; i < positions.Length; i++)
		{
			Transform3D transform = new Transform3D(Basis.Identity, positions[i]);
			multiMesh.SetInstanceTransform(i, transform);
		}
	}
}
