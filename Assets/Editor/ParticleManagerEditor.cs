using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(ParticleManager))]
public class ParticleManagerEditor : Editor
{
	// Stores each ParticleEffect enum constant in an array.
	private ParticleEffect[] particleEffects;

	public void OnEnable()
	{
		// Stores the ParticleManager instance this inspector is modifying
		ParticleManager particleManager = (ParticleManager) target;

		// Store each ParticleEffect enum constant in an array
		particleEffects = (ParticleEffect[])System.Enum.GetValues (typeof(ParticleEffect));

		// If there are a different number of ParticleEffect enum constants than there are particle effect prefabs, the prefab array must be
		// re-created to correspond to the new size of the ParticleEffect enum
		if(particleEffects.Length != particleManager.SerializedParticlePrefabs.Length)
		{
			// Creates a new array which will store all the particle prefabs
			ParticleSystem[] prefabs = new ParticleSystem[particleEffects.Length];

			// Cycle through each ParticleEffect enum constant
			for(int i = 0; i < particleEffects.Length; i++)
			{
				// Find the index corresponding to the particle effect being cycled through. This is where the effect's prefab 
				// will be found in the 'particleManager.SerializedParticlePrefabs' array
				int index = EditorPrefs.GetInt (particleEffects[i].ToString(),-1);

				// If the particle effect's index is non-existant, this particle effect was just added to the ParticleEffect enum
				if(index  == -1 || index >= particleManager.SerializedParticlePrefabs.Length)
				{
					// Thus, set the particle effect's prefab to null, since no effect was assigned to it yet
					prefabs[i] = null;
				}
				// Else, if the particle effect being cycled through already has a prefab assigned in the 'SerializedParticlePrefabs' array
				else
				{
					// Copy the old Serialized prefab for the particle effect being cycled through into the new array.
					prefabs[i] = particleManager.SerializedParticlePrefabs[index];
				}


				// Save the index which corresponds to the current ParticleEffect enum constant. This way, when enum constants are
				// created or deleted, this script can know in which index to find the correct prefabs for each particle effect
				EditorPrefs.SetInt (particleEffects[i].ToString(), i);
			}

			// Copy the newly created prefabs array into the 'SerializedParticlePrefabs' array so that the prefabs can be saved
			particleManager.SerializedParticlePrefabs = prefabs;
		}
	}

	public override void OnInspectorGUI()
	{
		// Stores the ParticleManager instance this inspector is modifying
		ParticleManager particleManager = (ParticleManager) target;

		// Store each ParticleEffect enum constant in an array
		particleEffects = (ParticleEffect[])System.Enum.GetValues (typeof(ParticleEffect));

		// Cycle through each particle effect enum constant
		for(int i = 0; i < particleEffects.Length; i++)
		{
			// Store the particle effect enum constant being cycled through
			ParticleEffect particleEffect = particleEffects[i];

			// Assign a prefab to each ParticleEffect enum constant in the 'SerializedParticlePrefabs' array
			particleManager.SerializedParticlePrefabs[i] = (ParticleSystem)EditorGUILayout.ObjectField (particleEffect.ToString (),
			                                                                               particleManager.SerializedParticlePrefabs[i],
			                                                                               typeof(ParticleSystem),false);
		}
	}

	public void OnDisable()
	{
		// Save the changes made to this manager
		ParticleManager particleManager = (ParticleManager) target;
		AssetDatabase.Refresh();
		EditorUtility.SetDirty(particleManager);
		AssetDatabase.SaveAssets();
	}
}