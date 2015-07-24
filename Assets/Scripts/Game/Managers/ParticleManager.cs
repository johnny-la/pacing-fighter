using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The different types of particle effects. Each refers to a particle effect prefab.
/// </summary>
public enum ParticleEffect
{
	Player_Slash_Axe_A1,
	Player_Slash_Axe_A2,
	Player_Slash_Axe_A3	
}

/// <summary>
/// Manages the particles in the game.
/// </summary>
public class ParticleManager : MonoBehaviour 
{
	/** Stores a list of serialized particle system prefabs. Each particle system corresponds to one enum constant in the 
	  * ParticleEffect enum. Note that the contents of this array are serialized instead of the 'particleEffectPrefabs'
	  * since arrays are easier to serialize. The contents of this array are dumped to the dictionary at runtime. */
	[SerializeField]
	private ParticleSystem[] serializedParticlePrefabs = new ParticleSystem[System.Enum.GetValues (typeof(ParticleEffect)).Length];

	/** Stores prefabs for each particle effect. Every ParticleEffect enum constant is made to link to a particle system */
	private Dictionary<ParticleEffect, ParticleSystem> particleEffectPrefabs = new Dictionary<ParticleEffect, ParticleSystem>();

	/** A dictionary which links each ParticleEffect enum constant to a pool of particle effects. Allows each particle effect to be spawned
	  * from a pool */
	private Dictionary<ParticleEffect, ParticleEffectPool> particleEffectPools = new Dictionary<ParticleEffect, ParticleEffectPool>();

	public void Awake()
	{
		// If the singleton instance already exists, but this object is not singleton instance
		if(Instance != null && Instance != this)
		{
			// Destroy this ParticleManager to ensure only one every exists.
			Destroy (this.gameObject);
		}

		// If the singleton instance has not yet been assigned, set this ParticleManager as the singleton instance
		Instance = this;

		// Stores all the particle effect enum constants in an array
		System.Array particleEffects = System.Enum.GetValues (typeof(ParticleEffect));
		
		// Cycle through each particle effect
		for(int i = 0; i < particleEffects.Length; i++)
		{
			// Set the particle effect's prefab. The correct prefab is serialized in 'serializedParticlePrefabs' under the same index as the 'particleEffects' array.
			SetParticlePrefab ((ParticleEffect)particleEffects.GetValue(i), serializedParticlePrefabs[i]);
		}

		// Don't destroy the GameObject this ParticleManager is attached to when the scene changes. Ensures that the singleton persists through scenes
		DontDestroyOnLoad (this.gameObject);

	}

	/// <summary>
	/// Stores the singleton instance of the particle manager.
	/// </summary>
	public static ParticleManager Instance { get; private set; }

	/// <summary>
	/// Plays the given particle effect at the given position. Note that particle effects are denoted by enum constants
	/// instead of GameObjects
	/// </summary>
	public void Play(ParticleEffect particleEffect, Vector3 position)
	{
		// Retrieves the pool which stores instances of the desired particle effect
		ParticleEffectPool pool = particleEffectPools[particleEffect];

		// Obtain a particle effect from the pool. This is the particle effect that will be played
		ParticleSystem particleSystem = pool.Obtain ();

		// Set the position of the particle system
		particleSystem.transform.position = position;

		// Play the particle effects
		particleSystem.Play ();

		// Wait for the particles to die and place them back in the pool once they do
		StartCoroutine (WaitForParticleDeath(particleSystem, pool));
	}

	/// <summary>
	/// Wait for the particles to die. Once they finish playing, place them back in their respective pool.
	/// </summary>
	public IEnumerator WaitForParticleDeath(ParticleSystem particleSystem, ParticleEffectPool pool)
	{
		// Wait for as long as the particle system lasts
		yield return new WaitForSeconds(particleSystem.duration);

		// Stop the particle system once it is done playing
		particleSystem.Stop ();

		// Place the particle system back into its pool for later re-use.
		pool.Free (particleSystem);
	}

	/// <summary>
	/// Sets the ParticleSystem which will be played when the ParticleEffect enumerator constant is chosen as an effect to play
	/// </summary>
	public void SetParticlePrefab(ParticleEffect particleEffect, ParticleSystem prefab)
	{
		// Sets the particle effect prefab for the enum constant
		if(!particleEffectPrefabs.ContainsKey (particleEffect))
			particleEffectPrefabs.Add(particleEffect, prefab);
		else
			particleEffectPrefabs[particleEffect] = prefab;

		// Creates a new pool which will clone the given prefab. The pool is stored inside an internal dictionary.
		if(!particleEffectPools.ContainsKey (particleEffect))
			particleEffectPools.Add (particleEffect, new ParticleEffectPool(prefab));
		else
			particleEffectPools[particleEffect] = new ParticleEffectPool(prefab);
	}

	/// <summary>
	/// Returns the prefab particle system which is linked to the given ParticleEffect enumerator constant.
	/// Each particle effect is refered to by a ParticleEffect enum constant. </summary>
	public ParticleSystem GetParticlePrefab(ParticleEffect particleEffect)
	{
		// If the particleEffect key does not exist yet in the prefabs dictionary, return null, since no prefab was assigned to this
		// enum constant yet.
		if(!particleEffectPrefabs.ContainsKey (particleEffect))
			return null;

		// Returns an entry in the 'particleEffectPrefabs' dictionary, which maps each ParticleEffect enum constant to an
		// actual particle system prefab
		return particleEffectPrefabs[particleEffect];
	}

	/// <summary>
	/// Stores a list of serialized particle system prefabs. Each particle system corresponds to one enum constant in the 
	/// ParticleEffect enum. Note that the contents of this array are serialized instead of the 'particleEffectPrefabs'
	/// since arrays are easier to serialize. The contents of this array are dumped to the dictionary at runtime.
	/// </summary>
	public ParticleSystem[] SerializedParticlePrefabs
	{
		get { return serializedParticlePrefabs; }
		set { this.serializedParticlePrefabs = value; }
	}
}
