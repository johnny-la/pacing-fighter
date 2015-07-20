using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// Makes the character performing this action face the given target.
/// </summary>

public class FaceTarget : BehaviorDesigner.Runtime.Tasks.Action
{
	[BehaviorDesigner.Runtime.Tasks.Tooltip("The Transform that the character will face")]
	public SharedTransform target;

	/** The character component attached to the GameObject performing this action. */
	private Character character;

	public override void OnAwake()
	{
		// Cache the character component belonging to the character performing this action.
		character = transform.GetComponent<Character>();
	}

	public override TaskStatus OnUpdate()
	{
		// If the target is to the right of the character
		if(target.Value.position.x > transform.position.x)
		{
			// Make this character face to the right
			character.CharacterMovement.FacingDirection = Direction.Right;
		}
		// Else, if the character's target is to the left of him
		else
		{
			// Make the character face to the left.
			character.CharacterMovement.FacingDirection = Direction.Left;
		}

		return TaskStatus.Running;
	}

}
