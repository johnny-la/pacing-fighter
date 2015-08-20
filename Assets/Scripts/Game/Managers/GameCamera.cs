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

	/** The type of target the camera is following. */
	private CameraTarget target = CameraTarget.FocalPoint;

	/** The main focal point of the camera. The camera will never leave this Transform out of its sight, and if the camera
	    has no 'targetPosition' or 'targetTransform' set,  it falls back to following this Transform's position. */
	private Transform focalPoint;

	/** The position offset relative to the 'focalPoint's position that the camera follows. Allows the camera to follow
	 *  a point slightly to the left or right of the focal point. */
	private Vector3 focalPointOffset = Vector3.zero;

	/** Stores the Transform that the camera should follow around. */
	private Transform targetTransform;

	/** The position the camera is moving towards. */
	private Vector3 targetPosition;
	/** Specifies an offset position relative to the 'targetPosition/Transform'. The camera's target position will be offset by this position. */
	private Vector3 targetOffset = Vector3.zero;

	/** The zooming factor that the camera is trying to get to. */
	private float targetZoom;

	/** The speed at which the camera zooms and moves. */
	private float cameraSpeed;

	/** The maximum and minimum y-values the camera can see */
	private Range verticalBounds = new Range();
	/** The maximum and minimum x-values the camera can see */
	private Range horizontalBounds = new Range();

	/** Component used to shake the camera. */
	private ShakePosition cameraShaker;

	/** The camera's current velocity. Needed when calling the SmoothDamp() function to move the camera. */
	private Vector3 velocity;

	/** A cached version of the camera's Transform */
	private new Transform transform;
	
	void Awake () 
	{
		// Cache the camera components so that they can be controlled and moved around the world.
		camera = GetComponent<Camera>();
		camera2d = GetComponent<tk2dCamera>();

		// Add a 'ShakePosition' component to allow screen shake
		cameraShaker = gameObject.AddComponent<ShakePosition>();

		// Caches the camera's Transform for efficiency purposes
		transform = GetComponent<Transform>();
	}

	void Update () 
	{
		// Stores the camera's target destination
		Vector3 destination = Vector3.zero;

		// If the camera is trying to follow the Transform set in the member variable 'this.targetTransform'
		if(target == CameraTarget.Transform)
		{
			// The camera's move destination is the position of the 'target' Transform.
			destination = targetTransform.position + targetOffset;
		}
		// Else, if targetTransform is set to null, the camera is following a position, and not a Transform
		else if(target == CameraTarget.Position)
		{
			// Set the camera's destination to its 'targetPosition' member variable
			destination = targetPosition + targetOffset;
		}
		// Else, as a fallback option, follow the camera's focal point
		else if(target == CameraTarget.FocalPoint && focalPoint != null)
		{
			// The camera will move towards the focal point's position
			destination = focalPoint.position + focalPointOffset;
		}

		// Sets the camera destination's z-position to the default value to ensure that the whole world is always viewable
		destination.z = PositionZ;

		// Move the camera to the destination position using Vector2.SmoothDamp()
		transform.position = Vector3.SmoothDamp (transform.position, destination, ref velocity, 1/cameraSpeed);

		// Clamp the camera to ensure it never exceeds its current boundaries
		ClampCameraPosition(); 

		// Zoom the camera to its target zoom smoothly.
		camera2d.ZoomFactor = Mathf.Lerp (camera2d.ZoomFactor, this.targetZoom, cameraSpeed * Time.deltaTime);
	}

	/// <summary>
	/// Apply the given camera movement to the camera.
	/// </summary>
	public void ApplyCameraMovement(CameraMovement cameraMovement)
	{
		// Stores true if the CameraMovement requires the camera to follow a transform and not a static position
		bool followTransform = (cameraMovement.target == global::TargetPosition.Self);

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
			TargetPosition = cameraMovement.targetPosition;
		}

		// Update the speed at which the camera moves and zooms.
		cameraSpeed = cameraMovement.cameraSpeed;

		// Set the camera's target zoom
		targetZoom = cameraMovement.zoom;

	}

	/// <summary>
	/// Shakes the camera using the given settings.	
	/// </summary>
	public void Shake(float duration, float speed, float magnitude)
	{
		// Shake the camera using the 'cameraShaker' component.
		cameraShaker.PlayShake (duration,speed,magnitude);
	}

	/// <summary>
	/// Clamps this camera's position to its boundaries to ensure it never goes outside the level's bounds
	/// </summary>
	private void ClampCameraPosition()
	{
		// Clamp the camera's x and y position so that the camera can never see beyond these coordinates
		float x = Mathf.Clamp (transform.position.x, horizontalBounds.min + (this.WorldWidth*0.5f), 
		                                             horizontalBounds.max - (this.WorldWidth*0.5f));
		float y = Mathf.Clamp (transform.position.y, verticalBounds.min + (this.WorldHeight*0.5f), 
		                                             verticalBounds.max - (this.WorldHeight*0.5f));

		// Clamp the camera's position to the position computed above
		transform.position = new Vector3(x,y, PositionZ);
	}

	/// <summary>
	/// Returns true if the given point is viewable by the camera.
	/// </summary>
	public bool IsViewable(Vector2 point)
	{
		// If the given point is within the viewable bounds of the camera, return true, since the camera can see the point
		if(point.x <= Right && point.x >= Left && point.y <= Top && point.y >= Bottom)
			return true;

		// If this statement is reached, the camera cannot see the given point. Thus, return false.
		return false;
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
	/// The type of target (transform, position, etc.) the camera is following
	/// </summary>
	public CameraTarget Target
	{
		get { return target; }
		set { target = value; }
	}

	/// <summary>
	/// The maximum and minimum y-values the camera can see
	/// </summary>
	public Range VerticalBounds
	{
		get { return verticalBounds; }
		set { verticalBounds = value; }
	}

	/// <summary>
	/// The maximum and minimum x-values the camera can see
	/// </summary>
	public Range HorizontalBounds
	{
		get { return horizontalBounds; }
		set { horizontalBounds = value; }
	}

	/// <summary>
	/// The main focal point of the camera. The camera will never leave this Transform out of its sight, and if the camera
	/// has no 'targetPosition' or 'targetTransform' set,  it falls back to following this Transform's position. 
	/// </summary>
	public Transform FocalPoint
	{
		get { return focalPoint; }
		set { focalPoint = value; }
	}
	
	/// <summary>
	/// The position offset relative to the 'focalPoint's position that the camera follows. Allows the camera to follow
	/// a point slightly to the left or right of the focal point.
	/// </summary>
	public Vector3 FocalPointOffset
	{
		get { return focalPointOffset; }
		set { focalPointOffset = value; }
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
			// Tell the camera to follow this new Transform
			target = CameraTarget.Transform;
		}
	}

	/// <summary>
	/// The static position the camera move towards.
	/// </summary>
	/// <value>The target position.</value>
	public Vector2 TargetPosition 
	{
		get { return targetPosition; }
		set 
		{
			targetPosition = value;
			// Tell the camera to follow this new position
			target = CameraTarget.Position;
		}
	}

	/// <summary>
	/// Specifies an offset position relative to the 'targetPosition/Transform'. 
	/// The camera's target position will be offset by this position.
	/// </summary>
	public Vector3 TargetOffset
	{
		get { return targetOffset; }
		set { targetOffset = value; }
	}

	/// <summary>
	/// The speed at which the camera moves and zooms
	/// </summary>
	public float CameraSpeed
	{
		get { return cameraSpeed; }
		set { cameraSpeed = value; }
	}

	/// <summary>
	/// Returns the height of the camera in world units
	/// </summary>
	public float WorldHeight
	{
		get
		{
			// The camera's height is twice its orthographic size
			return camera.orthographicSize * 2.0f / PixelsPerMeter;
		}
	}

	/// <summary>
	/// Returns the width of the camera.
	/// </summary>
	public float WorldWidth
	{
		get 
		{
			// The width of camera is its height times its aspect ratio.
			return WorldHeight * camera.aspect;
		}
	}

	/// <summary>
	/// Returns the number of pixels compressed into a meter (one world unit).
	/// This is the setting the camera is using to display the game.
	/// </summary>
	public float PixelsPerMeter
	{
		get { return camera2d.CameraSettings.orthographicPixelsPerMeter; }
	}

	/// <summary>
	/// Returns the y-position of the top of the camera.
	/// </summary>
	public float Top
	{
		get { return transform.position.y + (WorldHeight * 0.5f); }
	}
	
	/// <summary>
	/// Returns the y-position of the bottom of the camera.
	/// </summary>
	public float Bottom
	{
		get { return transform.position.y - (WorldHeight * 0.5f); }
	}
	
	/// <summary>
	/// Returns the x-position of the left of the camera.
	/// </summary>
	public float Left
	{
		get { return transform.position.x - (WorldWidth * 0.5f); }
	}
	
	/// <summary>
	/// Returns the x-position of the right of the camera.
	/// </summary>
	public float Right
	{
		get { return transform.position.x + (WorldWidth * 0.5f); }
	}

	/// <summary>
	/// Returns the camera's Transform component used to move the camera around.
	/// </summary>
	public Transform Transform
	{
		get { return transform; }
	}

	public string ToString()
	{
		return "Position: ( " + transform.position.x + ", " + transform.position.y + ") Dimensions: " + WorldWidth + " x " + WorldHeight +
			" X-Bounds: " + horizontalBounds.ToString () + " Y-Bounds: " + verticalBounds.ToString (); 
	}
}

/// <summary>
/// The current mode the camera is set to. The camera works differently depending on their current camera mode.
/// </summary>
public enum CameraMode
{
	Combat
}

/// <summary>
/// The type of target the camera is following
/// </summary>
public enum CameraTarget
{
	Transform,
	Position,
	FocalPoint
}
