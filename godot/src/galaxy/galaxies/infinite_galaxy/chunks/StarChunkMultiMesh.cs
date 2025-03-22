using Godot;
using System;
using System.Collections.Generic;

public partial class StarChunkMultiMesh : MultiMeshInstance3D
{
	public void DrawStars(Transform3D[] transforms, Mesh mesh)
	{
		MultiMesh multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multiMesh.Mesh = mesh;
		multiMesh.InstanceCount = transforms.Length;
		SetMultimesh(multiMesh);

		for (int i = 0; i < transforms.Length; i++)
		{
			Transform3D transform = transforms[i];
			multiMesh.SetInstanceTransform(i, transform);
		}
	}
}
