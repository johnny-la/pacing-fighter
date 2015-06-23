using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Denotes a collider which can inflict or receive damage
/// </summary>
[System.Serializable]
public class HitBox
{
	/** The name of the Spine bone to which the hit box is attached */
	public string boneName;
	/** The position offset relative to the Spine bone this box is attached to. */
	public Vector2 offset;
	/** The dimensions of the hit box. */
	public Vector2 size;
	
	/** The GameObject to which this HitBox is attached */
	private GameObject gameObject;
	/** The character to which this hit box belongs. */
	private Character character;
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
	/// The collider which allows the physics engine to detect collisions
	/// </summary>
	public BoxCollider2D Collider
	{
		get { return collider; }
		set { this.collider = value; }
	}
}