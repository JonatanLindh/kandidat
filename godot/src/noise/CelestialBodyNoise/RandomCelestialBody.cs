using Godot;
using System;
using static Godot.XmlParser;

public interface RandomCelestialBody
{
    public int GetRadius();
    public int GetSize();
    public int GetWidth();
    public int GetHeight();
    public int GetDepth();
    public int GetOctaves();
    public float GetFrequency();
    public float GetAmplitude();
    public float GetLacunarity();
    public float GetPersistence();
    public int GetSeed();
}
