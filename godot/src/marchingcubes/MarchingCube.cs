using Godot;
using System;



/// <summary>
/// The MarchingCube class generates a 3D mesh from a scalar field represented by a 3D array of float values.
/// </summary>
public class MarchingCube
{
	private IVerticesGenerationStrategy _strategy;
	private float[,,] _datapoints;
	private float _scale;
	private readonly float _threshold;
	
	private float _maxHeight = float.MinValue;
	private float _minHeight = float.MaxValue;
	public float MaxHeight => _maxHeight;
	public float MinHeight => _minHeight;


    // Enum for strategy selection
    public enum GenerationMethod
    {
        Cpu,
        CpuMultiThread,
        Gpu
    }


    /// <summary>
    /// Initializes a new instance of the MarchingCube class with the specified scale and threshold.
    /// </summary>
    /// <param name="scale">The scale factor for the mesh generation.</param>
    /// <param name="method">The method used for generating the vertices, either on the cpu or on the gpu</param>
    /// <param name="threshold">The threshold value for determining the surface of the mesh.</param>
    public MarchingCube(float scale = 1, float threshold = 0.1f, GenerationMethod method = GenerationMethod.Cpu)
    {
        _scale = scale;
        _threshold = threshold;
        _strategy = method switch
        {
            GenerationMethod.Cpu => new CpuVerticesGenerator(),
            GenerationMethod.Gpu => new GpuVerticesGenerator(),
            GenerationMethod.CpuMultiThread => new CpuVerticesGeneratorMultiThread(),
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
    /// <param name="scale">The scale factor to adjust the size of the generated mesh.</param>
    /// <returns>A MeshInstance3D object representing the generated mesh</returns>
    public Mesh GenerateMesh(float[,,] datapoints, float scale = 1)
    {
        _scale = scale;
        
        var vertices = _strategy.GenerateVertices(datapoints, _threshold, _scale);
        if (vertices.Count == 0) return new Mesh();
        
        // Calculate the actual geometric center of the vertices
        var center = Vector3.Zero;
        foreach (var vertex in vertices)
        {
            center += vertex;
        }
        center /= vertices.Count;
        
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
		return mesh;
	}
}