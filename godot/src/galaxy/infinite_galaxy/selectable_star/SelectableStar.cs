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
            // Get the galaxy node, kind of ugly but works
            Node3D galaxy = GetNode<Node3D>("/root/InfiniteGalaxyTest/InfiniteGalaxy");
            InfiniteGalaxy infiniteGalaxy = (InfiniteGalaxy) galaxy;
            infiniteGalaxy.SelectedStarUI(this);
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
