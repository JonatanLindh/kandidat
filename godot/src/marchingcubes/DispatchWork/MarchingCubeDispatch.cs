using Godot;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;


/// <summary>
/// A singleton class responsible for managing and dispatching Marching Cube mesh generation tasks.
/// It handles task queuing, multithreaded processing, and mesh generation using the Marching Cube algorithm.
/// </summary>
public sealed partial class MarchingCubeDispatch : Node
{
	private static MarchingCubeDispatch _instance = null;
	
	private static readonly object Lock = new object();
	private readonly object _generateMeshLock = new();
	
	private Thread _planetGenerator;
	private readonly ConcurrentQueue<MarchingCubeRequest> _planetQueue = new();
	private MarchingCube _marchingCube;
	private bool _insideTree = false;
	
	private readonly ConcurrentBag<long> _workerThreads = new();
	private const uint MaxThreads = 16;


	public override void _Notification(int what)
	{
		if (what == NotificationPredelete || what == NotificationExitTree)
		{
			// Cleanup logic here
			Instance.Cleanup();
		}
		if (what == NotificationReady)
		{
			// Initialization logic here
			Instance.Initialize();
		}
	}

	/// <summary>
	/// Initializes the <see cref="MarchingCubeDispatch"/> instance, setting up the necessary resources
	/// such as the marching cube generator and the thread for processing mesh generation tasks.
	/// </summary>
	private void Initialize()
	{
		Cleanup();
		_insideTree = true;
		// Initialize the marching cube instance
		_marchingCube = new MarchingCube(method: MarchingCube.GenerationMethod.CpuMultiThread);
		
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
			queue.TryPeek(out MarchingCubeRequest request);
			
			
			if (request != null)
			{
				// Try to dequeue the request
				queue.TryDequeue(out request);
				
				// If the request is null, continue to the next iteration
				if (request == null) continue;
				
				
				// Add the task to the worker thread pool
				_workerThreads.Add(WorkerThreadPool.AddTask(Callable.From(() =>
				{
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

					}
					else
					{
						datapoints = request.DataPoints;
					}
					
					var mesh = marchingCube.GenerateMesh(datapoints, request.Scale, request.Offset);
					
					if (mesh == null || mesh.GetSurfaceCount() == 0)
					{
						return;
					}
					
					var meshInstance = request.CustomMeshInstance ?? new MeshInstance3D();
					if (IsInstanceValid(meshInstance))
					{
						if (request.CustomMeshInstance != null)
						{
							meshInstance.CallDeferred(MeshInstance3D.MethodName.SetMesh, mesh);
							meshInstance.CallDeferred(MeshInstance3D.MethodName.CreateMultipleConvexCollisions);
						}
						else
						{
							meshInstance.Mesh = mesh;
							meshInstance.CreateMultipleConvexCollisions();
						}
					}
					//meshInstance.Translate(request.Offset); 
					
					if (request.GeneratePlanetShader != null)
					{
						var material = request.GeneratePlanetShader(marchingCube.MinHeight, marchingCube.MaxHeight);
						if (IsInstanceValid(meshInstance))
						{
							//meshInstance.MaterialOverride = material;
							meshInstance.MaterialOverride = new StandardMaterial3D()
							{
								CullMode = BaseMaterial3D.CullModeEnum.Disabled
							};
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
				})));
				
			}
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

/// <summary>
/// Represents a request for generating a Marching Cube mesh, containing data points,
/// scaling, offset, and references to nodes for mesh generation and scene management.
/// </summary>
public record MarchingCubeRequest
{
	public float[,,] DataPoints { get; init; }
	public CelestialBodyNoise PlanetDataPoints { get; init; }
	public float Scale { get; init; }
	public Vector3 Offset { get; init; }
	public Vector3 Center { get; init; }
	public Node Root { get; init; }
	public Node TempNode { get; init; }
	
	public MeshInstance3D CustomMeshInstance { get; init; }
	
	public Func<float, float, ShaderMaterial> GeneratePlanetShader { get; init; }
}
