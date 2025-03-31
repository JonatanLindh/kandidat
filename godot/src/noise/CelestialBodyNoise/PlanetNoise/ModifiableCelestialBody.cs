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
    /// Sets the width of the celestial body
    /// </summary>
    /// <param name="width"></param>
    void SetWidth(int width);

    /// <summary>
    /// Sets the height of the celestial body
    /// </summary>
    /// <param name="height"></param>
    void SetHeight(int height);

    /// <summary>
    /// Sets the depth of the celestial body
    /// </summary>
    /// <param name="depth"></param>
    void SetDepth(int depth);

    /// <summary>
    /// Sets the size if the celestial body, i.e. makes the width, height and depth the same
    /// </summary>
    /// <param name="size"></param>
    void SetSize(int size);

    /// <summary>
    /// Sets the amount of octaves to be used in Fbm
    /// </summary>
    /// <param name="octaves"></param>
    void SetOctaves(int octaves);

    /// <summary>
    /// Sets the seed to use when generating the planet
    /// </summary>
    /// <param name="seed"></param>
    void SetSeed(int seed);

    /// <summary>
    /// Sets the amplitude to be used in Fbm
    /// </summary>
    /// <param name="amplitude"></param>
    void SetAmplitude(float amplitude);

    /// <summary>
    /// Sets the frequency to be used in Fbm
    /// </summary>
    /// <param name="frequency"></param>
    void SetFrequency(float frequency);

    /// <summary>
    /// Sets the lacunarity to be used in Fbm, the amount to multiply the frequency with each octave
    /// </summary>
    /// <param name="lacunarity"></param>
    void SetLacunarity(float lacunarity);

    /// <summary>
    /// Sets the persistence to be used in Fbm, the amount to multiply the amplitude with each octave
    /// </summary>
    /// <param name="persistence"></param>
    void SetPersistence(float persistence);
}
