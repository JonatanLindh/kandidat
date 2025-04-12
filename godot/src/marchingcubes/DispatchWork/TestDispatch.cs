using Godot;
using System;

[Tool]
public partial class TestDispatch : Node3D
{
	public override void _Ready()
	{

		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					// Temporary Mesh
					var mesh = new SphereMesh();
					mesh.Radius = 32;
					mesh.Height = 64;
					var meshInstance = new MeshInstance3D();
					meshInstance.Mesh = mesh;
					AddChild(meshInstance);
					meshInstance.GlobalPosition = new Vector3(64 * i, 64 * j, 64 * k);
					
					// Create a new MarchingCubeRequest
					MarchingCubeRequest request = new MarchingCubeRequest
					{
						DataPoints = GenerateDataPoints(),
						Scale = 1,
						Offset = new Vector3(64 * i, 64 * j, 64 * k),
						Root = this,
						TempNode = meshInstance
					};
					MarchingCubeDispatch.Instance.AddToQueue(request);
				}

			}
		}
		
	}
	
	
	private float[,,] GenerateDataPoints()
	{
		var radius = 32;
		int size = radius * 2 + 1;
		var dataPoints = new float[size, size, size];

		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				for (int z = 0; z < size; z++)
				{
					Vector3 worldPos = new Vector3(x, y, z);
					var value = -Sphere(worldPos, Vector3.One * radius, radius);
					value = Mathf.Clamp(value, -1.0f, 1.0f);
					dataPoints[x, y, z] = value;
				}
			}
		}
		return dataPoints;
	}
	
	private static float Sphere(Vector3 worldPos, Vector3 origin, float radius) {
		return (worldPos - origin).Length() - radius;
	}
}
