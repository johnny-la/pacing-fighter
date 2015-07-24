using UnityEngine;
using System.Collections;

/// <summary>
/// A pool of particle effects
/// </summary>
/// 
public class ParticleEffectPool : Pool<ParticleSystem>
{
	/// <summary>
	/// The prefab particle effect which is cloned and stored inside this pool.
	/// </summary>
	private ParticleSystem prefab;

	/// <summary>
	/// Initializes a new pool which stores clones of the given prefab
	/// </summary>
	public ParticleEffectPool (ParticleSystem prefab)
	{
		this.prefab = prefab;
	}

	/// <summary>
	/// Retrieves and returns a ParticleSystem from the pool
	/// </summary>
	public override ParticleSystem Obtain()
	{
		// Obtain a particle system from the pool
		ParticleSystem particleSystem = base.Obtain ();

		// Make the particle system active again
		particleSystem.gameObject.SetActive (true);

		// Return the particle system which was retrieved from the pool
		return particleSystem;
	}

	/// <summary>
	/// Add the object back into the pool
	/// </summary>
	public override void Free(ParticleSystem particleSystem)
	{
		// De-activate the particle system before putting it back into the pool 
		particleSystem.gameObject.SetActive (false);

		// Place the particle system back into the pool
		base.Free (particleSystem);
	}
	
	/// <summary>
	/// Creates and returns a new ParticleSystem to store inside the pool.
	/// </summary>
	public override ParticleSystem NewItem()
	{
		// Returns a clone of the given prefab
		return GameObject.Instantiate(prefab);
	}
}

