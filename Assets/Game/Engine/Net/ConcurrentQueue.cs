using UnityEngine;
using System.Collections.Generic;

public class ConcurrentQueue<TType>
{
	private Queue<TType> queue;
	private object queueLock;

	public int count { get {return queue.Count; } }

	public ConcurrentQueue()
	{
		queue = new Queue<TType>();
		queueLock = new object();
	}
	
	public void enqueue(TType data)
	{
		lock (queueLock)
		{
			queue.Enqueue(data);
		}
	}

	public TType dequeue() {
		if (queue.Count > 0)
			return queue.Dequeue();
		else
			return default( TType );
	}
	
	public bool tryDequeue(out TType data)
	{
		data = default(TType);
		bool success = false;
		lock (queueLock)
		{
			if (queue.Count > 0)
			{
				data = queue.Dequeue();
				success = true;
			}
		}
		return success;
	}
}