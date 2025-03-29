using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

/// <summary>
/// Generates vertices for the marching cubes algorithm on the GPU.
/// </summary>
public class GpuVerticesGenerator : IVerticesGenerationStrategy
{
	private float _isoLevel = 0f;
	private int _sizeX;
	private int _sizeY;
	private int _sizeZ;
	private int _scale = 1;
	private readonly List<Vector3> _vertices = new();
	private float[,,] _dataPoints;
	private readonly bool _customSize = false;
	
    // Shader Variables
    private RenderingDevice _renderingDevice;
    private Rid _shaderRid;
    private Rid _pipeline;
    private Rid _uniformSet;
    private Rid _triangleBuffer;
    private Rid _dataPointsBuffer;
    private Rid _counterBuffer;
    private Rid _paramBuffer;
    private Rid _triangleTableBuffer;
	
    // Data received from Compute Shader
    private float[] _triangles;
    private int _triangleCount;
    
	public GpuVerticesGenerator(Vector3I batchSize)
	{
		_sizeX = batchSize.X;
		_sizeY = batchSize.Y;
		_sizeZ = batchSize.Z;
		InitGpu();
	}
	public GpuVerticesGenerator()
	{
		_sizeX = _sizeY = _sizeZ = 64;
		InitGpu();
	}

    ~GpuVerticesGenerator()
	{
		RenderingServer.Singleton.CallOnRenderThread(Callable.From(
			CleanupGpu));
	}
    public List<Vector3> GenerateVertices(float[,,] datapoints, float isoLevel = 0f, int scale = 1)
    {
	    _dataPoints = datapoints;
	    _isoLevel = isoLevel;
	    _scale = scale;
	    var dataSizeX = datapoints.GetLength(0);
	    var dataSizeY = datapoints.GetLength(1);
	    var dataSizeZ = datapoints.GetLength(2);
	    var batchSize = new Vector3I(_sizeX, _sizeY, _sizeZ);
	    for (int xOffset = 0; xOffset < dataSizeX; xOffset += batchSize.X - 1)
	    {
		    for (int yOffset = 0; yOffset < dataSizeY; yOffset += batchSize.Y - 1)
		    {
			    for (int zOffset = 0; zOffset < dataSizeZ; zOffset += batchSize.Z - 1)
			    {
				    // Set the size for the current chunk
				    _sizeX = Math.Min(batchSize.X, dataSizeX - xOffset);
				    _sizeY = Math.Min(batchSize.Y, dataSizeY - yOffset);
				    _sizeZ = Math.Min(batchSize.Z, dataSizeZ - zOffset);

				    UpdateBuffers(new Vector3I(xOffset, yOffset, zOffset));
				    RunCompute(new Vector3(_sizeX, _sizeY, _sizeZ));
				    ProcessComputeData();
				    CreateVertices();
			    }
		    }
	    }

	    return _vertices;
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
    private Rid LoadShader(RenderingDevice rd, string path)
    {
	    var shaderFileData = GD.Load<RDShaderFile>(path);
	    var shaderSpirV = shaderFileData.GetSpirV();
	    return rd.ShaderCreateFromSpirV(shaderSpirV);
    }
    private void SetupBuffers()
	{
		// Create Triangle Buffer
		const int maxTrisPerVoxel = 5; 
		int maxTriangles = maxTrisPerVoxel * _sizeX * _sizeY * _sizeZ;
		const int floatsPerTriangle = sizeof(float) * 3;
		const int bytesPerTriangle = floatsPerTriangle * sizeof(float);
		var maxBytes = new byte[bytesPerTriangle * maxTriangles];
		
		_triangleBuffer = _renderingDevice.StorageBufferCreate((uint)maxBytes.Length, maxBytes);
		var triangleUniform = new RDUniform
		{
			UniformType = RenderingDevice.UniformType.StorageBuffer,
			Binding = 0
		};
		triangleUniform.AddId(_triangleBuffer);
		
		// Create Data Points Buffer
		var dataPointsBytes = new byte[(_sizeX * _sizeY * _sizeZ) * sizeof(float)];
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
				_sizeX, 
				_sizeY, 
				_sizeZ, 
				_isoLevel,
				_scale,
				0,
 				0,
				0
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
		
		// Create Triangle Table Buffer
		var triangleTable = LoadTriangulations();
		var triangleTableBytes = new byte[triangleTable.Length * sizeof(int)];
		Buffer.BlockCopy(triangleTable, 0, triangleTableBytes, 0, triangleTableBytes.Length);
		_triangleTableBuffer = _renderingDevice.StorageBufferCreate((uint)triangleTableBytes.Length, triangleTableBytes);
		var triangleTableUniform = new RDUniform()
		{
			UniformType = RenderingDevice.UniformType.StorageBuffer,
			Binding = 4
		};
		triangleTableUniform.AddId(_triangleTableBuffer);
		
		var buffers = new Array<RDUniform>{triangleUniform, dataPointsUniform, paramUniform, counterUniform, triangleTableUniform};
		_uniformSet = _renderingDevice.UniformSetCreate(buffers, _shaderRid, 0);
		_pipeline = _renderingDevice.ComputePipelineCreate(_shaderRid);
	}
    
	private void RunCompute(Vector3 chunkSize)
	{
		// Now create the compute list
		var computeList = _renderingDevice.ComputeListBegin();
		_renderingDevice.ComputeListBindComputePipeline(computeList, _pipeline);
		_renderingDevice.ComputeListBindUniformSet(computeList, _uniformSet, 0);

		
		uint workGroupSize = 8;
		var groupsNeededX = (uint)Math.Ceiling(chunkSize.X / workGroupSize);
		var groupsNeededY = (uint)Math.Ceiling(chunkSize.Y / workGroupSize);
		var groupsNeededZ = (uint)Math.Ceiling(chunkSize.Z / workGroupSize);
		_renderingDevice.ComputeListDispatch(computeList,
			xGroups: groupsNeededX,
			yGroups: groupsNeededY,
			zGroups: groupsNeededZ);
			
		_renderingDevice.ComputeListEnd();
		_renderingDevice.Submit();
	}
	
	
	private void ProcessComputeData()
	{
		_renderingDevice.Sync();
		
		// Get Output Data
		var counterData = _renderingDevice.BufferGetData(_counterBuffer);
		_triangleCount = BitConverter.ToInt32(counterData, 0);
		
		var triangleData = _renderingDevice.BufferGetData(_triangleBuffer);
		_triangles = new float[triangleData.Length / sizeof(float)];
		Buffer.BlockCopy(triangleData, 0, _triangles, 0, triangleData.Length);
		
	}

	private void UpdateBuffers(Vector3I offsets)
	{
		// Update data points buffer
		var dataPoints = ExtractDataPoints(offsets);
		var dataPointsBytes = new byte[dataPoints.Length * sizeof(float)];
		Buffer.BlockCopy(dataPoints, 0, dataPointsBytes, 0, dataPointsBytes.Length);
		_renderingDevice.BufferUpdate(_dataPointsBuffer, 0, (uint)dataPointsBytes.Length, dataPointsBytes);
		
		// Update params buffer
		var paramsArray = new float[]
		{
			_sizeX, 
			_sizeY, 
			_sizeZ, 
			_isoLevel,
			_scale,
			offsets.X,
			offsets.Y,
			offsets.Z
		};
		var paramBytes = new byte[paramsArray.Length * sizeof(float)];
		Buffer.BlockCopy(paramsArray, 0, paramBytes, 0, paramBytes.Length);
		_renderingDevice.BufferUpdate(_paramBuffer,0 ,(uint)paramBytes.Length, paramBytes);
		
		// Reset counter
		var counter = new uint[] {0};
		var counterBytes = new byte[sizeof(uint)];
		Buffer.BlockCopy(counter, 0, counterBytes, 0, counterBytes.Length);
		_renderingDevice.BufferUpdate(_counterBuffer, 0, (uint)counterBytes.Length, counterBytes);
	}
	
	private void CreateVertices()
	{
		for (int i = 0; i < _triangleCount; i++)
		{
			var triIndex = i * 12;
			// Safety check to prevent out of bounds access
			if (triIndex + 10 < _triangles.Length)
			{
				_vertices.Add(new Vector3(_triangles[triIndex], _triangles[triIndex + 1], _triangles[triIndex + 2]));
				_vertices.Add(new Vector3(_triangles[triIndex + 4], _triangles[triIndex + 5], _triangles[triIndex + 6]));
				_vertices.Add(new Vector3(_triangles[triIndex + 8], _triangles[triIndex + 9], _triangles[triIndex + 10]));
			}
			else
			{
				GD.PrintErr($"Skipping triangle {i} due to insufficient data");
				break;
			}
		}
	}
	
	private int[] LoadTriangulations()
	{
		// Read in the triangle table
		using var file = FileAccess.Open("res://src/marchingcubes/ComputeShader/MarchingCubesLUT.txt", FileAccess.ModeFlags.Read);
		string content = file.GetAsText();
		List<int> triangulations = new List<int>();
		file.Close();
		var indexStrings = content.Split(",");
		foreach(var index in indexStrings)
		{
			triangulations.Add(int.Parse(index));
		}
		return triangulations.ToArray();
	}
	
	private float[] ExtractDataPoints(Vector3I offsets)
	{
		var dataPoints = new float[_sizeX * _sizeY * _sizeZ];
		
		for (int z = 0; z < _sizeZ; z++)
		{
			for (int y = 0; y < _sizeY; y++)
			{
				for (int x = 0; x < _sizeX; x++)
				{
					var index = new Vector3I(x, y, z) + offsets;
					dataPoints[x + _sizeX * y + _sizeX * _sizeY * z] =  _dataPoints[index.X, index.Y, index.Z];
				}
			}
		}
		return dataPoints;
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
		
		_renderingDevice.FreeRid(_triangleTableBuffer);
		_triangleTableBuffer = new Rid();

		_renderingDevice.Free();
		_renderingDevice = null;
	}
	
}