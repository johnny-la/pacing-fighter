using UnityEngine;
using System.Collections;

public class CharacterStats : MonoBehaviour 
{
	/** The Character instance controlling this script. */
	private Character character;

	/** The character's default stats */
	public float defaultHealth;
	public float defaultStrength;
	public float defaultDefense;

	/** The character's current amount of health. */
	private float health;
	/** The character's strength stat */
	private float strength;
	/** The character's defense stat */
	private float defense;

	/** The number of consecutive hits the character has performed. */
	private int combo;

	void Awake () 
	{
		// Cache the Character instance which controls this component. Avoids excessive runtime lookups.
		character = GetComponent<Character>();

		// Set up the character's stats based on his defaults
		health = defaultHealth;
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
		float damage = hitInfo.baseDamage + (strength / adversary.CharacterStats.defense);

		// Inform the AIDirector that the adversary hit this character with the damage computed above.
		AIDirector.Instance.OnDamageDealt(adversary, character, damage);

		// Inflict damage to the character to which this component belongs
		TakeDamage (damage);
	}

	/// <summary>
	/// Inflict the given amount of damage to the player
	/// </summary>
	public void TakeDamage(float amount)
	{
		// Decrement the character's health by the given amount
		health -= amount;
		
		Debug.Log (character.name + " takes damage: -" + amount + ", New health: " + health); 
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
		return character.name + " Health: " + health + " Strength: " + strength + " Defense: " + defense;
	}

	/// <summary>
	/// The character's current amount of health.
	/// </summary>
	public float Health
	{
		get { return this.health; }
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
