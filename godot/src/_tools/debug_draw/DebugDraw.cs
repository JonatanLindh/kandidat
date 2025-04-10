using Godot;
using System;

public partial class DebugDraw : Node
{
	[Export] StandardMaterial3D sphereMaterial;
	[Export] StandardMaterial3D lineMaterial;

	public void Clear()
	{
		foreach (Node child in GetChildren())
		{
			child.QueueFree();
		}
	}

	public void DrawSphere(Vector3 position, float radius)
	{
		MeshInstance3D mesh = new MeshInstance3D();
		SphereMesh sphereMesh = new SphereMesh();

		sphereMesh.Radius = radius;
		sphereMesh.Height = radius * 2;

		mesh.Mesh = sphereMesh;
		mesh.MaterialOverride = sphereMaterial;
		mesh.Position = position;

		AddChild(mesh);
	}

	public void DrawLine(Vector3 from, Vector3 to)
	{
		MeshInstance3D mesh = new MeshInstance3D();
		ImmediateMesh immediateMesh = new ImmediateMesh();

		immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
		immediateMesh.SurfaceAddVertex(from);
		immediateMesh.SurfaceAddVertex(to);
		immediateMesh.SurfaceEnd();

		mesh.Mesh = immediateMesh;
		mesh.MaterialOverride = lineMaterial;

		AddChild(mesh);
	}
}
