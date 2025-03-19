using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using Array = Godot.Collections.Array;

[Tool]
public partial class McSpawnerCS : Node
{
	private float _isoLevel = 0.5f;
	private int _size = 2;
	private int _scale = 1;
	private List<Vector3> _vertices;
	
	// Shader Variables
	private RenderingDevice _renderingDevice;
	private Rid _shaderRid;
	private Rid _pipeline;
	private Rid _uniformSet;
	private Rid _triangleBuffer;
	private Rid _dataPointsBuffer;
	private Rid _counterBuffer;
	private Rid _paramBuffer;
	
	// Data received from Compute Shader
	private float[] _triangles;
	private int _triangleCount;


	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_vertices = new List<Vector3>();
		
		InitGpu();
		RunCompute();
		ProcessComputeData();
		CreateMesh();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Notification(int what)
	{
		if (what == NotificationPredelete)
		{
			CleanupGpu();
		}
	}

	private void RunCompute()
	{
		var computeList = _renderingDevice.ComputeListBegin();
		_renderingDevice.ComputeListBindComputePipeline(computeList, _pipeline);
		_renderingDevice.ComputeListBindUniformSet(computeList, _uniformSet, 0);

		uint groupsNeeded = (uint)Math.Ceiling(_size / 8.0f);
		groupsNeeded = 1;
		_renderingDevice.ComputeListDispatch(computeList, 
			xGroups: groupsNeeded, 
			yGroups: groupsNeeded, 
			zGroups: groupsNeeded);
		_renderingDevice.ComputeListEnd();

		_renderingDevice.Submit();
	}
	
	private void InitGpu()
	{
		// Expensive operation, only do it once and cache the result

		_renderingDevice = RenderingServer.CreateLocalRenderingDevice();

		if (_renderingDevice == null)
		{
			OS.Alert("Couldn't create rendering device");
			return;
		}

		_shaderRid = LoadShader(_renderingDevice, "res://src/marchingcubes/ComputeShader/MarchingCube.GLSL");
		SetupBuffers();
	}
	
	private void CleanupGpu()
	{
		if (_renderingDevice == null) return;
		
		_renderingDevice.FreeRid(_pipeline);
		_pipeline = new Rid();
		
		_renderingDevice.FreeRid(_uniformSet);
		_uniformSet = new Rid();

		_renderingDevice.FreeRid(_shaderRid);
		_shaderRid = new Rid();

		_renderingDevice.FreeRid(_triangleBuffer);
		_triangleBuffer = new Rid();

		_renderingDevice.FreeRid(_dataPointsBuffer);
		_dataPointsBuffer = new Rid();

		_renderingDevice.FreeRid(_counterBuffer);
		_counterBuffer = new Rid();

		_renderingDevice.FreeRid(_paramBuffer);
		_paramBuffer = new Rid();
		
		_renderingDevice.Free();
		_renderingDevice = null;
	}

	private void ProcessComputeData()
	{
		_renderingDevice.Sync();
		
		// Get Output Data
		var counterData = _renderingDevice.BufferGetData(_counterBuffer);
		_triangleCount = BitConverter.ToInt32(counterData, 0);
		GD.Print("Counter: ", _triangleCount);
		
		var triangleData = _renderingDevice.BufferGetData(_triangleBuffer);
		_triangles = new float[triangleData.Length / sizeof(float)];
		Buffer.BlockCopy(triangleData, 0, _triangles, 0, triangleData.Length);
		/*
		for (int i = 0; i < 6 * 4; i += 4)
		{
			GD.Print("Vertex ", i / 4, ": (", _triangles[i], ", ", 
				_triangles[i + 1], ", ", _triangles[i + 2], ")");
		}
		*/
		/*
		// Print Triangles
		for (int i = 0; i < _triangleCount; i++)
		{
			var triIndex = i * 12;
			GD.Print("Triangle ", i, ": (", _triangles[triIndex], ", ", 
				_triangles[triIndex + 1], ", ", _triangles[triIndex + 2], "), (",
				_triangles[triIndex + 4], ", ", _triangles[triIndex + 5], ", ", _triangles[triIndex + 6], "), (",
				_triangles[triIndex + 8], ", ", _triangles[triIndex + 9], ", ", _triangles[triIndex + 10], ")");
		}
		*/
	}

	private void CreateMesh()
	{
		var numVertices = _triangleCount * 3;
		_vertices.Capacity = numVertices;
		
		for (int i = 0; i < _triangleCount; i++)
		{
			var triIndex = i * 12;
			_vertices.Add(new Vector3(_triangles[triIndex], _triangles[triIndex + 1], _triangles[triIndex + 2]));
			_vertices.Add(new Vector3(_triangles[triIndex + 4], _triangles[triIndex + 5], _triangles[triIndex+ 6]));
			_vertices.Add(new Vector3(_triangles[triIndex + 8], _triangles[triIndex + 9], _triangles[triIndex + 10]));
		}
		
		var surfaceTool = new SurfaceTool();
		surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
		
		//surfaceTool.SetSmoothGroup(UInt32.MaxValue);
		surfaceTool.SetSmoothGroup(0);
		
		foreach (var vertex in _vertices)
		{
			surfaceTool.AddVertex(vertex);
		}
		surfaceTool.GenerateNormals();
		surfaceTool.Index();
		Mesh mesh = surfaceTool.Commit();
		var meshInstance = new MeshInstance3D();
		meshInstance.Mesh = mesh;
		AddChild(meshInstance);
		
	}
	
	private Rid LoadShader(RenderingDevice rd, string path)
	{
		var shaderFileData = GD.Load<RDShaderFile>(path);
		var shaderSpirV = shaderFileData.GetSpirV();
		return rd.ShaderCreateFromSpirV(shaderSpirV);
	}
	
	private void SetupBuffers()
	{
		// Create Triangle Buffer
		int maxTrisPerVoxel = 5;
		int maxTriangles = maxTrisPerVoxel * (int)Math.Pow(8*8, 3);
		int floatsPerTriangle = sizeof(float) * 3;
		int bytesPerTriangle = floatsPerTriangle * sizeof(float);
		//var maxBytes = new byte[bytesPerTriangle * maxTriangles];
		var maxBytes = new byte[512 * sizeof(float)];
		
		_triangleBuffer = _renderingDevice.StorageBufferCreate((uint)maxBytes.Length, maxBytes);
		var triangleUniform = new RDUniform
		{
			UniformType = RenderingDevice.UniformType.StorageBuffer,
			Binding = 0
		};
		triangleUniform.AddId(_triangleBuffer);
		
		// Create Data Points Buffer
		float[,,] dataPoints =
		{
			{
				{0, 0}, 
				{1, 1}
			}, 
			{ 
				{0, 0}, 
				{1, 1} 
			}
		};

		GD.Print(dataPoints[0, 0, 0]);
		GD.Print(dataPoints[1, 0, 0]);
		GD.Print(dataPoints[0, 0, 1]);
		GD.Print(dataPoints[1, 0, 1]);
		GD.Print("----");
		GD.Print(dataPoints[0, 1, 0]);
		GD.Print(dataPoints[1, 1, 0]);
		GD.Print(dataPoints[0, 1, 1]);
		GD.Print(dataPoints[1, 1, 1]);
		
		
		int totalDataPoints = dataPoints.GetLength(0) * dataPoints.GetLength(1) * dataPoints.GetLength(2);
		var dataPointsBytes = new byte[totalDataPoints * sizeof(float)];
		Buffer.BlockCopy(dataPoints, 0, dataPointsBytes, 0, totalDataPoints * sizeof(float));
		
		_dataPointsBuffer = _renderingDevice.StorageBufferCreate((uint)dataPointsBytes.Length, dataPointsBytes);
		var dataPointsUniform = new RDUniform()
		{
			UniformType = RenderingDevice.UniformType.StorageBuffer,
			Binding = 1
		};
		dataPointsUniform.AddId(_dataPointsBuffer);
		
		// Create Uniforms Buffer
		var paramsArray = new float[]
			{
				_size, 
				_size, 
				_size, 
				_isoLevel,
				_scale,
				0.0f, // Padding
 				0.0f, // Padding
				0.0f // Padding
			};
		var paramBytes = new byte[paramsArray.Length * sizeof(float)];
		Buffer.BlockCopy(paramsArray, 0, paramBytes, 0, paramBytes.Length);

		_paramBuffer = _renderingDevice.UniformBufferCreate((uint)paramBytes.Length, paramBytes);
		var paramUniform = new RDUniform()
		{
			UniformType = RenderingDevice.UniformType.UniformBuffer,
			Binding = 2
		};
		paramUniform.AddId(_paramBuffer);
		
		// Create Counter Buffer
		var counter = new uint[] {0};
		var counterBytes = new byte[sizeof(uint)];
		Buffer.BlockCopy(counter, 0, counterBytes, 0, counterBytes.Length);
		_counterBuffer = _renderingDevice.StorageBufferCreate((uint)counterBytes.Length, counterBytes);
		var counterUniform = new RDUniform()
		{
			UniformType = RenderingDevice.UniformType.StorageBuffer,
			Binding = 3
		};
		counterUniform.AddId(_counterBuffer);
		
		var buffers = new Array<RDUniform>{triangleUniform, dataPointsUniform, paramUniform, counterUniform};
		_uniformSet = _renderingDevice.UniformSetCreate(buffers, _shaderRid, 0);
		_pipeline = _renderingDevice.ComputePipelineCreate(_shaderRid);
	}
	
	private float[,,] GenerateDataPoints(int size)
	{
		var dataPoints = new float[size, size, size];
		
		for (var x = 0; x < size; x++)
		{
			for (var y = 0; y < size; y++)
			{
				for (var z = 0; z < size; z++)
				{
					float value = 0;
					
					if(new Vector3(10, 10, 10).DistanceTo(new Vector3(x, y, z)) < 10)
					{
						value = 1;
					}
					else
					{
						value = -1;
					}
					dataPoints[x, y, z] = value;
				}
			}
		}
		return dataPoints;
	}

	private void GetParamsArray()
	{
		var paramsArray = new Array();
		
		
	}
	
	
}
