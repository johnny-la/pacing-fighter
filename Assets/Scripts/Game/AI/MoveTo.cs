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

public class MoveTo : Action 
{
	/** The type of target the character will want to move towards */
	public TargetType targetType;

	/** The maximum distance that the GameObject can be from his target before stopping */
	public float stoppingDistance;

	/** The Transform the GameObject wants to move to */
	public Transform target;

	/** Holds the script used to make the entity move to a desired location. */
	private CharacterMovement characterMovement;

	/** Stores the squared distance from the move target before the GameObject stops moving. */
	private float stoppingDistanceSquared;

	/** Helper Vector2 to avoid initialization. */
	private Vector2 helperVector2 = Vector2.zero;

	public override void OnAwake()
	{
		// Cache the MoveToTarget script to allow the character to move to a desired location
		characterMovement = transform.GetComponent<CharacterMovement>();
		
		// Square the stopping distance and cache it to reduce runtime multiplication count
		stoppingDistanceSquared = stoppingDistance * stoppingDistance;
	}

	public override void OnStart()
	{
	}

	public override TaskStatus OnUpdate()
	{
		// Set the character's move target to the given Transform's position
		characterMovement.MoveTo (target.position);

		// Caches the GameObject's and the target's position
		Vector2 position = transform.position;
		Vector2 targetPosition = target.position;

		// Calculate the distance vector from the GameObject to his target
		helperVector2.Set ( targetPosition.x - position.x, targetPosition.y - position.y );

		// Compute the squared distance between the GameObject and his move target
		float distanceSquared = helperVector2.SqrMagnitude ();

		// If the GameObject is within stopping distance of his target
		if(distanceSquared <= stoppingDistanceSquared)
		{
			// The GameObject has reached his destination. Thus, make him stop moving towards his target.
			characterMovement.MoveToTargetScript.LoseMoveTarget ();
			// Return Success. The GameObject has successfully reached his move target
			return TaskStatus.Success;
		}

		// The GameObject is still moving towards his target. Thus, the action is still running
		return TaskStatus.Running;
	}
	
}
