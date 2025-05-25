using System;
using Godot;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;


/// <summary>
/// A singleton class responsible for managing and dispatching Marching Cube mesh generation tasks.
/// It handles task queuing, multithreaded processing, and mesh generation using the Marching Cube algorithm.
/// </summary>
public sealed partial class MarchingCubeDispatch: Node3D
{
	private static MarchingCubeDispatch _instance = null;
	
	private static readonly object Lock = new object();
	private readonly object _generateMeshLock = new();
	private readonly object _materialAssignmentLock = new();
	
	private Thread _planetGenerator;
	private readonly ConcurrentStack<MarchingCubeRequest> _planetQueue = new();
	private bool _insideTree = false;
	
	private readonly ConcurrentBag<long> _workerThreads = new();
	private const uint MaxThreads = 32;
	
	private readonly ConcurrentDictionary<Guid, MarchingCubeRequest> _requests = new();
	public ConcurrentDictionary<Guid, MarchingCubeRequest> Requests => _requests;

	private ConcurrentQueue<(Mesh, MeshInstance3D)> _meshQueue = new();
	private int _meshQueueCount = 0;
	private const int MaxMeshQueueCount = 10;
	
	/// <summary>
	/// Initializes the <see cref="MarchingCubeDispatch"/> instance, setting up the necessary resources
	/// such as the marching cube generator and the thread for processing mesh generation tasks.
	/// </summary>
	private void Initialize()
	{
		Cleanup();
		_insideTree = true;
		
		// Add the singleton to the scene tree if it's not already
		if (!IsInsideTree())
		{
			// Get the current scene tree
			var tree = Engine.GetMainLoop() as SceneTree;
			tree?.Root.AddChild(this);
		}
		
		// Start the planet generator thread
		if (_planetGenerator is { IsAlive: true }) return;
		_planetGenerator = new Thread(GenerateLoop);
		_planetGenerator.Start();
	}


	/// <summary>
	/// Cleans up resources and resets the state of the <see cref="MarchingCubeDispatch"/>  instance.
	/// This includes clearing the task queue, waiting for worker threads to complete,
	/// and resetting the worker thread collection.
	/// </summary>
	private void Cleanup()
	{
		_insideTree = false;
		_planetQueue.Clear();
		_requests.Clear();
		foreach (var threadId in _workerThreads)
		{
			WorkerThreadPool.WaitForTaskCompletion(threadId);
		}
		_workerThreads.Clear();
		_planetGenerator?.Join();
		_planetGenerator = null;
		
	}
	
	public static MarchingCubeDispatch Instance
	{
		get
		{
			lock (Lock)
			{
				if (_instance == null)
				{
					_instance = new MarchingCubeDispatch();
					// Initialize the instance
					_instance.Initialize();
				}
				return _instance;
			}
		}
	}

	public override void _Process(double delta)
	{
		// Process the mesh queue
		while (_meshQueue.TryDequeue(out var tuple) && _meshQueueCount < MaxMeshQueueCount)
		{
			if (IsInstanceValid(tuple.Item2))
			{
				tuple.Item2.CallDeferred(MeshInstance3D.MethodName.SetMesh, tuple.Item1);
				tuple.Item2.CallDeferred(MeshInstance3D.MethodName.CreateConvexCollision);
			}
			_meshQueueCount++;
		}
		if (_meshQueueCount > 0)
		{
			// Reset the mesh queue count
			_meshQueueCount = 0;
		}
	}

	public bool IsTaskBeingProcessed(Guid id)
	{
		//_requests.TryGetValue(id, out var request);
		return _requests.TryGetValue(id, out _);
		//&& request.IsProcessing;
	}


	/// <summary>
	/// Processes the tasks in the provided queue by generating meshes for each request.
	/// It dequeues tasks, generates the mesh using the Marching Cube algorithm, and adds
	/// the resulting mesh to the scene while managing temporary nodes and thread safety.
	/// </summary>
	/// <param name="queue">The queue containing <see cref="MarchingCubeRequest"/> tasks to process.</param>
	/// <seealso cref="MarchingCubeRequest"/>
	private void HandleGeneratingTask(ConcurrentStack<MarchingCubeRequest> queue)
	{
		// Check if the queue is empty
		while (!queue.IsEmpty)
		{
			// Try to pop the first item
			if (!queue.TryPop(out MarchingCubeRequest request)) continue;

			// If the request is null or the request is already processing, continue to the next iteration
			if (request == null) continue;
			if(request.IsProcessing) continue;
			
			// Skip if the request has been cancelled (no longer in the dictionary)
			if (!_requests.ContainsKey(request.Id)) continue;

			// Add the task to the worker thread pool
			_workerThreads.Add(WorkerThreadPool.AddTask(Callable.From(() => { GeneratePlanet(request); })));
		}
	}

	private void GeneratePlanet(MarchingCubeRequest request)
	{
		_requests[request.Id].IsProcessing = true;
		MarchingCube marchingCube = new MarchingCube(method: MarchingCube.GenerationMethod.Cpu);
					
		var dataPointsOffset = request.Center - request.Offset;
		// Either CelestialBodyNoise or a regular float[,,] array
		float[,,] datapoints;
		if (request.PlanetDataPoints != null)
		{
			
			lock (_generateMeshLock)
			{
				request.PlanetDataPoints.VoxelSize = request.Scale;
				datapoints = request.PlanetDataPoints.GetNoise(dataPointsOffset);
			}
			
			
			//datapoints = GenerateDataPoints(dataPointsOffset, request.Scale);
		}
		else
		{
			datapoints = request.DataPoints;
		}
					
		var mesh = marchingCube.GenerateMesh(datapoints, request.Scale, request.Offset);
					
		if (mesh == null || mesh.GetSurfaceCount() == 0)
		{
			if (!_requests.TryRemove(request.Id, out _))
			{
				GD.Print($"Unable to remove request with ID {request.Id} from the queue.");
			}
			return;
		}

					
		var meshInstance = request.CustomMeshInstance ?? new MeshInstance3D();
		if (IsInstanceValid(meshInstance))
		{
			_meshQueue.Enqueue((mesh, meshInstance));
			//meshInstance.CallDeferred(MeshInstance3D.MethodName.SetMesh, mesh);
			//meshInstance.CallDeferred(MeshInstance3D.MethodName.CreateTrimeshCollision);

		}
		//meshInstance.Translate(request.Offset); 
					
		if (request.GeneratePlanetShader != null)
		{
			// Create the material directly in the worker thread
			if (IsInstanceValid(meshInstance))
			{
				lock (_materialAssignmentLock)
				{
					try
					{
						var material = request.GeneratePlanetShader(marchingCube.MinHeight, marchingCube.MaxHeight, request.Center);
						meshInstance.MaterialOverride = material;
					}
					catch (Exception)
					{
						// ignored
					}
				}
			}
		}

		if (IsInstanceValid(request.TempNode))
		{
			request.TempNode.CallDeferred(Node.MethodName.QueueFree);
		}

		if (IsInstanceValid(request.Root) && request.CustomMeshInstance == null)
		{
			request.Root.CallDeferred(Node.MethodName.AddChild, meshInstance);
		}

		if (request.GenerateGrass)
		{	
			NewGrass grass = new NewGrass();
			var meshSurface = mesh.SurfaceGetArrays(0);
			if (meshSurface != null)
			{
				var grassInstance = grass.PopulateMesh(meshSurface, 50f);
				if (grassInstance != null && IsInstanceValid(meshInstance))
				{
					meshInstance.CallDeferred(Node.MethodName.AddChild, grassInstance);
				}
			}
		}

		if (!_requests.TryRemove(request.Id, out _))
		{
			GD.Print($"Unable to remove request with ID {request.Id} from the queue.");
		}
	}

	private void GenerateLoop()
	{
		while (_insideTree)
		{
			HandleGeneratingTask(_planetQueue);
			
			// Check if the worker threads are busy
			if (_workerThreads.Count >= MaxThreads)
			{
				// Wait for all worker threads to finish
				foreach (var threadId in _workerThreads)
				{
					WorkerThreadPool.WaitForTaskCompletion(threadId);
				}
				_workerThreads.Clear();
			}
			else
			{
				//Thread.Sleep(100); // Sleep for a short duration to avoid busy waiting
			}
		}
	}
	
	public void AddToQueue(MarchingCubeRequest request)
	{
		_requests.TryAdd(request.Id, request);
		_planetQueue.Push(request);
	}
	public void AddToQueue(MarchingCubeRequest request, Guid id)
	{
		_requests.TryAdd(id, request);
		_planetQueue.Push(request);
	}

	public void CancelRequest(Guid requestId)
	{
		if (_requests.TryRemove(requestId, out _))
		{
			// Successfully removed the request
		}
	}
	
	private static float[,,] GenerateDataPoints(Vector3 offset , float voxelSize)
	{
		
		var radius = 100;
		int size = 16 + 1;
		var dataPoints = new float[size, size, size];
		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				for (int z = 0; z < size; z++)
				{
					Vector3 worldPos = new Vector3(x, y, z) * voxelSize;
					worldPos += offset;
					var value = -Sphere(worldPos, Vector3.Zero, radius);
					value = Mathf.Clamp(value, -1.0f, 1.0f);
					dataPoints[x, y, z] = value;
					

				}
			}
		}
		return dataPoints;
	}
	private static float Sphere(Vector3 worldPos, Vector3 origin, float radius) {
		return (worldPos - origin).Length() - radius;
	}
	
	internal class CleanupModule
	{
		[System.Runtime.CompilerServices.ModuleInitializer]
		public static void Initialize()
		{
			System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(System.Reflection.Assembly.GetExecutingAssembly()).Unloading += alc =>
			{
				// Unload any unloadable references
				_instance?.Cleanup();
			};
		}
	}
}