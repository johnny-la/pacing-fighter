using UnityEngine;
using System.Collections;

/// <summary>
/// Regenerates a character's health when one of its methods are called.
/// </summary>

public class RegeneratingHealth : MonoBehaviour 
{
	/** The character being healed by this script. */
	private Character character;

	[Tooltip("The default rate (in percent/second) at which the character heals his health. If set to 0.5, the character heals 50% of his total health every second.")]
	[Range(0,1)]
	public float defaultRegenerationRate;

	/** The rate at which the character's health regenerates (in percent/second). That is, if this value is set to 0.5, for 
	 *  example, the character heals 50% of his total health every second. */
	private float regenerationRate;

	void Awake()
	{
		// Cache the Character component for the character being healed
		character = GetComponent<Character>();

		// Set the character's regeneration rate to its default value.
		regenerationRate = defaultRegenerationRate;
	}

	/// <summary>
	/// Regenerates the character back to the given health percentage at the speed of his current 'regenerationRate'.
	/// </summary>
	public void Regenerate(float percentHealth)
	{
		Debug.LogWarning ("REGENERATE HEALTH");
		// Start a coroutine which which regenerate the character back to the given health percentage
		StartCoroutine(RegenerateCoroutine(percentHealth));
	}

	/// <summary>
	/// Coroutine which regenerates the character back to a percentage of his max health
	/// </summary>
	private IEnumerator RegenerateCoroutine(float percentHealth)
	{
		// If the character's health percentage is below the desired value, keep regenerating
		while(character.CharacterStats.HealthPercent < percentHealth)
		{
			// Regenerate the character's health at a rate of 'regenerationRate' percent per second. If 'regenerationRate=0.2', the character
			// heals 20% of his health bar each second.
			float newHealth = character.CharacterStats.HealthPercent + (regenerationRate * Time.deltaTime);

			// Clamp the character's new health to 'percentHealth' to avoid over-healing the character's health beyond the given percentage.
			character.CharacterStats.HealthPercent = Mathf.Clamp(newHealth, 0, percentHealth);

			// Wait for the next frame to heal more.
			yield return null;
		}

	}

	/// <summary>
	/// The rate at which the character's health regenerates (in percent/second). That is, if this value is set to 0.5, for 
	/// example, the character heals 50% of his total health every second.
	/// </summary>
    private float RegenerationRate
	{
		get { return regenerationRate; }
		set { regenerationRate = value; }
	}
}
