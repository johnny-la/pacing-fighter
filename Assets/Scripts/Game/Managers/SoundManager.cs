using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour 
{
	/** Caches the GameObject's AudioSource component */
	private AudioSource audioSource;
	
	void Awake () 
	{
		// Cache the GameObject's audio source for performance reasons
		audioSource = GetComponent<AudioSource>();
	}

	/// <summary>
	/// Play the specified sound from this GameObject's AudioSource component.
	/// </summary>
	public void Play(AudioClip sound)
	{
		// Randomize the pitch of the sound to introduce variability
		audioSource.pitch = (float)UnityEngine.Random.Range (0.95f, 1.05f);

		// Play the given AudioClip
		audioSource.clip = sound;
		audioSource.Play ();
	}
	
	public void PlayRandomSound(AudioClip[] sounds)
	{
		// Ignore this call if the given array contains no sounds
		if(sounds == null || sounds.Length == 0)
			return;

		// Stores the audio clip to play
		AudioClip sound = null;

		// Choose a random sound from the given array
		while(sound == null)
			sound = ArrayUtils.RandomElement (sounds);

		// Play the chosen sound
		Play (sound);
	}
}
