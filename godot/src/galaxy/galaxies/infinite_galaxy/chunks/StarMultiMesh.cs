using Godot;
using System;
using System.Collections.Generic;

public partial class StarMultiMesh : MultiMeshInstance3D
{
	MultiMesh multiMesh = new MultiMesh();

	/// <summary>
	/// Initalizes the multimesh and draws the stars.
	/// </summary>
	/// <param name="positions"></param>
	/// <param name="mesh"></param>
	public void DrawStars(Vector3[] positions, Mesh mesh)
	{
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

	/// <summary>
	/// <para>Redraws the stars in the multimesh.</para>
	/// Make sure to initialize the multimesh with <code>DrawStars()</code> before calling this method.
	/// </summary>
	/// <param name="stars"></param>
	public void RedrawStars(Transform3D[] stars)
	{
		if(multiMesh == null)
		{
			GD.PrintErr("StarMultimesh: MultiMesh is not initialized.");
			return;
		}

		for (int i = 0; i < stars.Length; i++)
		{
			multiMesh.SetInstanceTransform(i, stars[i]);
		}
	}
}
