using System.Collections.Generic;
using Godot;

namespace Kandidat.marchingcubes;

public interface IVerticesGenerationStrategy
{
    List<Vector3> GenerateVertices(float[,,] datapoints, float isoLevel = 0f, int scale = 1);
}