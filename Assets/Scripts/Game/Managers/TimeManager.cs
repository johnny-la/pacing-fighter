using UnityEngine;
using System.Collections;

/// <summary>
/// Time manager used to manipulate the game's time scale
/// </summary>

public class TimeManager : MonoBehaviour 
{
	/** Stores the original 'fixedDeltaTime' value. Like this, if it is ever changed, we can set it back to its default value. */
	private static readonly float defaultFixedDeltaTime = Time.fixedDeltaTime;

	void Awake () 
	{
		// If the TimeManager singleton already exists, delete this new instance to avoid duplicates
		if(Instance != null && Instance != this)
			Destroy (this);

		// Choose this instance to be the singleton
		Instance = this;

		// Never destroy this singleton, even when the next scene is loaded.
		DontDestroyOnLoad(gameObject);
	}

	/// <summary>
	/// The TimeManager singleton used to manipulate game time from any script
	/// </summary>
	public static TimeManager Instance { get; set; }

	/// <summary>
	/// Sets the game's time scale for the given amount of time. When this duration elapses, the 
	/// time scale is reverted to its value before this method was called.
	/// </summary>
	/// <param name="duration">If set to zero, the time scale of the game is permanently changed to the given value. 
	public void SetTimeScale(float timeScale, float duration)
	{
		// If the given duration is zero
		if(duration == 0)
			// Change the time scale to the given value permanently
			ChangeTimeScale (timeScale);

		// Delegate the call to a coroutine
		StartCoroutine (SetTimeScaleCoroutine(timeScale, duration));
	}

	/// <summary>
	/// Sets the time scale for 'duration' seconds. Once this timer is elapsed, the time scale is reset to
	/// its original value.
	/// </summary>
	private IEnumerator SetTimeScaleCoroutine(float timeScale, float duration)
	{
		// Store the time scale before this method was called
		float originalTimeScale = Time.timeScale;

		// Update the game's time scale to the given value
		ChangeTimeScale(timeScale);

		// Wait 'duration' seconds. Note that we multiply the duration by the time scale since the 'WaitForSeconds' coroutine
		// is dependent on the timeScale.
		yield return new WaitForSeconds(duration * timeScale);

		// Revert the time scale back to its original value before this method was called
		ChangeTimeScale (originalTimeScale);
	}

	/// <summary>
	/// Update the time scale to the given value. This helper method is used to ensure that the physics get scaled accordingly.
	/// </summary>
	private void ChangeTimeScale(float timeScale)
	{
		// Update the game's time scale to the given value
		Time.timeScale = timeScale;
		// Update the fixedDeltaTime. We scale the default 'fixedDeltaTime' by 'timeScale', to ensure that physics runs more often
		// if the game is running in slow motion
		Time.fixedDeltaTime = defaultFixedDeltaTime * timeScale;
	}
}
