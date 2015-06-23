using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool<T> where T : new()
{
	private List<T> pool;

	public Pool()
	{
		pool = new List<T>();
	}

	public Pool(int defaultCapacity)
	{
		pool = new List<T>(defaultCapacity);
	}

	public T GetItem()
	{
		T item = default (T); 

		// If no more objects remain in the pool
		if(pool.Count == 0)
		{
			// Create a new object
			item = new T();
		}
		// Else, if items remain in the pool
		else
		{
			// Remove the last object in the pool
			item = pool[pool.Count - 1];
			pool.RemoveAt(pool.Count - 1);
		}

		// Return an item
		return item;
	}

	/// <summary>
	/// Add the object back into the pool
	/// </summary>
	public void Free(T item)
	{
		pool.Add (item);
	}
}
