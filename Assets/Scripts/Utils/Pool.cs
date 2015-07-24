using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Pool<T> where T : new()
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

	/// <summary>
	/// Creates and returns a new item to store inside the pool. 
	/// </summary>
	public abstract T NewItem();

	public virtual T Obtain()
	{
		T item = default (T); 

		// If no more objects remain in the pool
		if(pool.Count == 0)
		{
			// Create a new object to store in the pool
			item = NewItem ();
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
	public virtual void Free(T item)
	{
		pool.Add (item);
	}
}
