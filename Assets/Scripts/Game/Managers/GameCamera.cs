using UnityEngine;
using System.Collections;

/// <summary>
/// The component which controls the game camera's movement and behaviour
/// </summary>
public class GameCamera : MonoBehaviour 
{
	/** The camera's z-position. The camera stays locked at this z-position. */
	private const float PositionZ = -10.0f;

	/** The GameCamera's Camera/tk2dCamera components, which handles game rendering. */
	private Camera camera;
	private tk2dCamera camera2d;

	/** The current mode the camera is set to (combat, exploration, etc.). Changes the camera's movement behaviour. */
	private CameraMode cameraMode;

	/** The higher this constant, the slower the camera moves. */
	public float dampTime = 0.15f;

	/** Stores the Transform that the camera should follow around. */
	private Transform targetTransform;

	/** The position the camera is moving towards. */
	private Vector2 targetPosition;

	/** The zooming factor that the camera is trying to get to. */
	private float targetZoom;

	/** The speed at which the camera zooms. */
	private float cameraSpeed;

	/** The camera's current velocity. Needed when calling the SmoothDamp() function to move the camera. */
	private Vector3 velocity;

	/** A cached version of the camera's Transform */
	private new Transform transform;
	
	void Awake () 
	{
		// Cache the camera components so that they can be controlled and moved around the world.
		camera = GetComponent<Camera>();
		camera2d = GetComponent<tk2dCamera>();

		// TODO: Set this variable using the GameManager to avoid a call to Find().
		targetTransform = GameObject.Find ("Player").transform;

		// Caches the camera's Transform for efficiency purposes
		transform = GetComponent<Transform>();
	}

	void FixedUpdate () 
	{
		// Stores the camera's target destination
		Vector3 destination = Vector3.zero;

		// If the camera is trying to follow a Transform
		if(targetTransform != null)
		{
			// The camera's move destination is the position of the 'target' Transform.
			destination = targetTransform.position;
		}
		// Else, if targetTransform is set to null, the camera is following a position, and not a Transform
		else 
		{
			// Set the camera's destination to its 'targetPosition' member variable
			destination = targetPosition;
		}

		// Sets the camera destination's z-position to the default value to ensure that the whole world is always viewable
		destination.z = PositionZ;

		// Move the camera to the destination position using Vector2.SmoothDamp()
		transform.position = Vector3.SmoothDamp (transform.position, destination, ref velocity, dampTime);

		// Zoom the camera to its target zoom smoothly.
		camera2d.ZoomFactor = Mathf.Lerp (camera2d.ZoomFactor, this.targetZoom, cameraSpeed * Time.deltaTime);
	}

	/// <summary>
	/// Apply the given camera movement to the camera.
	/// </summary>
	public void ApplyCameraMovement(CameraMovement cameraMovement)
	{
		// Stores true if the CameraMovement requires the camera to follow a transform and not a static position
		bool followTransform = (cameraMovement.targetPosition == TargetPosition.Self);

		// If the camera must follow a Transform's position
		if(followTransform)
		{
			// Set the camera's target Transform to follow to the Transform stored inside the CameraMovement instance
			TargetTransform = cameraMovement.targetTransform;
		}
		// Else, if the camera must follow a static position, and not a Transform
		else
		{
			// Make the camera follow the position stored inside the CameraMovement instance
			MovePosition = cameraMovement.movePosition;
		}

		// Set the camera's target zoom
		targetZoom = cameraMovement.zoom;

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
	public Transform TargetTransform
	{
		get { return targetTransform; }
		set 
		{ 
			targetTransform = value; 
		}
	}

	/// <summary>
	/// The static position the camera move towards.
	/// </summary>
	/// <value>The target position.</value>
	public Vector2 MovePosition 
	{
		get { return targetPosition; }
		set 
		{
			// Set the camera's target Transform to null. The camera can only follow either a position or a Transform
			targetTransform = null;
			targetPosition = value;
		}
	}

	/// <summary>
	/// The speed at which the camera moves and zooms
	/// </summary>
	public float CameraSpeed
	{
		get { return cameraSpeed; }
		set { cameraSpeed = value; }
	}
}

/// <summary>
/// The current mode the camera is set to. The camera works differently depending on their current camera mode.
/// </summary>
public enum CameraMode
{
	Combat
}
