using Godot;
using System;

/// <summary>
/// Interface should be implemented by noise which is to be modified outside of the editor
/// </summary>
public interface ModifiableCelestialBody
{
    /// <summary>
    /// Sets the radius of the celestial body
    /// </summary>
    /// <param name="radius"></param>
    void SetRadius(int radius);

    /// <summary>
    /// Sets the seed of the celestial body
    /// </summary>
    /// <param name="seed"></param>
    void SetSeed(int seed);
}
