using Godot;
using System;
using Godot.Collections;
using Array = Godot.Collections.Array;

[Tool]
public partial class McSpawnerCS : Node
{
	
	private RenderingDevice _renderingDevice;
	private Rid _shaderRid;
	private Rid _uniformSet;
	private Rid _pipeline;
	private Rid _triangleBuffer;
	private Rid _dataPointsBuffer;
	private Rid _counterBuffer;
	private Rid _paramBuffer;

	private float _isoLevel;
	private int _size = 2;
	private int _scale = 1;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitGpu();
		RunCompute();
		ProcessComputeData();
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
		var counter = BitConverter.ToUInt32(counterData, 0);
		GD.Print(counter);
		
		var triangleData = _renderingDevice.BufferGetData(_triangleBuffer);
		var triangles = new float[triangleData.Length / sizeof(float)];
		Buffer.BlockCopy(triangleData, 0, triangles, 0, triangleData.Length);

		
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
		var maxBytes = new byte[bytesPerTriangle * maxTriangles];
		
		_triangleBuffer = _renderingDevice.StorageBufferCreate((uint)maxBytes.Length, maxBytes);
		var triangleUniform = new RDUniform
		{
			UniformType = RenderingDevice.UniformType.StorageBuffer,
			Binding = 0
		};
		triangleUniform.AddId(_triangleBuffer);
		
		// Create Data Points Buffer
		int totalDataPoints = (_size + 1) * (_size + 1) * (_size + 1);
		float[] datapoints = new float[totalDataPoints];
		for (int i = 0; i < totalDataPoints; i++)
		{
			datapoints[i] = 1.0f; // Default value
		}
		var dataPointsBytes = new byte[datapoints.Length * sizeof(float)];
		Buffer.BlockCopy(datapoints, 0, dataPointsBytes, 0, dataPointsBytes.Length);
		
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
