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

        foreach (Vector3 pos in n.CreateSphere())
        {
            Display(pos);
        }
    }

    // Add a debug sphere at global location.

    private void Display(Vector3 position)
    {
        int size = 1;
        //Node3D displayNode = GetNode<Node3D>("PlanetNoise/DisplayNoise");

        MeshInstance3D node = new MeshInstance3D();
        SphereMesh sphere = new SphereMesh();
        var material = new OrmMaterial3D();

        node.Mesh = sphere;
        node.Translate(new Vector3(position.X, position.Y, position.Z));
        node.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
       // node.Position = position;

        //sphere.RadialSegments = 8;
        //sphere.Rings = 8;
        sphere.Radius = 0.1f;
        sphere.Height = 0.1f * 2.0f;
        sphere.Material = material;

        material.ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel;
        material.AlbedoColor = Colors.YellowGreen;

        this.AddChild(node);
    }
}
