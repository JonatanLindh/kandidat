using Godot;
using System;
using System.Collections.Concurrent;
using System.Threading;

public sealed partial class MarchingCubeDispatch : Node
{
	private static MarchingCubeDispatch _instance = null;
	private static readonly object Lock = new object();
	
	
	private Thread _planetGenerator;
	private readonly ConcurrentQueue<MarchingCubeRequest> _planetQueue = new();
	private MarchingCube _marchingCube;
	private bool _insideTree = false;
	
	private readonly ConcurrentBag<long> _workerThreads = new();
	private const uint MaxThreads = 16;
	private readonly object _generateMeshLock = new();


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

	private void Initialize()
	{
		_insideTree = true;
		// Initialize the marching cube instance
		_marchingCube = new MarchingCube(method: MarchingCube.GenerationMethod.CpuMultiThread);
		
		// Start the planet generator thread
		if (_planetGenerator is { IsAlive: true }) return;
		_planetGenerator = new Thread(GenerateLoop);
		_planetGenerator.Start();
	}


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


	private void HandleGeneratingTask(ConcurrentQueue<MarchingCubeRequest> queue)
	{
		// Check if the queue is empty
		while (!queue.IsEmpty)
		{
			// Try to dequeue the first item
			queue.TryPeek(out MarchingCubeRequest request);
			
			// If the request is null, continue to the next iteration
			if (request != null)
			{
				// Try to dequeue the request
				queue.TryDequeue(out request);
				
				// If the request is null, continue to the next iteration
				if (request == null) continue;
				
				
				// Add the task to the worker thread pool
				_workerThreads.Add(WorkerThreadPool.AddTask(Callable.From(() =>
				{
					lock (_generateMeshLock) // Ensure only one thread calls GenerateMesh at a time
					{
						var mesh = _marchingCube.GenerateMesh(request.DataPoints, request.Scale);
						
						var meshInstance = new MeshInstance3D();
						meshInstance.Mesh = mesh;
						meshInstance.CreateMultipleConvexCollisions();
						meshInstance.Translate(request.Offset); 
						
						if (IsInstanceValid(request.TempNode))
							request.TempNode.CallDeferred(Node.MethodName.QueueFree);

						if (IsInstanceValid(request.Root))
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
	
	public void ClearQueue()
	{
		_planetQueue.Clear();
	}
	
	private float[,,] GenerateDataPoints()
	{
		var radius = 32;
		int size = radius * 2 + 1;
		var dataPoints = new float[size, size, size];

		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				for (int z = 0; z < size; z++)
				{
					Vector3 worldPos = new Vector3(x, y, z);
					var value = -Sphere(worldPos, Vector3.One * radius, radius);
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
	
}

public record MarchingCubeRequest
{
	public float[,,] DataPoints { get; init; }
	public float Scale { get; init; }
	public Vector3 Offset { get; init; }
	public Node Root { get; init; }
	public Material MeshMaterial { get; init; }
	public Node TempNode { get; init; }
}
