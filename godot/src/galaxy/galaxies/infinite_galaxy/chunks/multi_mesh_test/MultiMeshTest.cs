using Godot;
using System;

public partial class MultiMeshTest : MultiMeshInstance3D
{
	[Export] public Mesh MeshResource;
	[Export] public int InstanceCount = 100;

	public override void _Ready()
	{
		if (MeshResource == null)
		{
			GD.PrintErr("MeshResource is not assigned!");
			return;
		}

		// Create and configure MultiMesh
		MultiMesh multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
		multiMesh.Mesh = MeshResource;
		multiMesh.InstanceCount = InstanceCount;

		// Assign MultiMesh to the MultiMeshInstance3D node using SetMultimesh
		SetMultimesh(multiMesh);

		// Set instance transforms
		for (int i = 0; i < InstanceCount; i++)
		{
			Transform3D transform = new Transform3D(Basis.Identity, new Vector3(i * 2, 0, 0)); // Spaced on X-axis
			multiMesh.SetInstanceTransform(i, transform);
		}
	}
}
