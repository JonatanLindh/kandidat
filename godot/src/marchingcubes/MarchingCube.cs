using Godot;
using System;
using System.Collections.Generic;
using Godot.NativeInterop;

// TODO 
// Add multithreading to the mesh generation

/// <summary>
/// The MarchingCube class generates a 3D mesh from a scalar field represented by a 3D array of float values.
/// </summary>
public class MarchingCube
{

    private float[,,] _datapoints;
    private readonly int _scale;
    private readonly float _threshold;
    private readonly List<Vector3> _vertices;


    /// <summary>
    /// Initializes a new instance of the MarchingCube class with the specified scale and threshold.
    /// </summary>
    /// <param name="scale">The scale factor for the mesh generation.</param>
    /// <param name="threshold">The threshold value for determining the surface of the mesh.</param>
    public MarchingCube(int scale = 1, float threshold = 0.0f)
    {
        _scale = scale;
        _threshold = threshold;
        _vertices = new List<Vector3>();
    }

    /// <summary>
    /// Generates a mesh from a 3D array of float values with the Marching Cubes Algorithm
    /// </summary>
    /// <param name="datapoints">3D array of float values representing the scalar field</param>
    /// <returns>A MeshInstance3D object representing the generated mesh</returns>
    public MeshInstance3D GenerateMesh(float[,,] datapoints)
    {
        _datapoints = datapoints;
        _vertices.Clear();
        for (var x = 0; x < _datapoints.GetLength(0) - 1; x++)
        {
            for (var y = 0; y < _datapoints.GetLength(1) - 1; y++)
            {
                for (var z = 0; z < _datapoints.GetLength(2) - 1; z++)
                {
                    MarchCube(x, y, z);
                }
            }
        }
        var surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        surfaceTool.SetSmoothGroup(UInt32.MaxValue);
        foreach (var vertex in _vertices)
        {
            surfaceTool.AddVertex(vertex);
        }
        surfaceTool.GenerateNormals();
        surfaceTool.Index();
        Mesh mesh = surfaceTool.Commit();
        
        var meshInstance = new MeshInstance3D();
        meshInstance.Mesh = mesh;
        return meshInstance;
    }
    private void MarchCube(int x, int y, int z)
    {
        var tri = GetTriangulation(x, y, z);
        foreach (var edgeIndex in tri)
        {
            if (edgeIndex == -1) break;
            var pointsIndices = MarchingTable.EDGES[edgeIndex];

            Vector3I p0 = MarchingTable.POINTS[pointsIndices.X];
            Vector3I p1 = MarchingTable.POINTS[pointsIndices.Y];

            Vector3I posA = new Vector3I((x + p0.X) * _scale, (y + p0.Y) * _scale, (z + p0.Z) * _scale);
            Vector3I posB = new Vector3I((x + p1.X) * _scale, (y + p1.Y) * _scale, (z + p1.Z) * _scale);
            Vector3 A = posA;
            Vector3 B = posB;
            float valueA = _datapoints[posA.X, posA.Y, posA.Z];
            float valueB = _datapoints[posB.X, posB.Y, posB.Z];

            // TODO: use an actual interpolation function between the two points
            float t = (0 - valueA) / (valueB - valueA);
            var position = A + t * (B - A);

            _vertices.Add(position);
        }
        
    }
    
    private int[] GetTriangulation(int x, int y, int z)
    {
        var idx = 0b00000000;
        if (_datapoints[x, y, z] >= _threshold) idx |= 0b00000001;
        if (_datapoints[x, y, z + 1] >= _threshold) idx |= 0b00000010;
        if (_datapoints[x + 1, y, z + 1] >= _threshold) idx |= 0b00000100;
        if (_datapoints[x + 1, y, z] >= _threshold) idx |= 0b00001000;
		
        if (_datapoints[x, y + 1, z] >= _threshold) idx |= 0b00010000;
        if (_datapoints[x, y + 1, z + 1] >= _threshold) idx |= 0b00100000;
        if (_datapoints[x + 1, y + 1, z + 1] >= _threshold) idx |= 0b01000000;
        if (_datapoints[x + 1, y + 1, z] >= _threshold) idx |= 0b10000000;
		
        return MarchingTable.TRIANGULATIONS[idx];
    }
    
    
}
