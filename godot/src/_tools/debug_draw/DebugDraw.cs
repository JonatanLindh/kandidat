using Godot;
using System;

public partial class DebugDraw : Node
{
	[Export] StandardMaterial3D sphereMaterial;
	[Export] StandardMaterial3D lineMaterial;

	/// <summary>
	/// Clears all drawn debug objects.
	/// </summary>
	public void Clear()
	{
		foreach (Node child in GetChildren())
		{
			if(child is MeshInstance3D)
			{
				child.QueueFree();
			}
		}
	}

	/// <summary>
	/// Draws a sphere at the given position with the given radius.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="radius"></param>
	public void DrawSphere(Vector3 position, float radius)
	{
		MeshInstance3D mesh = CreateSphere(position, radius, sphereMaterial);
		AddChild(mesh);
	}

	/// <summary>
	/// Draws a sphere at the given position with the given radius.
	/// Overrides the material color.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="radius"></param>
	/// <param name="color"></param>
	public void DrawSphere(Vector3 position, float radius, Color color)
	{
		StandardMaterial3D mat = sphereMaterial.Duplicate() as StandardMaterial3D;
		mat.AlbedoColor = color;

		MeshInstance3D mesh = CreateSphere(position, radius, mat);
		AddChild(mesh);
	}

	private MeshInstance3D CreateSphere(Vector3 position, float radius, StandardMaterial3D material)
	{
		MeshInstance3D mesh = new MeshInstance3D();
		SphereMesh sphereMesh = new SphereMesh();

		sphereMesh.Radius = radius;
		sphereMesh.Height = radius * 2;

		mesh.Mesh = sphereMesh;
		mesh.MaterialOverride = material;
		mesh.Position = position;

		return mesh;
	}

	/// <summary>
	/// Draws a line from <c>from</c> to <c>to</c>.
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	public void DrawLine(Vector3 from, Vector3 to)
	{
		MeshInstance3D mesh = CreateLine(from, to, lineMaterial);
		AddChild(mesh);
	}

	/// <summary>
	/// Draws a line from <c>from</c> to <c>to</c>.
	/// Overrides the material color.
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="color"></param>
	public void DrawLine(Vector3 from, Vector3 to, Color color)
	{
		StandardMaterial3D mat = lineMaterial.Duplicate() as StandardMaterial3D;
		mat.AlbedoColor = color;

		MeshInstance3D mesh = CreateLine(from, to, mat);
		AddChild(mesh);
	}

	private MeshInstance3D CreateLine(Vector3 from, Vector3 to, StandardMaterial3D material)
	{
		MeshInstance3D mesh = new MeshInstance3D();
		ImmediateMesh immediateMesh = new ImmediateMesh();

		immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
		immediateMesh.SurfaceAddVertex(from);
		immediateMesh.SurfaceAddVertex(to);
		immediateMesh.SurfaceEnd();

		mesh.Mesh = immediateMesh;
		mesh.MaterialOverride = material;
		return mesh;
	}
}
