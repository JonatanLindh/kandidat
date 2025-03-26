using System.Collections.Generic;
using Godot;


/// <summary>
/// Generates vertices for the marching cubes algorithm on the CPU.
/// </summary>
public class CpuVerticesGenerator : IVerticesGenerationStrategy
{
    private float _isoLevel = 0f;
    private int _scale = 1;
    private float[,,] _datapoints;
    private readonly List<Vector3> _vertices = new();


    public List<Vector3> GenerateVertices(float[,,] datapoints, float isoLevel = 0f, int scale = 1)
    {
        _datapoints = datapoints;
        _isoLevel = isoLevel;
        _scale = scale;
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
        return _vertices;
    }
    private void MarchCube(int x, int y, int z)
    {
        var tri = GetTriangulation(x, y, z);
        foreach (var edgeIndex in tri)
        {
            if (edgeIndex == -1) break;
            var pointsIndices = MarchingTable.EDGES[edgeIndex];

            var p0 = MarchingTable.POINTS[pointsIndices.X];
            var p1 = MarchingTable.POINTS[pointsIndices.Y];
			
            var posA = new Vector3((x + p0.X) * _scale, (y + p0.Y) * _scale, (z + p0.Z) * _scale);
            var posB = new Vector3((x + p1.X) * _scale, (y + p1.Y) * _scale, (z + p1.Z) * _scale);
			
            // Get density values at each point
            var valueA = _datapoints[x + (int)p0.X, y + (int)p0.Y, z + (int)p0.Z];
            var valueB = _datapoints[x + (int)p1.X, y + (int)p1.Y, z + (int)p1.Z];
            
            // Interpolate position based on how close each density value is to the iso-level
            float t = (valueA == valueB) ? 0.5f : (_isoLevel - valueA) / (valueB - valueA);
            var position = posA + t * (posB - posA);
			
            _vertices.Add(position);
        }
    }
    
    private int[] GetTriangulation(int x, int y, int z)
    {
        var idx = 0b00000000;
        if (_datapoints[x, y, z] >= _isoLevel) idx |= 0b00000001;
        if (_datapoints[x, y, z + 1] >= _isoLevel) idx |= 0b00000010;
        if (_datapoints[x + 1, y, z + 1] >= _isoLevel) idx |= 0b00000100;
        if (_datapoints[x + 1, y, z] >= _isoLevel) idx |= 0b00001000;
		
        if (_datapoints[x, y + 1, z] >= _isoLevel) idx |= 0b00010000;
        if (_datapoints[x, y + 1, z + 1] >= _isoLevel) idx |= 0b00100000;
        if (_datapoints[x + 1, y + 1, z + 1] >= _isoLevel) idx |= 0b01000000;
        if (_datapoints[x + 1, y + 1, z] >= _isoLevel) idx |= 0b10000000;
		
        return MarchingTable.TRIANGULATIONS[idx];
    }

}