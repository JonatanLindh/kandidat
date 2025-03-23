using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace Kandidat.marchingcubes.ComputeShader;
public class GpuVerticesGenerator : IVerticesGenerationStrategy
{
	private float _isoLevel = 0f;
	private int _sizeX;
	private int _sizeY;
	private int _sizeZ;
	private int _scale = 1;
	private readonly List<Vector3> _vertices = new();
	private float[,,] _dataPoints;
	
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
    
    private const int ChunkSize = 32;
    private const int WorkGroupSize = 8;
    private const int NumVoxelPerAxis = ChunkSize * WorkGroupSize;


    private Thread thread;

    public GpuVerticesGenerator()
	{
		thread = new Thread(InitGpu);
		thread.Start();
	}
    ~GpuVerticesGenerator()
	{
		CleanupGpu();
	}
    public List<Vector3> GenerateVertices(float[,,] datapoints, float isoLevel = 0f, int scale = 1)
    {
	    _dataPoints = datapoints;
	    _isoLevel = isoLevel;
	    _scale = scale;
	    _sizeX = datapoints.GetLength(0);
	    _sizeY = datapoints.GetLength(1);
	    _sizeZ = datapoints.GetLength(2);
	    try
	    {
		    thread.Join();
		    for (int xOffset = 0; xOffset < _sizeX; xOffset += ChunkSize - 1)
		    {
			    for (int yOffset = 0; yOffset < _sizeY; yOffset += ChunkSize - 1)
			    {
				    for (int zOffset = 0; zOffset < _sizeZ; zOffset += ChunkSize - 1)
				    {
					    // Update uniform buffer with current chunk information
					    int chunkXSize = Math.Min(ChunkSize, _sizeX - xOffset);
					    int chunkYSize = Math.Min(ChunkSize, _sizeY - yOffset);
					    int chunkZSize = Math.Min(ChunkSize, _sizeZ - zOffset);

					    UpdateChunkUniforms(new Vector3I(xOffset, yOffset, zOffset), 
						    new Vector3I(chunkXSize, chunkYSize, chunkZSize));

					    RunCompute(new Vector3(chunkXSize, chunkYSize, chunkZSize));
					    ProcessComputeData();
					    CreateVertices();
				    }
			    }
		    }
	    }
	    finally
	    {
		    CleanupGpu();
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
		int maxTriangles = maxTrisPerVoxel * (int)Math.Pow(ChunkSize, 3);
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
		var dataPointsBytes = new byte[(ChunkSize * ChunkSize * ChunkSize) * sizeof(float)];

		
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
    
	private void RunCompute(Vector3 chunkSize)
	{
		// Now create the compute list
		var computeList = _renderingDevice.ComputeListBegin();
		_renderingDevice.ComputeListBindComputePipeline(computeList, _pipeline);
		_renderingDevice.ComputeListBindUniformSet(computeList, _uniformSet, 0);

		
		var groupsNeeded = (uint)Math.Ceiling(_sizeX / 8.0f);
		uint workGroupSize = 8;
		/*
		var groupsNeededX = (uint)Math.Ceiling(_sizeX / 8.0f);
		var groupsNeededY = (uint)Math.Ceiling(_sizeY / 8.0f);
		var groupsNeededZ = (uint)Math.Ceiling(chunkZSize / 8.0f);
		*/
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

	private void UpdateChunkUniforms(Vector3I offset, Vector3I chunkSize)
	{
		// Update the datapoints buffer
		var batch = ExtractBatch(chunkSize, offset);
		var dataPointsBytes = new byte[batch.Length * sizeof(float)];
		Buffer.BlockCopy(batch, 0, dataPointsBytes, 0, dataPointsBytes.Length);
		_renderingDevice.BufferUpdate(_dataPointsBuffer, 0, (uint)dataPointsBytes.Length, dataPointsBytes);
		
		var paramsArray = new float[]
		{
			chunkSize.X, 
			chunkSize.Y, 
			chunkSize.Z,
			_isoLevel,
			_scale,
			offset.X, 
			offset.Y,
			offset.Z 
		};
    
		var paramBytes = new byte[paramsArray.Length * sizeof(float)];
		Buffer.BlockCopy(paramsArray, 0, paramBytes, 0, paramBytes.Length);
    
		// Update the uniform buffer
		_renderingDevice.BufferUpdate(_paramBuffer, 0, (uint)paramBytes.Length, paramBytes);
		
		// Reset the counter buffer
		
		var counter = new uint[] {0};
		var counterBytes = new byte[sizeof(uint)];
		Buffer.BlockCopy(counter, 0, counterBytes, 0, counterBytes.Length);
		_renderingDevice.BufferUpdate(_counterBuffer, 0, (uint)counterBytes.Length, counterBytes);
		
		
	}

	private float[] ExtractBatch(Vector3I chunkSize, Vector3I offset)
	{
		var datapoints = _dataPoints;
		
		var batch = new float[chunkSize.X * chunkSize.Y * chunkSize.Z];
		
		for (int z = 0; z < chunkSize.Z; z++)
		{
			for (int y = 0; y < chunkSize.Y; y++)
			{
				for (int x = 0; x < chunkSize.X; x++)
				{
					var index = new Vector3I(x, y, z) + offset;
					batch[x + chunkSize.X * y + chunkSize.X * chunkSize.Y * z] = datapoints[index.X, index.Y, index.Z];
				}
			}
		}
		return batch;
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
	
	private void CreateVertices()
	{
		//var numVertices = _triangleCount * 3;
		//_vertices.Capacity = numVertices;
		
		for (int i = 0; i < _triangleCount; i++)
		{
			var triIndex = i * 12;
			_vertices.Add(new Vector3(_triangles[triIndex], _triangles[triIndex + 1], _triangles[triIndex + 2]));
			_vertices.Add(new Vector3(_triangles[triIndex + 4], _triangles[triIndex + 5], _triangles[triIndex+ 6]));
			_vertices.Add(new Vector3(_triangles[triIndex + 8], _triangles[triIndex + 9], _triangles[triIndex + 10]));
		}
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
	
}