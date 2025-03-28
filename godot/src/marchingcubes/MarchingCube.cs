using Godot;
using System;



/// <summary>
/// The MarchingCube class generates a 3D mesh from a scalar field represented by a 3D array of float values.
/// </summary>
public class MarchingCube
{
	private IVerticesGenerationStrategy _strategy;
	private float[,,] _datapoints;
	private readonly int _scale;
	private readonly float _threshold;

	
	// Enum for strategy selection
	public enum GenerationMethod
	{
		Cpu,
		Gpu
	}
	

	/// <summary>
	/// Initializes a new instance of the MarchingCube class with the specified scale and threshold.
	/// </summary>
	/// <param name="scale">The scale factor for the mesh generation.</param>
	/// <param name="method">The method used for generating the vertices, either on the cpu or on the gpu</param>
	/// <param name="threshold">The threshold value for determining the surface of the mesh.</param>
	public MarchingCube(int scale = 1, float threshold = 0.1f, GenerationMethod method = GenerationMethod.Cpu)
	{
		_scale = scale;
		_threshold = threshold;
		_strategy = method switch
		{
			GenerationMethod.Cpu => new CpuVerticesGenerator(),
			GenerationMethod.Gpu => new GpuVerticesGenerator(),
			_ => new GpuVerticesGenerator()
		};
	}

	~MarchingCube()
	{
		_strategy = null;
		_datapoints = null;
	}

	/// <summary>
	/// Generates a mesh from a 3D array of float values with the Marching Cubes Algorithm
	/// </summary>
	/// <param name="datapoints">3D array of float values representing the scalar field</param>
	/// <returns>A MeshInstance3D object representing the generated mesh</returns>
	public MeshInstance3D GenerateMesh(float[,,] datapoints)
	{
		var vertices = _strategy.GenerateVertices(datapoints, _threshold, _scale);

		var surfaceTool = new SurfaceTool();
		surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
		
		//surfaceTool.SetSmoothGroup(UInt32.MaxValue);
		surfaceTool.SetSmoothGroup(0);
		
		foreach (var vertex in vertices)
		{
			surfaceTool.AddVertex(vertex);
		}
		vertices.Clear();
		surfaceTool.GenerateNormals();
		surfaceTool.Index();
		Mesh mesh = surfaceTool.Commit();
		var meshInstance = new MeshInstance3D();
		meshInstance.Mesh = mesh;
		return meshInstance;
	}
}
