using Godot;
using System;

public partial class DebugDraw : Node
{
	[Export] StandardMaterial3D sphereMaterial;

	public void DrawSphere(Vector3 position, float radius)
	{
		MeshInstance3D mesh = CreateSphere(position, radius, sphereMaterial);
		AddChild(mesh);
	}

	public void DrawSphere(Vector3 position, float radius, Color color)
	{
		sphereMaterial.AlbedoColor = color;
		MeshInstance3D mesh = CreateSphere(position, radius, sphereMaterial);
		AddChild(mesh);
	}

	private MeshInstance3D CreateSphere(Vector3 position, float radius, StandardMaterial3D material)
	{
		MeshInstance3D meshInstance = new MeshInstance3D();
		SphereMesh sphereMesh = new SphereMesh();

		sphereMesh.Radius = radius;
		sphereMesh.Height = radius * 2;

		meshInstance.Mesh = sphereMesh;
		meshInstance.MaterialOverride = sphereMaterial;
		meshInstance.Position = position;

		return meshInstance;
	}
}
