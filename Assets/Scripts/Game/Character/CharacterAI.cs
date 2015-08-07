using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;

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

	/** The BehaviorTree controlling the character's AI. May be null. */
	private BehaviorTree behaviorTree;
	/** The target this character will attack when his behavior tree signals him to attack. */
	private SharedTransform behaviorTreeAttackTarget;

	/** The characters which are currently performing an attack on this character. */
	private List<Character> attackers = new List<Character>(3);

	public void Awake () 
	{
		// Cache the Character instance which controls this component. Avoids excessive runtime lookups.
		character = GetComponent<Character>();

		// Cache the BehaviorTree controlling the character's AI. May be null if the character does not have a behavior tree
		behaviorTree = GetComponent<BehaviorTree>();
		
		// If the character has a behavior tree
		if(behaviorTree != null)
		{
			// Retrieve the 'SharedTransform' variable which decides which entity this character should attack when the behavior tree wants him to attack
			behaviorTreeAttackTarget = (SharedTransform) behaviorTree.GetVariable ("AttackTarget");
		}
	}

	void OnEnable()
	{
		// Subscribe to this character's events to be notified of important events 
		character.OnDeath += OnDeath;
	}

	void OnDisable()
	{
		// Unsubscribe from this character's events to avoid errors
		character.OnDeath -= OnDeath;
	}

	/// <summary>
	/// Called when this character is hit by an enemy. This character must then lose his attack target
	/// and remove himself from any attacker list. This is because, once the character is hit, he is 
	/// forced to cancel his attack (if he was performing one).
	/// </summary>
	public void OnHit()
	{
		// If this character was attacking someone when he got hit
		if(attackTarget != null)
		{
			// Remove this character from his target's attacking list. The character is no longer attacking his target since he got hit
			attackTarget.CharacterAI.RemoveAttacker (this.character);
		}

		// Set this character's attack target to null. The character cannot attack anyone when he is hit.
		SetAttackTarget (null);

		// Tell this character to stop moving. When hit, the character should stop moving.
		character.CharacterMovement.MoveToTargetScript.LoseMoveTarget ();
	}

	/// <summary>
	/// Called when this character dies.
	/// </summary>
	private void OnDeath(Character character)
	{
		// Cancel the character's attack target, if he has any. Ensures that the character he is attacking knows that this character is no longer attacking him.
		CancelAttackTarget();
	}

	/// <summary>
	/// Cancel the target this character is attacking. Ensures that the character he is attacking knows that this character is no longer attacking him.
	/// </summary>
	public void CancelAttackTarget()
	{
		// Stores the entity that this character is currently attacking.
		Character currentTarget = character.CharacterAI.CurrentTarget;
		
		// If this character is currently attacking something
		if(currentTarget != null)
		{
			// Inform the character that he has no more attack target. Tells his behaviour tree that he shouldn't attack again
			character.CharacterAI.SetAttackTarget(null);
			// Remove this character from the list of attackers of his previous target. Since this character stopped attacking,
			// the one being attacked should be informed that there is one less character out to attack him.
			currentTarget.CharacterAI.RemoveAttacker (character);
		}
	}

	/// <summary>
	/// Sets the target that this character is attacking. Allows this character's behavior tree to know which character
	/// he is currently attacking. This signals his behavior tree that he should start attacking the given target
	/// </summary>
	public void SetAttackTarget(Character target)
	{
		// Inform this character that he is currently attacking the given target
		attackTarget = target;

		// If the character's AI is controlled by a behavior tree
		//if(behaviorTree != null)
		//{
			// Tell this character's behavior tree to attack the given target the next time the behavior tree decides this character
			// should attack. i.e., when the behavior tree decides this character should attack, the given target will be attacked.
			// Note that, if this character is an enemy, this value is usually set to same attack target as the enemy mob it belongs to.
			//BehaviorTreeAttackTarget = target.Transform;
		//}
	}
	
	/// <summary>
	/// Returns true if this character is currently attacking the given target. 
	/// </summary>
	public bool IsAttacking(Character target)
	{
		// If the given target is equal to the character's current 'attackTarget', this character is attacking the given target
		if(target == attackTarget && !attackTarget.CharacterStats.IsDead())
			return true;

		// If this statement is reached, this character is not attacking the given target. Thus, return false
		return false;
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

	/// <summary>
	/// Removes the given attacker from this character's list of attackers. This lets this character keep
	/// track of the people that are attacking him.  
	/// </summary>
	/// <param name="character">Character.</param>
	public void RemoveAttacker(Character character)
	{
		// Remove the given character from this character's list of attackers. Informs this character he has one less person attacking him
		attackers.Remove (character);
	}

	/// <summary>
	/// Returns true if this character can be attacked.	This requires that there are less characters attacking this
	/// character than the value of 'simultaneousAttackers'
	/// </summary>
	public bool CanBeAttacked()
	{
		// If the number of characters attacking this character is below the max, return true since this character can be attacked
		if(attackers.Count < simultaneousAttackers)
			return true;

		// If this statement is reached, this character is already being attacked by too many other characters.
		// Thus, return false, since this character cannot be attacked
		return false;
	}

	/// <summary>
	/// Stores this character's current attacking target. If this is non-null, this character is attacking
	/// the character returned by this property. 
	/// </summary>
	public Character CurrentTarget
	{
		get { return attackTarget; }
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

	/// <summary>
	/// The behavior tree controlling this character's AI. May be null if the character has no BehaviorTree component attached.
	/// </summary>
	public BehaviorTree BehaviorTree
	{
		get { return behaviorTree; }
		set { behaviorTree = value; }
	}

	/// <summary>
	/// The target this character will attack when his behavior tree signals him to attack.
	/// If the character is an enemy, even if he is not attacking anyone, he will move towards this 
	/// target to prepare for a potential attack.
	/// </summary>
	public Transform BehaviorTreeAttackTarget
	{
		get { return behaviorTreeAttackTarget.Value; }
		set { behaviorTreeAttackTarget.Value = value; }
	}
}
