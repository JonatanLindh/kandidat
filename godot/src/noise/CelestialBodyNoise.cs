using Godot;
using System;

public interface CelestialBodyNoise
{
    ///<summary>
    /// Returns the 3d noise of a celestial body to be used with marching cubes
    ///</summary>
    public float[,,] GetNoise();
}
