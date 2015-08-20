using UnityEngine;
using System.Collections;

/// <summary>
/// Shakes GameObject's position using Perlin noise.
/// Adapted from unitytipsandtricks.blogspot.ca / YouTube tutorial called "Unity Tips and Tricks: Camera Shake" 
/// from user UnityTnT
/// </summary>
public class ShakePosition : MonoBehaviour 
{
	/** Shaking properties */
	private float duration = 0.5f;
	private float speed = 1.0f;
	private float magnitude = 0.1f;
	
	private bool test = true;

	/** This position is shaken to avoid shaking the Transform's position directly. */
	private Vector3 offset;

	/** Caches this GameObject's Transform. */
	private new Transform transform;

	void Awake () 
	{
		// Cache this GameObject's Transform
		transform = GetComponent<Transform>();
	}

	void Update() 
	{
		if (test) {
			test = false;
			PlayShake(duration,speed,magnitude);
		}
	}

	/// <summary>
	/// Start shaking this GameObject with the given parameters
	/// </summary>
	public void PlayShake(float duration, float speed, float magnitude) 
	{
		this.duration = duration;
		this.speed = speed;
		this.magnitude = magnitude;

		StopAllCoroutines();
		StartCoroutine("Shake");
	}

	IEnumerator Shake() 
	{
		float elapsed = 0.0f;
		
		Vector3 originalPosition = transform.position;
		float randomStart = Random.Range(-1000.0f, 1000.0f);
		
		while (elapsed < duration) {
			
			elapsed += Time.deltaTime;			
			
			float percentComplete = elapsed / duration;			
			
			// We want to reduce the shake from full power to 0 starting half way through
			float damper = 1.0f - Mathf.Clamp(2.0f * percentComplete - 1.0f, 0.0f, 1.0f);
			
			// Calculate the noise parameter starting randomly and going as fast as speed allows
			float alpha = randomStart + speed * percentComplete;
			
			// map noise to [-1, 1]
			float x = Util.Noise.GetNoise(alpha, 0.0f, 0.0f) * 2.0f - 1.0f;
			float y = Util.Noise.GetNoise(0.0f, alpha, 0.0f) * 2.0f - 1.0f;
			
			x *= magnitude * damper;
			y *= magnitude * damper;
			
			transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
			
			yield return null;
		}
		
		// transform.position = originalCamPos;
	}
}
