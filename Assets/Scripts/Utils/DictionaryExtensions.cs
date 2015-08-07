using System.Collections;
using System.Collections.Generic;

public static class DictionaryExtensions
{
	/// <summary>
	/// Insert a (key,value) pair into the dictionary. If the key already exists in the dictionary, its value is updated to the 
	/// given value.
	/// </summary>
	public static void Insert<T,U>(this Dictionary<T, U> dictionary, T key, U value)
	{
		// If the key already exists in the dictionary
		if(dictionary.ContainsKey (key))
			// Update its value
			dictionary[key] = value;
		// Else, if the key doesn't exist in the dictionary
		else
			// Add the (key,value) pair into the dictionary
			dictionary.Add (key, value);
	}
}


