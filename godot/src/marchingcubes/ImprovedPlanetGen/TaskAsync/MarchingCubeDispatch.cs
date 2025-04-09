using Godot;
using System;
using System.Collections.Concurrent;
using System.Threading;

public sealed class MarchingCubeDispatch
{
	private static MarchingCubeDispatch _instance = null;
	private static readonly object Lock = new object();
	
	
	private Thread _planetGenerator;
	private readonly ConcurrentQueue<MarchingCubeRequest> _planetQueue = new();
	private MarchingCube _marchingCube;
	private bool _insideTree = false;
	


	public MarchingCubeDispatch()
	{
		_insideTree = true;
		_marchingCube = new MarchingCube();
		_planetGenerator = new Thread(GenerateLoop);
		_planetGenerator.Start();
		
	}

	~MarchingCubeDispatch()
	{
		_insideTree = false;
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
				}
				return _instance;
			}
		}
	}


	private void HandleGeneratingTask(ConcurrentQueue<MarchingCubeRequest> queue)
	{
		while (!queue.IsEmpty)
		{
			queue.TryPeek(out MarchingCubeRequest request);
			if (request != null)
			{
				queue.TryDequeue(out request);
				if (request == null) continue;
				var mesh = _marchingCube.GenerateMesh(request.DataPoints, request.Scale);
				// Handle the generated mesh (e.g., add it to the scene)
				// For example: AddChild(mesh);
				mesh.Translate(request.Offset); 
				request.Root.CallDeferred(Node.MethodName.AddChild, mesh);

			}
		}
	}

	private void GenerateLoop()
	{
		while (_insideTree)
		{
			HandleGeneratingTask(_planetQueue);
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
}
