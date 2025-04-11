using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public class CpuVerticesGeneratorMultiThread : IVerticesGenerationStrategy
{
	private float _isoLevel = 0f;
    private float _scale = 1;
    private float[,,] _datapoints;
    private readonly List<Vector3> _vertices = new();
    private readonly object _lock = new();
    
    public List<Vector3> GenerateVertices(float[,,] datapoints, float isoLevel = 0f, float scale = 1)
    {
        _datapoints = datapoints;
        _isoLevel = isoLevel;
        _scale = scale;
        _vertices.Clear();
        
        int xSize = _datapoints.GetLength(0) - 1;
        int ySize = _datapoints.GetLength(1) - 1;
        int zSize = _datapoints.GetLength(2) - 1;

        Parallel.For(0, xSize, x =>
        {
            for (var y = 0; y < ySize; y++)
            {
                for (var z = 0; z < zSize; z++)
                {
                    MarchCube(x, y, z);
                }
            }
        });
        return _vertices;
    }
    private void MarchCube(int x, int y, int z)
    {
        var tri = GetTriangulation(x, y, z);
        for (int i = 0; i < tri.Length; i+= 3)
        {
            if (tri[i] == -1) break;
            var p1 = MarchingTable.EDGES[tri[i]];
            var p2 = MarchingTable.EDGES[tri[i + 1]];
            var p3 = MarchingTable.EDGES[tri[i + 2]];
            
            var position1 = EvalPosition(x, y, z, p1);
            var position2 = EvalPosition(x, y, z, p2);
            var position3 = EvalPosition(x, y, z, p3);
            
            // Lock the vertices list to ensure thread safety
            lock (_lock)
            {
                _vertices.Add(position1);
                _vertices.Add(position2);
                _vertices.Add(position3);
            }
        }

    }

    private Vector3 EvalPosition(int x, int y, int z, Vector2I pointsIndices)
    {
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
        return position;
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