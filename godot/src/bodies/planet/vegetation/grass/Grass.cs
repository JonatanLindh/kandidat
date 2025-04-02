using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using Range = Godot.Range;

[Tool]
public partial class Grass : Node
{
    [ExportToolButton("Generate Grass")]
    private Callable ClickMeButton => Callable.From(SpawnGrass);
    [Export] public MeshInstance3D Mesh { get; set; }

    [Export]
    public Mesh GrassMesh
    {
        get => _grassMesh;
        set
        {
            _grassMesh = value;
            CallDeferred(nameof(SpawnGrass));
        }
    }

    [Export]
    public float Density
    {
        get => _density;
        set
        {
            _density = value;
            CallDeferred(nameof(SpawnGrass));
        }
    }

    [Export]
    public float TileSize
    {
        get => _tileSize;
        set
        {
            _tileSize = value;
            CallDeferred(nameof(SpawnGrass));
        }
    }
    
    [Export]
    public int MapRadius
    {
        get => _mapRadius;
        set
        {
            _mapRadius = value;
            CallDeferred(nameof(SpawnGrass));
        }
    }
    
    [Export]
    public Material GrassMaterial
    {
        get;
        set;
    }

    private Node3D _grassParent;
    private Mesh _grassMesh;
    private float _density = 1f;
    private float _tileSize = 5f;
    private int _mapRadius = 1;
    
    private readonly List<object[]> _grassInstances = [];

    public override void _Ready()
    {
        SpawnGrass();
        
    }
    private void SpawnGrass()
    {
        //_grassMaterial ??= GD.Load<ShaderMaterial>("res://src/bodies/planet/vegetation/grass/assets/grass.gdshader");
        _grassMesh ??= GD.Load<Mesh>("res://src/bodies/planet/vegetation/grass/assets/grass_high.obj");

        
        _grassParent?.QueueFree();
        _grassInstances.Clear();
        _grassParent = new Node3D();
        
        AddChild(_grassParent);
        SetupGrassInstances();
        GenerateGrassMultiMeshes();
    }
    
    private void SetupGrassInstances()
    {

        for (float i = -_mapRadius; i <= _mapRadius; i += _tileSize)
        {
            for (float j = -_mapRadius; j <= _mapRadius; j += _tileSize)
            {
                var grassInstance = GetNodeOrNull<MultiMeshInstance3D>($"GrassInstance_{i}_{j}");
                if (grassInstance != null) RemoveChild(grassInstance);
                var instance = new MultiMeshInstance3D();
                instance.CastShadow = GeometryInstance3D.ShadowCastingSetting.On;
                instance.Position = new Vector3(i, 0f, j);
                instance.Name = $"GrassInstance_{i}_{j}";
                _grassParent.AddChild(instance);
                _grassInstances.Add([instance, instance.Position]);
            }
        }



        /*
        if (_grassInstance != null) RemoveChild(_grassInstance);
        var instance = new MultiMeshInstance3D();
        instance.Position = new Vector3(0f, 0f, 0f);
        AddChild(instance);
        _grassInstance = instance;
        */

    }

    private void GenerateGrassMultiMeshes()
    {
        // Create a new material and disable backface culling
        StandardMaterial3D material = new StandardMaterial3D(); 
        //material.SetCullMode(BaseMaterial3D.CullModeEnum.Disabled);
        //material.SetFlag(BaseMaterial3D.Flags.Do, true); 
        
        foreach (var grassInstance in _grassInstances)
        {
            var instance = (MultiMeshInstance3D) grassInstance[0];
            instance.Multimesh = CreateMeshMultiMesh(_density, _grassMesh);
            if(GrassMaterial != null)
                instance.MaterialOverride = GrassMaterial;
        }
    }

    private MultiMesh CreateMeshMultiMesh(float density, Mesh mesh)
    {
        var rowSize = (int)Math.Ceiling(_tileSize * Mathf.Lerp(0.0f, 10.0f, density));
        var multiMesh = new MultiMesh();
        multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multiMesh.Mesh = mesh;
        multiMesh.InstanceCount = rowSize * rowSize;

        var jitterOffset = _tileSize / (float)rowSize * 0.5 * 0.9;
        for (int i = 0; i < rowSize; i++)
        {
            for(int j = 0; j < rowSize; j++)
            {
                var grassPosition = new Vector3(
                    (float)((float)i / rowSize - 0.5), 
                    0, 
                    (float)((float)j / rowSize - 0.5)
                    ) * _tileSize;
                
                var grassOffset = new Vector3(
                    (float)GD.RandRange(-jitterOffset, jitterOffset),
                    0,
                    (float)GD.RandRange(-jitterOffset, jitterOffset)
                    );
                    
                multiMesh.SetInstanceTransform(i + j * rowSize, 
                    new Transform3D(Basis.Identity, grassPosition + grassOffset));
            }
        }
        return multiMesh;
    }
    
    
}
