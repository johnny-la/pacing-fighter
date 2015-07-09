using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// Tells the character (that is controlled by this action) to stop attacking the entity he is currently attacking
/// If the character is currently not attacking anything, this action returns Failure.
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

			// Return success, since this character has stopped attacking his current target
			return TaskStatus.Success;
		}
		// Else, if this character is currently not attacking anything
		else
		{
			// Return failure, since the character can't stop an attack he is not performing
			return TaskStatus.Failure;
		}
	}
}
