using System;
using UnityEngine;

public class MoveToTarget : MonoBehaviour
{
	/** The speed at which the entity moves towards his move target */
	private Vector2 travelVelocity = Vector2.zero;

	/** The minimum and maximum speeds at which the entity moves to his target */
	private float minTravelSpeed, maxTravelSpeed;

	/** The target position where the entity is moving towards */
	private Vector2 moveTarget = Vector2.zero;
	/** True if the entity has reached his move target */
	private bool moveTargetReached;
	/** True if hte player has a move target */
	private bool hasMoveTarget;

	/** Caches the entity's Rigidbody for efficiency */
	private new Rigidbody2D rigidbody;

	/** The GameObject's position the previous frame. */
	private Vector2 previousPosition = Vector2.zero;
	/** Helper vector2 to avoid allocations */
	private Vector2 helperVector2 = Vector2.zero;

	public void Awake()
	{
		// Caches the entity's Rigidbody to efficiently modify its physics behaviour
		rigidbody = GetComponent<Rigidbody2D>();
	}

	public void FixedUpdate()
	{
		// If the character has a move target
		if (HasMoveTarget)
		{
			//  If the character has reached his move target
			if (Reached(MoveTarget))
			{
				// Make the character stop following his move target, since he has reached it
				LoseMoveTarget();
			}
			// Else, if the character still hasn't reached his move target
			else
			{
				// Make the character follow his move target
				FollowMoveTarget();
			}
		}

		// Keep a record of the character's previous position to determine if 
		// he has reached his move target next frame
		previousPosition = transform.position;
	}

	/// <summary>
	/// Makes the entity follow his move target
	/// </summary>
	private void FollowMoveTarget()
	{
		ComputeWalkSpeed(moveTarget);

		// Update the entity's Rigidbody to move to his target at his pre-computed velocity 
		rigidbody.velocity = travelVelocity;
	}

	private void ComputeWalkSpeed(Vector2 targetPosition)
	{
		Vector2 vector = base.transform.position;

		helperVector2.y = targetPosition.y - vector.y;
		helperVector2.x = targetPosition.x - vector.x;

		float distanceSquared = helperVector2.SqrMagnitude();
		float angle = Mathf.Atan2(targetPosition.y - vector.y, targetPosition.x - vector.x);

		float percentSpeed = distanceSquared / PlayerConstants.MOVE_TARGET_DASH_DISTANCE;

		// Clamp the player's percentage of his max speed to 100%
		if (percentSpeed >= 1f)
			percentSpeed = 1f;

		// Compute the x and y speeds at which to move towards the player
		float totalSpeed = minTravelSpeed + percentSpeed * (maxTravelSpeed - minTravelSpeed);
		float speedX = totalSpeed * Mathf.Cos(angle);
		float speedY = totalSpeed * Mathf.Sin(angle);

		//Debug.Log ("Character: " + this.name + " Speed: " + totalSpeed);

		// Update the Character's walking velocity
		travelVelocity.Set(speedX, speedY);
	}

	/// <summary>
	/// Sets the entity's move target to the given world position. The entity
	/// will start moving towards this target at a speed between the given min 
	/// and max travel speeds. The speed at which the entity moves is determined
	/// by the distance between the entitiy and his move target. The further away
	/// the target, the faster the entity travels.
	/// </summary>
	public void MoveTo(Vector2 moveTarget, float minTravelSpeed, float maxTravelSpeed)
	{
		// Sets the entity's move target
		this.moveTarget.Set(moveTarget.x, moveTarget.y);

		// Update the minimum and maximum speed at which the entity can move to his target
		this.minTravelSpeed = minTravelSpeed;
		this.maxTravelSpeed = maxTravelSpeed;

		// The move target is not yet reached
		MoveTargetReached = false;
		// Tells the entity he has a move target
		hasMoveTarget = true;
	}

	/// <summary>
	/// Cancels the entity's move target. The entity will stop moving and 
	/// his move target will be ignored.
	/// </summary>
	public void LoseMoveTarget()
	{
		MoveTargetReached = true;
		hasMoveTarget = false;
		travelVelocity.Set(0f, 0f);
		rigidbody.velocity = travelVelocity;
	}

	/// <summary>
	/// Returns true if the player has reached the given position
	/// </summary>
	public bool Reached(Vector2 target)
	{
		// Caches the entity's position
		Vector2 position = transform.position;
		// Returns true if the entity has passed his move target. This is done
		// by comparing his current position and his position last frame to the
		// position of his move target
		return ((previousPosition.x >= target.x && position.x <= target.x) || (position.x >= target.x && previousPosition.x <= target.x)) 
			&& ((previousPosition.y >= target.y && position.y <= target.y) || (position.y >= target.y && previousPosition.y <= target.y));
	}

	/// <summary>
	/// The minimum speed at which the entity can move to his target
	/// </summary>
	public float MinTravelSpeed
	{
		get { return minTravelSpeed; }
		set { minTravelSpeed = value; }
	}

	/// <summary>
	/// The maximum speed at which the entity can move to his target
	/// </summary>
	public float MaxTravelSpeed
	{
		get { return maxTravelSpeed; }
		set { maxTravelSpeed = value; }
	}

	/// <summary>
	/// The position to which the character is walking to
	/// </summary>
	public Vector2 MoveTarget
	{
		get { return moveTarget; }
	}
	
	/// <summary>
	/// True if the GameObject has reached its move target 	
	/// </summary>
	public bool MoveTargetReached
	{
		get { return moveTargetReached; }
		set { moveTargetReached = value; }
	}
	
	/// <summary>
	/// True if the character has a move target.
	/// </summary>
	public bool HasMoveTarget
	{
		get { return hasMoveTarget; }
	}
}
