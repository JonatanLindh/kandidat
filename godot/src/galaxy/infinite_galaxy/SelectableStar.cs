using Godot;
using System;

public partial class SelectableStar : Node3D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnArea3dInputEvent(Node cam, InputEvent input, Vector3 eventPos, Vector3 normal, int shapeIdx)
    {
        if(input is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            GD.Print("Clicked on star " + this.Position);
        }
    }
}
