using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;

public class CanAttack : Conditional
{
	/** The target character being attacked. */
	[BehaviorDesigner.Runtime.Tasks.Tooltip("The character this Conditional node is asking if this character can attack")]
	public SharedTransform target;

	/** A cached version of the Character component for the entity this script is asking to attack. */
	private Character characterTarget;

	/** The Character script belonging to the GameObject controlled by this behavior tree */
	private Character character;
	
	public override void OnAwake()
	{
		// Caches the Character component for the GameObject this behavior tree is controlling
		character = transform.GetComponent<Character>();
	}

	public override void OnStart()
	{
		// Caches the Character component for the character this script is asking to attack
		characterTarget = target.Value.GetComponent<Character>();
	}
	
	public override TaskStatus OnUpdate()
	{
		// If this character is attacking his target
		if(character.CharacterAI.IsAttacking(characterTarget))
			// Return success, since this character can attack (and is currently attacking) his target
			// The character's current target is set by the AISettings instance.
			return TaskStatus.Success;
		
		// If this statement is reached, this character cannot attack his target. Thus, return failure
		return TaskStatus.Failure;
	}
}
