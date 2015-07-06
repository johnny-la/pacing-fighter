using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Governs the character's artificial intelligence 
/// </summary>
public class CharacterAI : MonoBehaviour 
{
	/** Caches the Character that this script is controlling. */
	private Character character;

	/** Stores the target this character is attacking. */
	private Character attackTarget;

	/** Stores the max number of characters that can attack this character at once. Set by the 'AISettings' instance (for the player) */
	private int simultaneousAttackers = 1;

	/** The characters which are currently performing an attack on this character. */
	private List<Character> attackers = new List<Character>(3);

	void Awake () 
	{
		// Cache the Character instance which controls this component. Avoids excessive runtime lookups.
		character = GetComponent<Character>();
	}

	/// <summary>
	/// Adds an attacker to this character's 'attacker' list. This character will now be able to keep track of the characters
	/// he's being attacked by
	/// </summary>
	public void AddAttacker(Character character)
	{
		// If this character cannot be attacked, we cannot add an attacker to his list. Thus, return this function.
		if(!CanBeAttacked())
			return;

		// Add the given character to this character's 'attackers' list
		attackers.Add (character);
	}

	public void RemoveAttacker(Character character)
	{

	}

	/// <summary>
	/// Sets the target that this character is attacking. Allows this character's behavior tree to know which character
	/// he is currently attacking. This signals his behavior tree that he should start attacking the given target
	/// </summary>
	public void SetAttackTarget(Character target)
	{
		// Inform this character that he is currently attacking the given target
		attackTarget = target;
	}

	/// <summary>
	/// Returns true if this character is currently attacking the given target. 
	/// </summary>
	public bool IsAttacking(Character target)
	{
		// If the given target is equal to the character's current 'attackTarget', this character is attacking the given target
		return (target == attackTarget);
	}

	/// <summary>
	/// Returns true if this character can be attacked.	This requires that there are less characters attacking this
	/// character than the value of 'simultaneousAttackers'
	/// </summary>
	public bool CanBeAttacked()
	{
		// If the number of characters attacking this character is below the max, return true since this character can be attacked
		if(attackers.Count < simultaneousAttackers)
			return false;

		// If this statement is reached, this character is already being attacked by too many other characters.
		// Thus, return false, since this character cannot be attacked
		return false;
	}

	/// <summary>
	/// Stores the character this 	/// </summary>
	/// <value>The target.</value>
	public Character Target
	{
		get { return attackTarget; }
		set { this.attackTarget = value; }
	}

	/// <summary>
	/// The max number of people that can attack this character at once. For the player, this is set by the AISettings instance
	/// on game start 
	/// </summary>
	public int SimultaneousAttackers
	{
		get { return simultaneousAttackers; }
		set { simultaneousAttackers = value; }
	}
}
