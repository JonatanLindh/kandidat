using Godot;
using System;

/// <summary>
/// Interface should be implemented by noise which is to be used for celestial bodies
/// </summary>
public interface CelestialBodyNoise
{
	///<summary>
	/// Returns the 3d noise of a celestial body to be used with marching cubes
	///</summary>
	public float[,,] GetNoise();
}
