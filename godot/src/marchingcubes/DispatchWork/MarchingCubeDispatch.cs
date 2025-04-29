using System;
using Godot;
using System.Collections.Concurrent;
using System.Threading;


/// <summary>
/// A singleton class responsible for managing and dispatching Marching Cube mesh generation tasks.
/// It handles task queuing, multithreaded processing, and mesh generation using the Marching Cube algorithm.
/// </summary>
public sealed partial class MarchingCubeDispatch: Node
{
	private static MarchingCubeDispatch _instance = null;
	
	private static readonly object Lock = new object();
	
	private Thread _planetGenerator;
	private readonly ConcurrentQueue<MarchingCubeRequest> _planetQueue = new();
	private bool _insideTree = false;
	
	private readonly ConcurrentBag<long> _workerThreads = new();
	private const uint MaxThreads = 16;
	
	
	
	/// <summary>
	/// Initializes the <see cref="MarchingCubeDispatch"/> instance, setting up the necessary resources
	/// such as the marching cube generator and the thread for processing mesh generation tasks.
	/// </summary>
	private void Initialize()
	{
		Cleanup();
		_insideTree = true;
		
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


	/// <summary>
	/// Processes the tasks in the provided queue by generating meshes for each request.
	/// It dequeues tasks, generates the mesh using the Marching Cube algorithm, and adds
	/// the resulting mesh to the scene while managing temporary nodes and thread safety.
	/// </summary>
	/// <param name="queue">The queue containing <see cref="MarchingCubeRequest"/> tasks to process.</param>
	/// <seealso cref="MarchingCubeRequest"/>
	private void HandleGeneratingTask(ConcurrentQueue<MarchingCubeRequest> queue)
	{
		// Check if the queue is empty
		while (!queue.IsEmpty)
		{
			// Try to dequeue the first item
			if (!queue.TryDequeue(out MarchingCubeRequest request)) continue;

			// If the request is null, continue to the next iteration
			if (request == null) continue;

			// Add the task to the worker thread pool
			_workerThreads.Add(WorkerThreadPool.AddTask(Callable.From(() => { GeneratePlanet(request); })));
		}
	}

	private static void GeneratePlanet(MarchingCubeRequest request)
	{
		var marchingCube = new MarchingCube(method: MarchingCube.GenerationMethod.Cpu);
				
		// Either CelestialBodyNoise or a regular float[,,] array
		var datapoints = request.DataPoints ?? request.PlanetDataPoints.GetNoise();
		var mesh = marchingCube.GenerateMesh(datapoints, request.Scale);

		var meshInstance = request.CustomMeshInstance ?? new MeshInstance3D();
		meshInstance.Mesh = mesh;
		meshInstance.CreateMultipleConvexCollisions();
		meshInstance.Translate(request.Offset);

		if (request.GeneratePlanetShader != null)
		{
			var material = request.GeneratePlanetShader(marchingCube.MinHeight, marchingCube.MaxHeight);
			meshInstance.MaterialOverride = material;
		}


		if (IsInstanceValid(request.TempNode))
		{
			request.TempNode.CallDeferred(Node.MethodName.QueueFree);
		}

		if (IsInstanceValid(request.Root))
		{
			request.Root.CallDeferred(Node.MethodName.AddChild, meshInstance);
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
		_planetQueue.Enqueue(request);
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