using UnityEngine;
using System.Collections;

/// <summary>
/// Called when this character deals damage to another character. Note that the player's AnxietyMonitor listens to this event. 
/// The more the player deals damage, the higher his anxiety.
/// </summary>
public delegate void OnDealDamageHandler(float damage, Character character, Character adversary);

public class CharacterStats : MonoBehaviour 
{
	/** The Character instance controlling this script. */
	private Character character;

	/** The character's default stats */
	public float defaultHealth;
	public float defaultStrength;
	public float defaultDefense;

	/** The character's current amount of health. */
	private float currentHealth;

	/** The max amount of health the character can have. */
	private float maxHealth;
	/** The character's strength stat */
	private float strength;
	/** The character's defense stat */
	private float defense;

	/** The number of consecutive hits the character has performed. */
	private int combo;
	
	/// <summary>
	/// Called when this character deals damage to another character. Note that the player's AnxietyMonitor listens to this event. 
	/// The more the player deals damage, the higher his anxiety.
	/// </summary>
	public event OnDealDamageHandler OnDealDamage;

	void Awake () 
	{
		// Cache the Character instance which controls this component. Avoids excessive runtime lookups.
		character = GetComponent<Character>();

		// Set up the character's stats based on his defaults
		currentHealth = defaultHealth;
		maxHealth = defaultHealth;
		strength = defaultStrength;
		defense = defaultDefense;
	}

	/// <summary>
	/// Called when an adversarial character hit this character. Inflicts damage on this character
	/// according to the given HitInfo instance and the adversary's stats
	/// </summary>
	public void OnHit(HitInfo hitInfo, Character adversary)
	{
		// Performs a damage formula which determines the amount of damage inflicted by the hit
		float damage = hitInfo.baseDamage + (adversary.CharacterStats.strength / defense);

		// Call the adversary's OnDealDamage event. This informs subscribers (e.g., AnxietyMonitor) that the adversary has dealt damage to this character.
		if(adversary.CharacterStats.OnDealDamage != null)
			adversary.CharacterStats.OnDealDamage(damage, adversary, character);

		// Inflict damage to the character to which this component belongs
		TakeDamage (damage);
	}

	/// <summary>
	/// Inflict the given amount of damage to the player
	/// </summary>
	public void TakeDamage(float amount)
	{
		// Decrement the character's health by the given amount
		currentHealth -= amount;
		
		Debug.Log (character.name + " takes damage: -" + amount + ", New health: " + currentHealth); 
	}
	
	/// <summary>
	/// Increments the character's current combo.
	/// </summary>
	public void IncrementCombo()
	{
		// Increment the number of consecutive hits the character has performed
		combo++;
	}

	/// <summary>
	/// Resets the character's combo to zero.
	/// </summary>
	public void ResetCombo()
	{
		combo = 0;
	}

	/// <summary>
	/// Returns true if this character is dead. That is, the character's health is below zero.
	/// </summary>
	public bool IsDead()
	{
		return Health <= 0;
	}

	public string ToString()
	{
		return character.name + " Health: " + currentHealth + " Strength: " + strength + " Defense: " + defense;
	}

	/// <summary>
	/// The character's current amount of health.
	/// </summary>
	public float Health
	{
		get { return this.currentHealth; }
	}

	/// <summary>
	/// The percentage of health remaining for the character. For instance, if this character's max health is 200, and 
	/// his current health is 100, 'HealthPercent' is equal to 0.5f.
	/// </summary>
	public float HealthPercent
	{
		get { return currentHealth / maxHealth; }
		set 
		{ 
			// Log an error if the given value is not a correct percentage
			if(value < 0 || value > 1)
				Debug.LogError("Character (" + name + ") health percentage set to illegal value: " + value);

			// Set the character's health to the given percentage
			currentHealth = value * maxHealth; 
		}
	}

	/// <summary>
	/// Return the max amount of health this character can have.
	/// </summary>
	public float MaxHealth
	{
		get { return this.maxHealth; }
	}

	/// <summary>
	/// The character's current strength stat.
	/// </summary>
	public float Strength
	{
		get { return this.strength; }
		set { this.strength = value; }
	}

	/// <summary>
	/// The character's defense stat.
	/// </summary>
	public float Defense
	{
		get { return this.defense; }
		set { this.defense = value; }
	}

	/// <summary>
	/// The character's current combo. This corresponds to the number of successful, consecutive
	/// hits the character has landed within a short time window. 
	/// </summary>
	public int Combo
	{
		get { return combo; }
		set { combo = value; }
	}
}
