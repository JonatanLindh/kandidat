using Godot;
using System.Collections.Generic;

[Tool]
public partial class DisplayNoise : Node
{
    private bool _reload;
    [Export]
    public bool reload
    {
        get => _reload;
        set
        {
            _reload = !value;
            UpdateNoise();
        }
    }

    List<Vector3> noise = new List<Vector3>();

    public override void _Ready()
    {
        Display(new Vector3(0,0,0));
    }

    private void UpdateNoise()
    {
        GD.Print("Updated noise");
        PlanetNoise n = new PlanetNoise();
        GD.Print(n);

        foreach (Vector3 pos in n.GetNoise())
        {
            Display(pos);
        }
    }

    // Add a debug sphere at global location.

    private void Display(Vector3 position)
    {
        int size = 1;
        //Node3D displayNode = GetNode<Node3D>("PlanetNoise/DisplayNoise");

        SphereMesh sphere = new SphereMesh();
        sphere.RadialSegments = 8;
        sphere.Rings = 8;
        sphere.Radius = size;
        sphere.Height = size * 2;


        MeshInstance3D node = new MeshInstance3D();

        node.Mesh = sphere;
        node.Translate(new Vector3(position.X, position.Y, position.Z));

        node.Name = "noise";

        this.AddChild(node);
    }
}
