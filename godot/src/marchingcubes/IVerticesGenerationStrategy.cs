using System.Collections.Generic;
using Godot;

/// <summary>
/// Interface for generating vertices with marching cube from a 3D grid of data points.
/// </summary>
public interface IVerticesGenerationStrategy
{
    List<Vector3> GenerateVertices(float[,,] datapoints, float isoLevel = 0f, int scale = 1);
}