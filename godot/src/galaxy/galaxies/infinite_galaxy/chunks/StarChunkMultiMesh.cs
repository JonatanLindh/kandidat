using Godot;
using System;
using System.Collections.Generic;

public partial class StarChunkMultiMesh : MultiMeshInstance3D
{
	public void DrawStars(List<Transform3D> transforms, int count, Mesh mesh)
	{
		MultiMesh multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multiMesh.Mesh = mesh;
		multiMesh.InstanceCount = count;
		SetMultimesh(multiMesh);

		for (int i = 0; i < count; i++)
		{
			Transform3D transform = transforms[i];
			multiMesh.SetInstanceTransform(i, transform);
		}
	}
}
