using Godot;
using System;

/// <summary>
/// A class used for spawning ocean onto a celestial body
/// </summary>
[Tool]
public partial class OceanSpawner : Node3D
{
    private Node3D instantiateOcean(PackedScene oceanScene)
    {
        return oceanScene.Instantiate() as Node3D;
    }

    private void resizeOcean(Node3D ocean, int radius, Vector3 scale)
    {
        ocean.Set("radius", radius);
        ocean.Scale = scale;
    }

    private bool isOceanSpawnable(double warmth)
    {
        return warmth >= 0.4d && warmth <= 0.6d;
    }

    /// <summary>
    /// Instantiates the oceanScene and configures it to fit the given parameters
    /// </summary>
    /// <param name="oceanScene"></param>
    /// <param name="radius"></param>
    /// <param name="scale"></param>
    /// <param name="warmth"></param>
    /// <returns>null if warmth is too hot or cold, otherwise, it returns the instantiated and configured ocean as a Node3D</returns>
    public Node3D GenerateOcean(PackedScene oceanScene, int radius, Vector3 scale, double warmth)
    {
        if (isOceanSpawnable(warmth) == false) return null;
        Node3D ocean = instantiateOcean(oceanScene);
        resizeOcean(ocean, radius, scale);
        
        return ocean;
    }

    /// <summary>
    /// Instantiates the oceanScene and configures it to fit the given parameters, including the colors
    /// </summary>
    /// <param name="oceanScene"></param>
    /// <param name="radius"></param>
    /// <param name="scale"></param>
    /// <param name="warmth"></param>
    /// <param name="deepColor"></param>
    /// <param name="shallowColor"></param>
    /// <param name="foamColor"></param>
    /// <returns>null if warmth is too hot or cold, otherwise, it returns the instantiated and configured ocean as a Node3D</returns>
    public Node3D GenerateOcean(PackedScene oceanScene, int radius, Vector3 scale, double warmth, Color deepColor, Color shallowColor, Color foamColor)
    {
        Node3D ocean = GenerateOcean(oceanScene, radius, scale, warmth);
        if (ocean == null) return null;

        ocean.Call("set_colors", deepColor, shallowColor, foamColor);

        return ocean;
    }

}
