using Godot;
using System;

public partial class SelectableStar : Node3D
{
    [Export] PackedScene solarSystemScene;
    int seed;

    public void OnArea3dInputEvent(Node cam, InputEvent input, Vector3 eventPos, Vector3 normal, int shapeIdx)
    {
        if(input is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            LoadSolarSystem();
        }
    }

    private void LoadSolarSystem()
    {
        GetTree().ChangeSceneToPacked(solarSystemScene);

        /*
        SolarSystem solarSystem = (SolarSystem) solarSystemScene.Instantiate();
        solarSystem.SetSeed(seed);
        solarSystem.Generate():

        GetTree().Root.AddChild(solarSystem);
        GetTree().Root.RemoveChild(GetTree().CurrentScene);

        GetTree().CurrentScene = solarSystem;
        */
    }

    public void SetSeed(int seed)
    {
        this.seed = seed;
    }
}
