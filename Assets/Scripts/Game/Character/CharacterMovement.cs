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

	void OnEnable()
	{
		// Subscribe the CharacterForces component to the onTargetReached event. This allows the character to know when a force is done being applied
		moveToTargetScript.onTargetReached += character.CharacterForces.OnTargetReached;
	}

	void OnDisable()
	{
		// Unsubscribe the CharacterForces component from the onTargetReached event since the component is disabled.
		moveToTargetScript.onTargetReached -= character.CharacterForces.OnTargetReached;
	}

	protected void FixedUpdate()
	{
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
	/// Moves the character to the given position at his physics walking speed. Can 
	/// be called once when the character wants to start moving. Sets the character
	/// to face the given direction.
	/// </summary>
	public void MoveTo(Vector2 moveTarget, Direction facingDirection)
	{
		// Tell the MoveToTargetScript to move the character to the given position at his walk speeds
		MoveToTargetScript.MoveTo(moveTarget, PhysicsData.DefaultMinWalkSpeed, 
		                          PhysicsData.DefaultMaxWalkSpeed);

		// Update the character's facing direction
		FacingDirection = facingDirection;
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