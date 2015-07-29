using UnityEngine;
using System.Collections;

/// <summary>
/// The component which controls the game camera's movement and behaviour
/// </summary>
public class GameCamera : MonoBehaviour 
{
	/** The camera's z-position. The camera stays locked at this z-position. */
	private const float PositionZ = -10.0f;

	/** The GameCamera's Camera component, which handles game rendering. */
	private Camera camera;

	/** The current mode the camera is set to (combat, exploration, etc.). Changes the camera's movement behaviour. */
	private CameraMode cameraMode;

	/** The higher this constant, the slower the camera moves. */
	public float dampTime = 0.15f;

	/** Stores the Transform that the camera should follow around. */
	private Transform target;

	/** The camera's current velocity. Needed when calling the SmoothDamp() function to move the camera. */
	private Vector3 velocity;

	/** A cached version of the camera's Transform */
	private new Transform transform;
	
	void Awake () 
	{
		// Cache the camera component so that it can be controlled and moved around the world.
		camera = GetComponent<Camera>();

		// TODO: Set this variable using the GameManager to avoid a call to Find().
		target = GameObject.Find ("Player").transform;

		// Caches the camera's Transform for efficiency purposes
		transform = GetComponent<Transform>();
	}

	void FixedUpdate () 
	{
		// The camera's move destination is the position of the 'target' Transform.
		Vector3 destination = target.position;
		// Sets the destination's z-position to the camera's default value
		destination.z = PositionZ;

		// Move the camera to the destination position using Vector2.SmoothDamp()
		transform.position = Vector3.SmoothDamp (transform.position, destination, ref velocity, dampTime);
	}

	/// <summary>
	/// The current mode the camera is set to (combat, exploration, etc.). Changes the camera's movement behaviour. */
	/// </summary>
	public CameraMode CameraMode
	{
		get { return cameraMode; }
		set { cameraMode = value; }
	}

	/// <summary>
	/// Stores the Transform that the camera should follow around. 
	/// </summary>
	public Transform Target
	{
		get { return target; }
		set { this.target = value; }
	}
}

/// <summary>
/// The current mode the camera is set to. The camera works differently depending on their current camera mode.
/// </summary>
public enum CameraMode
{
	Combat
}
