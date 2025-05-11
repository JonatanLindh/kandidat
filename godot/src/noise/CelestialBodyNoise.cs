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

    ///<summary>
    /// Returns the 3d noise of a celestial body to be used with marching cubes
    /// <param name="offset">A Vector3 representing the positional offset</param>
    ///</summary>
    public float[,,] GetNoise(Vector3 offset);

    /// <summary>
    /// Get the current radius of the celestial body
    /// </summary>
    /// <returns>The radius</returns>
    int GetRadius();


    
    int Resolution { get; set; }

    float VoxelSize { get; set; }
    
    int Seed { get; set; }
    
}
