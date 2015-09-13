using UnityEngine;
using System.Collections;


/// <summary>
/// Adds depth to sprites. That is, the sprites lower down the screen will appear in front of sprites
/// that are higher on top of the screen
/// </summary>

public class SpriteDepth : MonoBehaviour 
{
	/** Stores the amount the y-value of this sprite is divided by. The resultant value is set at the sprite's z-value */
	private readonly float Y_TO_DEPTH = 10.0f;

	[Tooltip("A z-offset used to ensure that some GameObjects are in front of others if they are at the same y-position.")]
	public float zOffset;

	/** Caches this GameObject's Transform for efficiency purposes. */
	private new Transform transform;

	/** Helper Vector2 used to avoid instantiation. */
	private Vector3 helperPosition = Vector3.zero;
	
	void Awake () 
	{
		// Cache this entity's Transform for performance reasons
		transform = GetComponent<Transform>();
	}

	void FixedUpdate () 
	{
		// Update the sprite's depth in the world
		UpdateDepth();
	}

	/// <summary>
	/// Updates the sprite's z-position so that, the higher the sprite is in the y-axis, the further back the sprite is rendered.
	/// </summary>
	public void UpdateDepth()
	{
		// Set the Transform's z-position to this transform's y-position divided by a constant. Like this,
		// the higher up the sprite is, the further down the screen he is. As such, the sprite will appear
		// behind the sprites lower down on the y-axis
		helperPosition.Set (transform.position.x, transform.position.y, (transform.position.y / Y_TO_DEPTH) + zOffset);
		transform.position = helperPosition;
	}
}
