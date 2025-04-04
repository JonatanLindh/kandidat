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
	private float _maxHeight = float.MinValue;
	private float _minHeight = float.MaxValue;


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

		// Calculate the actual geometric center of the vertices
		var center = Vector3.Zero;
		if (vertices.Count > 0)
		{
			foreach (var vertex in vertices)
			{
				center += vertex;
			}
			center /= vertices.Count;
		}
		var surfaceTool = new SurfaceTool();
		surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

		//surfaceTool.SetSmoothGroup(UInt32.MaxValue);
		surfaceTool.SetSmoothGroup(0);

		foreach (var vertex in vertices)
		{
			// Center the mesh using the actual geometric center
			var newVertex = vertex - center;
			float height = newVertex.Length();
			if (height > _maxHeight) _maxHeight = height;
			if (height < _minHeight) _minHeight = height;
			surfaceTool.AddVertex(newVertex);
		}
		vertices.Clear();
		surfaceTool.GenerateNormals();
		surfaceTool.Index();
		Mesh mesh = surfaceTool.Commit();
		var meshInstance = new MeshInstance3D();
		meshInstance.Mesh = mesh;
		meshInstance.CreateMultipleConvexCollisions();

		// Load the shader correctly
		var shader = ResourceLoader.Load<Shader>("res://src/bodies/planet/planet_shader.gdshader");
		var shaderMaterial = new ShaderMaterial();
		shaderMaterial.Shader = shader;
		GD.Print("Max Height: ", _maxHeight);
		GD.Print("Min Height: ", _minHeight);
		shaderMaterial.SetShaderParameter("min_height", _minHeight);
		shaderMaterial.SetShaderParameter("max_height", _maxHeight);

		// Generate and assign a gradient texture
		Gradient gradient = new Gradient();
		gradient.AddPoint(0.0f, new Color(0.0f, 0.3f, 0.7f)); // Ocean
		gradient.AddPoint(0.2f, new Color(0.5f, 0.4f, 0.1f)); // Mid heights: brownish
		gradient.AddPoint(0.5f, new Color(0.81f, 0.44f, 0.65f)); // Low heights: dark green
		gradient.AddPoint(0.7f, new Color(0.86f, 0.63f, 0.47f)); // High heights: gray
		gradient.AddPoint(1.0f, new Color(1.0f, 1.0f, 1.0f)); // Peaks: white (snow)


		GradientTexture1D gradientTexture = new GradientTexture1D();
		gradientTexture.Gradient = gradient;
		gradientTexture.Width = 256 * 2;

		shaderMaterial.SetShaderParameter("height_color", gradientTexture);

		meshInstance.MaterialOverride = shaderMaterial;

		return meshInstance;
	}
}
