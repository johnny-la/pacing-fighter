using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;

public class CanAttack : Conditional
{
	/** The target character being attacked. */
	public Character target;

	/** The Character script belonging to the GameObject controlled by this behavior tree */
	private Character character;
	
	public override void OnAwake()
	{
		// Caches the Character component for the GameObject this behavior tree is controlling
		character = transform.GetComponent<Character>();

		Debug.Log ("Zombie character instance: " + character);
	}
	
	public override TaskStatus OnUpdate()
	{
		// If this character is attacking his target
		if(character.CharacterAI.IsAttacking(target))
			// Return success, since this character can attack (and is currently attacking) his target
			// The character's current target is set by the AISettings instance.
			return TaskStatus.Success;
		
		// If this statement is reached, this character cannot attack his target. Thus, return failure
		return TaskStatus.Failure;
	}
}
