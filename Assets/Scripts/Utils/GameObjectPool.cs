using UnityEngine;
using System.Collections;

/// <summary>
/// A pool of game objects
/// </summary>
/// 
public class GameObjectPool : Pool<GameObject>
{
	/// <summary>
	/// The prefab gameObject which is cloned and stored inside this pool.
	/// </summary>
	private GameObject prefab;
	
	/// <summary>
	/// Initializes a new pool which stores clones of the given prefab
	/// </summary>
	public GameObjectPool (GameObject prefab)
	{
		this.prefab = prefab;
	}
	
	/// <summary>
	/// Retrieves and returns a GameObject from the pool
	/// </summary>
	public override GameObject Obtain()
	{
		// Obtain a particle system from the pool
		GameObject gameObject = base.Obtain ();
		
		// Make the GameObject active again
		gameObject.SetActive (true);
		
		// Return the game object which was retrieved from the pool
		return gameObject;
	}
	
	/// <summary>
	/// Add the object back into the pool
	/// </summary>
	public override void Free(GameObject gameObject)
	{
		// De-activate the game object before putting it back into the pool 
		gameObject.SetActive (false);
		
		// Place the game object back into the pool
		base.Free (gameObject);
	}
	
	/// <summary>
	/// Creates and returns a new GameObject to store inside the pool.
	/// </summary>
	public override GameObject NewItem()
	{
		// Returns a clone of the given prefab
		return GameObject.Instantiate(prefab);
	}
}

