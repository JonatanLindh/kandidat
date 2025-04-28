using System;
using Godot;

/// <summary>
/// Represents a request for generating a Marching Cube mesh, containing data points,
/// scaling, offset, and references to nodes for mesh generation and scene management.
/// </summary>
public record MarchingCubeRequest
{
	public float[,,] DataPoints { get; init; }
	public CelestialBodyNoise PlanetDataPoints { get; init; }
	public float Scale { get; init; }
	public Vector3 Offset { get; init; }
	public Vector3 Center { get; init; }
	public Node Root { get; init; }
	public Node TempNode { get; init; }
	
	public MeshInstance3D CustomMeshInstance { get; init; }
	
	public Func<float, float, ShaderMaterial> GeneratePlanetShader { get; init; }

	public bool IsProcessing { get; set; } = false;
	
	public Guid Id { get; init; } = Guid.NewGuid();
}