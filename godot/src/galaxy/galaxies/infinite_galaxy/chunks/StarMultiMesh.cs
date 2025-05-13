using Godot;
using System;
using System.Collections.Generic;

public partial class StarMultiMesh : MultiMeshInstance3D
{
	MultiMesh multiMesh = new MultiMesh();

	public void DrawStars(Vector3[] positions, Mesh mesh)
	{
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multiMesh.Mesh = mesh;
		multiMesh.UseColors = true;

		multiMesh.InstanceCount = positions.Length;
		SetMultimesh(multiMesh);

		for (int i = 0; i < positions.Length; i++)
		{
			Transform3D transform = new Transform3D(Basis.Identity, positions[i]);
			multiMesh.SetInstanceTransform(i, transform);
		}
	}

	public void RedrawStars(Transform3D[] stars)
	{
		if(multiMesh == null)
		{
			GD.PrintErr("MultiMesh is not initialized.");
			return;
		}

		for (int i = 0; i < stars.Length; i++)
		{
			multiMesh.SetInstanceTransform(i, stars[i]);
		}
	}

	public void ColorStar(Color[] colors)
	{
		if (multiMesh == null)
		{
			GD.PrintErr("MultiMesh is not initialized.");
			return;
		}

		for (int i = 0; i < colors.Length; i++)
		{
			multiMesh.SetInstanceColor(i, colors[i]);
		}
	}
}
