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
	/** The direction in which the character is facing */
	private Direction facingDirection = Direction.Right;

	/* The physics which govern the character's movement. */
	[SerializeField]
	private PhysicsData physicsData;

	/// <summary>
	/// Script which allows a character to move towards a move target
	/// </summary>
	MoveToTarget moveToTargetScript;

	/** Caches the entity's Rigidbody for efficiency */
	private new Rigidbody2D rigidbody;

	protected void Awake()
	{
		// Adds a new component to the character which allows him to move to target posiitons
		moveToTargetScript = gameObject.AddComponent<MoveToTarget>();

		// Caches the entity's Rigidbody to efficiently modify its physics behaviour
		rigidbody = GetComponent<Rigidbody2D>();
	}

	protected void FixedUpdate()
	{
	}

	/// <summary>
	/// Informs the character to move to the given position. Can be called once when
	/// the character wants to start moving. Only sets state. Does not actually move
	/// the character.
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