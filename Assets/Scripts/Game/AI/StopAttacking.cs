using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// Tells the character (that is controlled by this action) to stop attacking the entity he is currently attacking
/// Always returns success.
/// </summary>
public class StopAttacking : BehaviorDesigner.Runtime.Tasks.Action
{
	/** Stores the character instance that is performing this action. */
	private Character character;
	
	public override void OnAwake () 
	{
		// Caches the Character component for the GameObject being controlled by this behaviour tree
		character = transform.GetComponent<Character>();
	}

	public override TaskStatus OnUpdate () 
	{
		// Cancels the character's current attack target. Ensures that this character gets removed from his target's attacker list
		character.CharacterAI.CancelAttackTarget();

		return TaskStatus.Success;
	}
}
