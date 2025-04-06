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
		// PINK
		// gradient.AddPoint(0.0f, new Color(0.0f, 0.3f, 0.7f));
		// gradient.AddPoint(0.2f, new Color(0.5f, 0.4f, 0.1f));
		// gradient.AddPoint(0.5f, new Color(0.81f, 0.44f, 0.65f));
		// gradient.AddPoint(0.7f, new Color(0.86f, 0.63f, 0.47f));
		// gradient.AddPoint(1.0f, new Color(1.0f, 1.0f, 1.0f));

		// // Earth like
		// gradient.AddPoint(0.0f, new Color(0.0f, 0.3f, 0.7f)); // Ocean
		// gradient.AddPoint(0.2f, new Color(0.439f, 0.255f, 0.0f)); // Mid heights: brownish
		// gradient.AddPoint(0.5f, new Color(0.035f, 0.31f, 0.0f)); // Low heights: dark green
		// gradient.AddPoint(0.7f, new Color(0.3f, 0.3f, 0.3f)); // High heights: gray
		// gradient.AddPoint(1.0f, new Color(1.0f, 1.0f, 1.0f)); // Peaks: white (snow)

		// // // ALIEN
		// gradient.AddPoint(0.0f, new Color(0.02f, 0.0f, 0.1f));       // Deep void-like ocean
		// gradient.AddPoint(0.2f, new Color(0.1f, 0.0f, 0.2f));        // Purple lowlands
		// gradient.AddPoint(0.5f, new Color(0.0f, 0.8f, 0.6f));        // Glowing teal flora
		// gradient.AddPoint(0.7f, new Color(0.3f, 0.1f, 0.5f));        // Magenta cliffs
		// gradient.AddPoint(1.0f, new Color(0.9f, 1.0f, 1.0f));        // Icy glowing peaks

		// MARS
		gradient.AddPoint(0.0f, new Color(0.35f, 0.2f, 0.1f));       // Dark sand
		gradient.AddPoint(0.2f, new Color(0.7f, 0.3f, 0.1f));        // Reddish terrain
		gradient.AddPoint(0.5f, new Color(0.85f, 0.6f, 0.3f));       // Orange dunes
		gradient.AddPoint(0.7f, new Color(0.95f, 0.8f, 0.5f));       // Pale sandstone cliffs
		gradient.AddPoint(1.0f, new Color(1.0f, 1.0f, 0.9f));        // Bleached peaks

		// // ICE WORLD
		// gradient.AddPoint(0.0f, new Color(0.0f, 0.2f, 0.4f));        // Deep ice ocean
		// gradient.AddPoint(0.2f, new Color(0.2f, 0.4f, 0.7f));        // Slushy water
		// gradient.AddPoint(0.5f, new Color(0.6f, 0.8f, 0.9f));        // Ice crust
		// gradient.AddPoint(0.7f, new Color(0.8f, 0.9f, 1.0f));        // Snowy cliffs
		// gradient.AddPoint(1.0f, new Color(1.0f, 1.0f, 1.0f));        // Frozen peaks

		// // VOLCANIC WORLD (MARS LIKE)
		// gradient.AddPoint(0.0f, new Color(0.05f, 0.0f, 0.0f));       // Basalt base
		// gradient.AddPoint(0.2f, new Color(0.3f, 0.1f, 0.0f));        // Scorched rock
		// gradient.AddPoint(0.5f, new Color(0.6f, 0.1f, 0.0f));        // Lava flows
		// gradient.AddPoint(0.7f, new Color(0.8f, 0.4f, 0.1f));        // Glowing terrain
		// gradient.AddPoint(1.0f, new Color(1.0f, 0.9f, 0.7f));        // Molten peaks

		GradientTexture1D gradientTexture = new GradientTexture1D();
		gradientTexture.Gradient = gradient;
		gradientTexture.Width = 256 * 2;

		shaderMaterial.SetShaderParameter("height_color", gradientTexture);
		shaderMaterial.SetShaderParameter("cliff_color", new Vector3(0.5f, 0.5f, 0.5f));

		meshInstance.MaterialOverride = shaderMaterial;

		return meshInstance;
	}
}
