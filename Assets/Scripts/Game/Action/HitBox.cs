using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Denotes a collider which can inflict or receive damage
/// </summary>
[System.Serializable]
public class HitBox
{
	/** Denotes the type of this hit box. Is it a standard hit box, attached to a spine bone, or
	 *  does it activate at a specified frame and automatically hit its opponent? */
	public HitBoxType hitBoxType;

	/** The name of the Spine bone to which the hit box is attached */
	public string boneName;
	/** The position offset relative to the Spine bone this box is attached to. */
	public Vector2 offset;
	/** The dimensions of the hit box. */
	public Vector2 size;
	
	/** The frame at which the HitBox hits its specified target. Only used if 'hitBoxType = ForceHit'.
	 *  There is one element for each AnimationSequence in the Action which activated this HitBox. That
	 *  is, if the Action which created this HitBox has 4 possible animation sequences, this array will 
	 *  have 4 elements. The i-th element corresponds to the frame when the hit box activates
	 *  when the i-th animation sequence is selected. */
	public int[] hitFrames = new int[0];
	
	/** The GameObject to which this HitBox is attached */
	private GameObject gameObject;
	/** The character to which this hit box belongs. */
	private Character character;
	/** The action which activated this hit box. Allows the hit box to perform the appropri*/
	private Action action;
	/** The collider which allows the physics engine to detect collisions */
	private BoxCollider2D collider;
	
	/// <summary>
	/// Enables the HitBox. The HitBox can now collide with other hit boxes
	/// </summary>
	public void Enable()
	{
		if(collider != null)
			// Enable the hit box's collider to detect collisions
			collider.enabled = true;
	}
	
	/// <summary>
	/// Disable the HitBox. The HitBox can no longer collide with other hit boxes
	/// </summary>
	public void Disable()
	{
		if(collider != null)
			// Disable the hit box's collider to stop detecting collisions
			collider.enabled = false;
	}

	/// <summary>
	/// The GameObject to which this HitBox is attached
	/// </summary>
	public GameObject GameObject
	{
		get { return gameObject; }
		set { this.gameObject = value; }
	}

	/// <summary>
	/// The character to which this hit box belongs.
	/// </summary>
	public Character Character
	{
		get { return character; }
		set { this.character = value; }
	}

	/// <summary>
	/// The action which activated this HitBox. When this hit box hits another object, this action
	/// will dictate what sounds to play, along with any other actions that need to be performed.
	/// </summary>
	public Action Action
	{
		get { return action; }
		set { this.action = value; }
	}

	/// <summary>
	/// The collider which allows the physics engine to detect collisions
	/// </summary>
	public BoxCollider2D Collider
	{
		get { return collider; }
		set { this.collider = value; }
	}
}

/// <summary>
/// Denotes the type of a hit a box. Is it a standard box collider, or does it hit its adversary at a specific
/// frame without requiring hit detection?
/// </summary>
public enum HitBoxType
{
	/** Box collider attached to Spine bone */
	Standard,	
	/** Automatically hits its target at a specified time */
	ForceHit	
}
	