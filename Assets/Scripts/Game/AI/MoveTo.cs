using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// The target to which the character will want to move towards
/// </summary>
public enum TargetType
{
	Player,
	Enemy
}

public class MoveTo : BehaviorDesigner.Runtime.Tasks.Action
{
	/** The type of target the character will want to move towards */
	public TargetType targetType;

	/** The maximum distance that the GameObject can be from his target before stopping */
	public float stoppingDistance;

	/** The target character this entity wants to move to. */
	public Character characterTarget;

	/** Holds the character script attached to the entity performing this action. */
	private Character character;

	/** The target this character should move to. This is an enumeration constant refering to a 
	    position on another character. */
	private Target targetToMoveTo;

	/** Stores the squared distance from the move target before the GameObject stops moving. */
	private float stoppingDistanceSquared;

	/** Helper Vector2 to avoid initialization. */
	private Vector2 helperVector2 = Vector2.zero;

	public override void OnAwake()
	{
		// Cache the MoveToTarget script to allow the character to move to a desired location
		character = transform.GetComponent<Character>();
		
		// Square the stopping distance and cache it to reduce runtime multiplication count
		stoppingDistanceSquared = stoppingDistance * stoppingDistance;
	}

	public override void OnStart()
	{
		// Stores the target this character must move to in order to reach 'characterTarget'
		targetToMoveTo = character.CharacterTarget.GetWalkTargetTo (characterTarget);
	}

	public override TaskStatus OnUpdate()
	{
		// Stores the target position this character must move to in order to reach 'characterTarget'
		Vector2 targetPosition = characterTarget.CharacterTarget.GetTargetPosition (targetToMoveTo);

		// Caches this GameObject's position
		Vector2 position = transform.position;

		// The direction this character will face whilst walking.
		Direction facingDirection = character.CharacterMovement.FacingDirection;

		// Make this character face the same direction as his target character.
		if(characterTarget.Transform.position.x > position.x)
			facingDirection = Direction.Right;
		else
			facingDirection = Direction.Left;
		
		// Set the character's move target to the given Transform's position
		character.CharacterMovement.MoveTo(targetPosition, facingDirection);

		// Calculate the distance vector from the GameObject to his target
		helperVector2.Set ( targetPosition.x - position.x, targetPosition.y - position.y );

		// Compute the squared distance between the GameObject and his move target
		float distanceSquared = helperVector2.SqrMagnitude ();

		// If the GameObject is within stopping distance of his target
		if(distanceSquared <= stoppingDistanceSquared)
		{
			// The GameObject has reached his destination. Thus, make him stop moving towards his target.
			character.CharacterMovement.MoveToTargetScript.LoseMoveTarget ();
			// Return Success. The GameObject has successfully reached his move target
			return TaskStatus.Success;
		}

		// The GameObject is still moving towards his target. Thus, the action is still running
		return TaskStatus.Running;
	}
	
}
