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

	/** Called when this entity has reached his move target */
	public delegate void OnTargetReached();
	/** Notifies components like CharacterMovement that the character has reached his move target */
	public event OnTargetReached onTargetReached;

	/** Caches the entity's components for efficiency */
	private new Transform transform;
	private new Rigidbody2D rigidbody;

	/** Layer mask used when MoveTo() is called. If there is an obstacle is in the way between this object 
	 *  and his move target, the target is truncated. */
	private int obstacleLayerMask = (1 << Brawler.Layer.Obstacle);

	/** The GameObject's position the previous frame. */
	private Vector2 previousPosition = Vector2.zero;
	/** Helper vector2 to avoid allocations */
	private Vector2 helperVector2 = Vector2.zero;

	public void Awake()
	{
		// Caches the entity's componenets to efficiently modify the GameObject's physics behaviour
		transform = GetComponent<Transform>();
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
				// Inform subscribers to 'onTargetReached' that this entity has reached his move target
				if(onTargetReached != null)
					onTargetReached();

				// Snap the entity to his move target to ensure he stops at the exact position of his target
				// transform.position = moveTarget;

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
		// Compute the speed required for the character to reach his move target
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
		// If the character's move target has changed, draw a new raycast to test for movement collisions
		if(moveTarget != this.moveTarget)
		{
			// Computes the direction of the raycast. It will go from the character's position, towards the move
			// target. If obstacles are hit, the move target is truncated to the position where the ray hit an obstacle.
			Vector2 raycastDirection = moveTarget - (Vector2)transform.position;
			// Shoots a raycast from this GameObject's position to the move target. If an obstacles is hit, the move 
			// target is truncated to the point of intersection
			RaycastHit2D hit = Physics2D.Raycast (transform.position, raycastDirection, raycastDirection.magnitude, obstacleLayerMask); 

			// If the raycast from this GameObject's position to the move target hit an obstacle
			if(hit.transform != null)
			{
				// Set the move target to stop at the point of intersection between the move target and the obstacle.
				//this.moveTarget.Set (hit.point.x, hit.point.y);

				//Debug.Log ("Hit the object: " + hit.transform + " at position " + hit.point);
			}
			// Else, if this entity can make it to the move target without hitting obstacles
			else
			{
				// Sets the entity's move target to the given Vector2
				this.moveTarget.Set(moveTarget.x, moveTarget.y);
			}

			this.moveTarget.Set(moveTarget.x, moveTarget.y);

		}

		// Update the minimum and maximum speed at which the entity can move to his target
		this.minTravelSpeed = minTravelSpeed;
		this.maxTravelSpeed = maxTravelSpeed;

		// The move target is not yet reached
		MoveTargetReached = false;
		// Tells the entity he has a move target
		HasMoveTarget = true;
	}

	/// <summary>
	/// Move this GameObject to the given position in the given amount of time.
	/// </summary>
	public void MoveTo(Vector2 moveTarget, float time)
	{
		// Sets the entity's move target
		this.moveTarget.Set(moveTarget.x, moveTarget.y);

		// Computes the distance between this GameObject and his target position
		float distance = Vector2.Distance(transform.position, moveTarget);
		// Calculate the speed at which the entity must move
		float speed = distance / time;
		
		// Tell the entity to move at a constant speed. This speed is determined by the velocity argument
		this.minTravelSpeed = this.maxTravelSpeed = speed;
		
		// The move target is not yet reached
		MoveTargetReached = false;
		// Tells the entity he has a move target
		HasMoveTarget = true;
	}

	/// <summary>
	/// Set the GameObject to move at the given velocity for the given amount of time.
	/// </summary>
	public void SetVelocity(Vector2 velocity, float time)
	{
		// Stores the distance entity will travel after the given amount of time
		Vector2 moveDistance = velocity * time;

		// Computes the position of the character after he is done moving
		Vector2 targetPosition = (Vector2)transform.position + moveDistance;

		// Sets the entity's move target to the target position computed avoce
		this.moveTarget.Set(targetPosition.x, targetPosition.y);

		// Calculate the speed at which the entity must move
		float speed = velocity.magnitude;

		// Tell the entity to move at a constant speed. This speed is determined by the velocity argument
		this.minTravelSpeed = this.maxTravelSpeed = speed;
		
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
		HasMoveTarget = false;
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
		// Stores true if the entity has passed his move target. This is done
		// by comparing his current position and his position last frame to the
		// position of his move target
		bool passedTarget = ((previousPosition.x >= target.x && position.x <= target.x) || (position.x >= target.x && previousPosition.x <= target.x)) 
						 && ((previousPosition.y >= target.y && position.y <= target.y) || (position.y >= target.y && previousPosition.y <= target.y));
		// Stores true if the entity's position and his target position is 
		// approximately equal, given a small epsilon
		bool reachedTarget = (Mathf.Abs (position.x - target.x) <= 0.1f) && (Mathf.Abs (position.y - target.y) <= 0.1f);

		// If this GameObject has passed or reached his target, return true, since the 
		// target has been reached. Otherwise, return false.
		return passedTarget || reachedTarget;
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
		set { this.hasMoveTarget = value; }
	}
}
