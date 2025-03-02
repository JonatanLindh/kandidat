using Godot;
using System;
using System.Transactions;

public partial class CelestialBodyCreator
{

    CelestialBodyNoise planetNoise;

    public CelestialBodyCreator()
    {
       // planetNoise = new PlanetNoise();
        // moonNoise = new MoonNoise();
        // sunNoise = new SunNoise(); etc..
    }

    public float[,,] CreateMoon(int radius)
    {
        // moonNoise.GetNoise() etc ...
        throw new NotImplementedException();
    }

    public float[,,] CreatePlanet(int radius)
    {
        return planetNoise.GetNoise();
    }
}
