using Godot;
using System;
using System.Collections.Generic;
using Godot.NativeInterop;

public class MarchingCube
{
    private float[,,] _voxels;
    private int _scale;
    private float threshold;
    private List<Vector3> _vertices;

    public MarchingCube(float[,,] voxels, int scale = 1, float threshold = 0.0f)
    {
        _voxels = voxels;
        _scale = scale;
        this.threshold = threshold;
        _vertices = new List<Vector3>();
    }

    public MeshInstance3D GenerateMesh()
    {
        for (int x = 0; x < _voxels.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < _voxels.GetLength(1) - 1; y++)
            {
                for (int z = 0; z < _voxels.GetLength(2) - 1; z++)
                {
                    MarchCube(x, y, z);
                }
            }
        }
        var surface_tool = new SurfaceTool();
        surface_tool.Begin(Mesh.PrimitiveType.Triangles);
        surface_tool.SetSmoothGroup(UInt32.MaxValue);
        foreach (var vertex in _vertices)
        {
            surface_tool.AddVertex(vertex);
        }
        surface_tool.GenerateNormals();
        surface_tool.Index();
        Mesh mesh = surface_tool.Commit();
        
        MeshInstance3D mesh_instance = new MeshInstance3D();
        mesh_instance.Mesh = mesh;
        return mesh_instance;
    }
    private void MarchCube(int x, int y, int z)
    {
        var tri = GetTriangulation(x, y, z);
        foreach (var edge_index in tri)
        {
            if (edge_index == -1) break;
            var points_indices = MarchingTable.EDGES[edge_index];

            var p0 = MarchingTable.POINTS[points_indices.X];
            var p1 = MarchingTable.POINTS[points_indices.Y];
			
            var pos_a = new Vector3((x + p0.X) * _scale, (y + p0.Y) * _scale, (z + p0.Z) * _scale);
            var pos_b = new Vector3((x + p1.X) * _scale, (y + p1.Y) * _scale, (z + p1.Z) * _scale);
			
            // TODO: use an actual interpolation function between the two points
            var position = (pos_a + pos_b) / 2.0f;
			
            _vertices.Add(position);
        }
        
    }
    
    private int[] GetTriangulation(int x, int y, int z)
    {
        var idx = 0b00000000;
        if (_voxels[x, y, z] >= threshold) idx |= 0b00000001;
        if (_voxels[x, y, z + 1] >= threshold) idx |= 0b00000010;
        if (_voxels[x + 1, y, z + 1] >= threshold) idx |= 0b00000100;
        if (_voxels[x + 1, y, z] >= threshold) idx |= 0b00001000;
		
        if (_voxels[x, y + 1, z] >= threshold) idx |= 0b00010000;
        if (_voxels[x, y + 1, z + 1] >= threshold) idx |= 0b00100000;
        if (_voxels[x + 1, y + 1, z + 1] >= threshold) idx |= 0b01000000;
        if (_voxels[x + 1, y + 1, z] >= threshold) idx |= 0b10000000;
		
        return MarchingTable.TRIANGULATIONS[idx];
    }
    
    
}
