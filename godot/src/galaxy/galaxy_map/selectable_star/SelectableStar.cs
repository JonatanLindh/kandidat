using Godot;
using System;

public partial class SelectableStar : Node3D
{
    [Export] PackedScene solarSystemScene;
    int seed;

    public void OnArea3dInputEvent(Node cam, InputEvent input, Vector3 eventPos, Vector3 normal, int shapeIdx)
    {
        if (input is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            // GalaxyMap is part of the group "GalaxyMap", so we can call the SelectedStarUI method of it
            // Kind of meh, but for now it works
            GetTree().CallGroup("GalaxyMap", "SelectedStarUI", this);
        }
    }

    public void LoadSolarSystem()
    {
        GetTree().ChangeSceneToPacked(solarSystemScene);

        /*
        SolarSystem solarSystem = (SolarSystem) solarSystemScene.Instantiate();
        solarSystem.SetSeed(seed);
        solarSystem.SetNoiseOffset(new Vector3(GlobalPosition.X, GlobalPosition.Y, GlobalPosition.Z));
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
