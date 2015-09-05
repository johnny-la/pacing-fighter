using UnityEngine;
using System.Collections;

/// <summary>
/// Creates a GameObject that a UI component can follow around. That is, this GameObject represents the world position of 
/// the UI widget.
/// 
/// Attach this component to a UI component which should follow around a position in the world.
/// </summary>
public class WorldToCanvas : MonoBehaviour
{

	/** Stores a Transform placed in the world which this GUI element will follow. */
	private Transform worldTransform;
	/** Stores the Rigidbody used to move the GUI element in the world. */
	private Rigidbody2D worldRigidbody;

	/** The RectTransform for the Canvas that the UI component is placed in. */
	private RectTransform canvasRect;

	/** This UI component's Transform component. */
	private new Transform transform;

	// Use this for initialization
	void Awake ()
	{
		// Creates a GameObject in the world. This UI widget will follow its position.
		worldTransform = new GameObject(gameObject.name + " World Position").GetComponent<Transform>();

		// Cache this UI widget's Transform
		transform = GetComponent<Transform>();
	}

	void Update()
	{
		// Convert the world Transform's position into canvas coordinates, and place the UI Component at this position.
		transform.localPosition = UIUtils.WorldToCanvasPosition (canvasRect, worldTransform.position);
	}

	/// <summary>
	/// Creates a rigidbody on the world GameObject. This way, the UI component will utilize physics.
	/// </summary>
	public void CreateRigidbody()
	{
		// Creates and caches a Rigidbody for the world GameObject that this UI widget will follow
		worldRigidbody = worldTransform.gameObject.AddComponent<Rigidbody2D>();
	}

	/// <summary>
	/// The RectTransform for the Canvas where the UI component is positioned. Allows the world position to be converted
	/// to canvas coordinates.
	/// Warning: must be set on Awake/Start. Otherwise, this script's Update() method will throw a NullReferenceException
	/// </summary>
	public RectTransform CanvasRect
	{
		get { return canvasRect; }
		set { canvasRect = value; }
	}

	/// <summary>
	/// Stores a Transform placed in the world which this GUI element will follow. 
	/// </summary>
	public Transform WorldTransform
	{
		get { return worldTransform; }
		set { worldTransform = value; }
	}

	/// <summary>
	/// Stores the Rigidbody used to move the GUI element in the world. 
	/// </summary>
	public Rigidbody2D WorldRigidbody
	{
		get { return worldRigidbody; }
		set { worldRigidbody = value; }
	} 
}

