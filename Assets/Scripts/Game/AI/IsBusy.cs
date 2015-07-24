using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;

public class IsBusy : Conditional
{
	/** The Character script belonging to the GameObject controlled by this behavior tree */
	private Character character;

	public override void OnAwake()
	{
		// Caches the Character component for the GameObject this behavior tree is acting on
		character = transform.GetComponent<Character>();
	}

	public override TaskStatus OnUpdate()
	{
		// Stores true if the character is performing an action that can't be cancelled.
		bool performingImportantAction = (character.CharacterControl.CurrentAction != null)
			&& (character.CharacterControl.CurrentAction.cancelable == false);

		// If the character is performing an action that is not cancelable, the character is busy.
		// OR if the character is dead, he is also busy.
		// In either case, return success, since the character is busy.
		if(performingImportantAction || character.CharacterStats.IsDead ())
			return TaskStatus.Success;

		// If this statement is reached, this character is not busy performing a move or dying. Thus, return failure
		return TaskStatus.Failure;
	}
}
