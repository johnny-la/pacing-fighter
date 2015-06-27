using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ArrayUtils 
{
	/// <summary>
	/// Add the object to the given array and return the new array.
	/// Note: Inefficient. The original array is not altered.
	/// </summary>
	public static T[] Add<T>(T[] array, T item)
	{
		List<T> list = new List<T>(array);
		list.Add (item);
		return list.ToArray ();
	}

	/// <summary>
	/// Remove the object from the given array and return the new array.
	/// Note: Inefficient. The original array is not altered.
	/// </summary>
	public static T[] Remove<T>(T[] array, T item)
	{
		List<T> list = new List<T>(array);
		list.Remove (item);
		return list.ToArray ();
	}

	/// <summary>
	/// Returns a random element from the given array
	/// </summary>
	public static T RandomElement<T>(T[] array)
	{
		// Return a random element from the array
		return array[UnityEngine.Random.Range (0,array.Length)];
	}
}
