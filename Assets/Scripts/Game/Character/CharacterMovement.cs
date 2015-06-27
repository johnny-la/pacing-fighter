using UnityEngine;
using System.Collections;

/// <summary>
/// Denotes the direction in which a character is facing
/// </summary>
public enum Direction
{
	Left,
	Right
}

/// <summary>
/// Governs character movement 
/// </summary>
public class CharacterMovement : MonoBehaviour, IMovement
{
	/** The Character instance this movement script controls. */
	private Character character;

	/** The direction in which the character is facing */
	private Direction facingDirection = Direction.Right;

	/* The physics which govern the character's movement. */
	[SerializeField]
	private PhysicsData physicsData;

	/// <summary>
	/// Script which allows a character to move towards a move target
	/// </summary>
	MoveToTarget moveToTargetScript;

	/** Caches the entity's components for efficiency purposes */
	private new Rigidbody2D rigidbody;

	protected void Awake()
	{
		// Cache the Character instance controlling this component. Avoids excessive runtime lookups.
		character = GetComponent<Character>();

		// Adds a new component to the character which allows him to move to target posiitons
		moveToTargetScript = gameObject.AddComponent<MoveToTarget>();

		// Caches the entity's components to efficiently modify their behaviours
		rigidbody = GetComponent<Rigidbody2D>();
	}

	protected void FixedUpdate()
	{
	}

	/// <summary>
	/// Perform the action by moving the character according to the action's forces
	/// </summary>
	public void Play(Action action, GameObject touchedObject, Vector2 touchPosition)
	{
		// Apply each force designated by the given action
		for(int i = 0; i < action.forces.Length; i++)
			ApplyForce(action.forces[i], touchedObject, touchPosition);
	}

	/// <summary>
	/// Applies the given force on this character.
	/// </summary>
	/// <param name="touchedObject">The GameObject targetted by this action</param>
	/// <param name="touchPosition">The touch position when this action was activated</param>
	public void ApplyForce(Force force, GameObject touchedObject, Vector2 touchPosition)
	{
		// Stores the time at which the force will start being applied
		float startTime = 0.0f;
		
		// Determines the time at which the force starts
		switch(force.startTime.type)
		{
		case DurationType.Frame:
			// The start time is specified by the n-th frame of the character's current animation
			startTime = force.startTime.nFrames / CharacterAnimator.FRAME_RATE;
			break;
		case DurationType.WaitForAnimationComplete:
			// The force will start when 'animationToWaitFor' is complete
			startTime = character.CharacterAnimator.GetEndTime(force.startTime.animationToWaitFor);
			break;
		}
		
		// Stores the amount of time the force lasts, in seconds
		float duration = 0.0f;
		
		// Determines the time at which the force starts
		switch(force.duration.type)
		{
		case DurationType.Frame:
			// The duration is specified by the amount of time elapsed from n frames of the character's current animation
			duration = force.duration.nFrames / CharacterAnimator.FRAME_RATE;
			break;
		case DurationType.WaitForAnimationComplete:
			// The force will end when 'animationToWaitFor' is complete
			duration = character.CharacterAnimator.GetEndTime(force.duration.animationToWaitFor) - startTime;
			break;
		}
		
		//if(action.name == "Melee_Player")
		//Debug.Log ("Start: " + startTime + ", Duration: " + duration);
		
		// The position where the force will move the character
		Vector2 targetPosition = Vector2.zero;
		
		// If the force requires to character to move to a designated position
		if(force.forceType == ForceType.Position)
		{
			// Stores the direction in which the character will face once the force is applied. By default, the character keeps his current direction
			Direction newFacingDirection = character.CharacterMovement.FacingDirection;
			
			// If the force requires moving the character to the touched object
			if(force.target == TargetPosition.TouchedObject)
			{
				// Retrieve the Character component from the GameObject the character touched. The character will move towards this character
				Character characterTarget = touchedObject.GetComponent<Character>();
				
				// Stores the target location the player will move to because of this force
				Target targetToMoveTo = Target.None;
				
				//If the character targetted by this action is facing the character performing this action
				if(characterTarget.CharacterMovement.IsFacing(character))
				{
					// Move the character to the front of his target to perform this action
					targetToMoveTo = Target.MeleeFront;
				}
				//Else, if the target character is not facing the player, the character will have to move to the back of his target to perform this action 
				else
				{
					// Move the character to the back of his target to perform this action
					targetToMoveTo = Target.MeleeBack;
				}
				
				// Get the position of the chosen target, which is stored as a child Transform on the character targetted by this action.
				// This is the position that the force will push this character to.
				targetPosition = characterTarget.CharacterTarget.GetTarget (targetToMoveTo);
				
				// If the force makes the character look at his target
				if(force.faceTarget)
				{
					// If the target is to the right of the character, make the character face to the right
					if (characterTarget.Transform.position.x > transform.position.x)
						newFacingDirection = Direction.Right;
					// Else, if the target is to the left of the character, make the character face left
					else
						newFacingDirection = Direction.Left;
				}
			}
			// Else, if the force requires the character to move to the position he touched to activate the force
			else if(force.target == TargetPosition.TouchedPosition)
			{
				// If the force makes the character look at his touch-position target
				if(force.faceTarget)
				{
					// If the character's target position is to the right of him, make him face right
					if (touchPosition.x > transform.position.x)
						newFacingDirection = Direction.Right;
					// Else, if the target position is to the left of the character, make him left
					else
						newFacingDirection = Direction.Left;
				}
				
				// The force should move the character to the touch position which activated this force
				targetPosition = touchPosition;
			}
			
			// Start the coroutine which moves the character to his target position
			StartCoroutine (MoveToCoroutine (targetPosition,startTime,duration,newFacingDirection));
		}
		// If the force applies a constant velocity
		else if(force.forceType == ForceType.Velocity)
		{
			// Start the coroutine which moves the character at the given velocity for the specified amount of time
			StartCoroutine (ApplyVelocityCoroutine (force.velocity,startTime,duration));
		}
	}

	/// <summary>
	/// Moves the character to the given position at his default physics values. Can 
	/// be called once when the character wants to start moving. Only sets state. Does
	/// not actually move the character until the Update() method is called.
	/// </summary>
	public void MoveTo(Vector2 moveTarget)
	{
		// Tell the MoveToTargetScript to move the character to the given position at his walk speeds
		MoveToTargetScript.MoveTo(moveTarget, PhysicsData.DefaultMinWalkSpeed, 
		                          PhysicsData.DefaultMaxWalkSpeed);

		// If the move target is to the right of the character, make him face the right
		if (moveTarget.x > transform.position.x)
			facingDirection = Direction.Right;
		// Else, if the character is moving to the left, make him face to the left
		else
			facingDirection = Direction.Left;
	}

	/// <summary>
	/// Moves the character to the given position in the given amount of time.
	/// The character will face in the direction of the given move target.
	/// Call this method once when the character wants to start moving. Only 
	/// sets state. Does not actually move the character until the Update() 
	/// method is called.
	/// </summary>
	public void MoveTo(Vector2 moveTarget, float time)
	{
		// Delegate the movement call to the 'MoveToTarget' script, which controls moving the character
		MoveToTargetScript.MoveTo (moveTarget, time);

		// If the move target is to the right of the character, make him face the right
		if (moveTarget.x > transform.position.x)
			facingDirection = Direction.Right;
		// Else, if the character is moving to the left, make him face to the left
		else
			facingDirection = Direction.Left;
	}

	/// <summary>
	/// Moves the character to the given position in the given amount of time.
	/// The character will face in the specified direction.
	/// Call this method once when the character wants to start moving. Only 
	/// sets state. Does not actually move the character until the Update() 
	/// method is called.
	/// </summary>
	public void MoveTo(Vector2 moveTarget, float time, Direction facingDirection)
	{
		// Delegate the movement call to the 'MoveToTarget' script, which controls moving the character
		MoveToTargetScript.MoveTo (moveTarget, time);

		// Update the character's facing direction
		FacingDirection = facingDirection;
	}

	/// <summary>
	/// Moves the character at the given velocity for the specified amount of seconds
	/// </summary>
	public void SetVelocity(Vector2 velocity, float duration)
	{
		// Tell the MoveToTargetScript to move the character at the given velocity for a set amount of time 
		MoveToTargetScript.SetVelocity(velocity, duration);
		
		// If the character is moving to the right, make him face the right
		if (velocity.x > 0.0f)
			facingDirection = Direction.Right;
		// Else, if the character is moving to the left, make him face to the left
		else
			facingDirection = Direction.Left;
	}

	/// <summary>
	/// Returns true if the given character is facing the other character, and this character can thus see the other one.
	/// </summary>
	public bool IsFacing(Character other)
	{
		// Caches this character's position
		Vector2 position = character.Transform.position;

		// Caches the properties for the given character
		Vector2 otherPosition = other.Transform.position;
		Direction otherFacingDirection = other.CharacterMovement.FacingDirection;

		//If both this character and the other character are facing right
		if(FacingDirection == Direction.Right && otherFacingDirection == Direction.Right)
		{
			//If the other character is to the right of this character
			if(otherPosition.x > position.x)
				return true;	//Return true, since the character can see the other character.
		}
		//If the character is facing the right, and the character is facing the left
		else if(FacingDirection == Direction.Right && otherFacingDirection == Direction.Left)
		{
			//If this character is to the left of the other character, the character is facing the other character, and can see him.
			if(position.x < otherPosition.x)
				return true;	//Return true, since the character can see the other character.
		}
		else if(FacingDirection == Direction.Left && otherFacingDirection == Direction.Left)
		{
			//If the other character is to the left of the character, the character is facing the other character.
			if(otherPosition.x < position.x)
				return true;	//Return true, since the character can see the other character.
		}
		//Else, if the character is facing the left, and the other character is facing the right
		else if(FacingDirection == Direction.Left && otherFacingDirection == Direction.Right)
		{
			//If the character is to the right of the other character, the character can see the other character.
			if(position.x > otherPosition.x)
				return true;	//Return true, since the character can see the other character.
		}
		
		return false; //If this statement is reached, the character is not facing the other character.
	}

	/// <summary>
	/// Moves the character to given position at the specified times. The character will face the given direction
	/// when 'startTime' seconds have elapsed
	/// </summary>
	private IEnumerator MoveToCoroutine(Vector2 targetPosition, float startTime, float duration, Direction facingDirection)
	{
		// Wait for 'startTime' seconds before applying the force
		yield return StartCoroutine (Wait (startTime));
		
		// Move the character to the designated position in the given amount of time
		character.CharacterMovement.MoveTo (targetPosition, duration, facingDirection);
		
	}
	
	/// <summary>
	/// Apply a constant velocity for a specified amount of time
	/// </summary>
	private IEnumerator ApplyVelocityCoroutine(Vector2 velocity, float startTime, float duration)
	{
		// Wait for 'startTime' seconds before applying the force
		yield return StartCoroutine (Wait (startTime));
		
		// Move the character at the given velocity for the given amount of time
		character.CharacterMovement.SetVelocity (velocity, duration);
	}
	
	/** Waits for the given amount of seconds */
	private IEnumerator Wait(float duration)
	{
		// Yield every frame until the timer reached the designated number of seconds
		for(float timer = 0; timer < duration; timer += Time.deltaTime)
			yield return null;
	}
	
	/// <summary>
	/// The physics which govern the character's movement
	/// </summary>
	public PhysicsData PhysicsData
	{
		get { return physicsData; }
		set { physicsData = value; }
	}

	/// <summary>
	/// The direction in which the character is facing.
	/// This information is mainly used to decide which 
	/// direction to flip the character skeleton
	/// </summary>
	public Direction FacingDirection
	{
		get { return facingDirection; }
		set { facingDirection = value; }
	}

	/// <summary>
	/// The script responsible for moving the character to a target position
	/// </summary>
	public MoveToTarget MoveToTargetScript
	{
		get { return moveToTargetScript; }
		set { moveToTargetScript = value; }
	}
}

/// <summary>
/// Governs the physics controlling a character
/// </summary>
[System.Serializable]
public class PhysicsData
{
	/** The default minimum and maximum walking speeds of a character */
	[SerializeField] private float defaultMinWalkSpeed;
	[SerializeField] private float defaultMaxWalkSpeed;
	
	/// <summary>
	/// The character's default minimum walk speed.	
	/// </summary>
	public float DefaultMinWalkSpeed
	{
		get { return defaultMinWalkSpeed; }
		set { defaultMinWalkSpeed = value; }
	}
	
	/// <summary>
	/// The character's default maximum walk speed.	
	/// </summary>
	public float DefaultMaxWalkSpeed
	{
		get { return defaultMaxWalkSpeed; }
		set { defaultMaxWalkSpeed = value; }
	}
}