using System;
using System.Collections.Generic;
using UnityEngine;

public class TouchInfo
{
	private static readonly LayerMask touchableLayers = LayerMask.GetMask(
		"TouchRegion"
	);

	/** The ID used to refer to the touch */
	public int id;

	public Vector2 startPosition = new Vector2(0f, 0f);
	public Vector2 currentPosition = new Vector2(0f, 0f);
	public Vector2 finalPosition = new Vector2(0f, 0f);

	public Vector2 startWorldPosition = new Vector2(0f, 0f);
	public Vector2 currentWorldPosition = new Vector2(0f, 0f);
	public Vector2 finalWorldPosition = new Vector2(0f, 0f);

	private float deltaX;
	private float deltaY;
	private float velocitySquared;

	public float startTime;
	public float finalTime;
	public float totalTime;

	public GameObject goTouchInitial;
	public GameObject goTouchCurrent;
	public GameObject goTouchFinal;

	private List<GameObject> gameObjectsTouched;
	public bool pressed;

	public List<GameObject> GameObjectsTouched
	{
		get { return gameObjectsTouched; }
	}

	public TouchInfo(int id)
	{
		this.id = id;
		gameObjectsTouched = new List<GameObject>();
	}

	public void Pressed(float x, float y)
	{
		startPosition.Set(x, y);
		currentPosition.Set(x, y);
		RaycastHit2D hitInfo = GetHitInfo(startPosition);
		startWorldPosition = Camera.main.ScreenToWorldPoint(startPosition);
		Transform transform = hitInfo.transform;
		if (transform != null)
		{
			goTouchInitial = transform.gameObject;
			gameObjectsTouched.Add(goTouchInitial);
		}
		else
		{
			goTouchInitial = null;
		}
		startTime = Time.time;
		pressed = true;
	}

	public void Dragged(float x, float y)
	{
		currentPosition.Set(x, y);
		RaycastHit2D hitInfo = GetHitInfo(currentPosition);
		currentWorldPosition = Camera.main.ScreenToWorldPoint(currentPosition);
		Transform transform = hitInfo.transform;
		if (transform != null)
		{
			goTouchCurrent = transform.gameObject;
			gameObjectsTouched.Add(goTouchCurrent);
		}
		else
		{
			goTouchCurrent = null;
		}
		totalTime = Time.time - startTime;
	}

	public void Released(float x, float y)
	{
		finalPosition.Set(x, y);
		RaycastHit2D hitInfo = GetHitInfo(currentPosition);
		finalWorldPosition = Camera.main.ScreenToWorldPoint(finalPosition);
		Transform transform = hitInfo.transform;
		if (transform != null)
		{
			goTouchFinal = transform.gameObject;
			gameObjectsTouched.Add(goTouchFinal);
		}
		else
		{
			goTouchFinal = null;
		}
		finalTime = Time.time;
		totalTime = finalTime - startTime;
		deltaX = finalPosition.x - startPosition.x;
		deltaY = finalPosition.y - startPosition.y;
		velocitySquared = (deltaX * deltaX + deltaY * deltaY) / totalTime;
		pressed = false;
	}

	/// <summary>
	/// Returns true if this touch is a click. Must be called the frame this touch is 
	/// released. Otherwise, it will always return false.
	/// </summary>
	public bool IsClick()
	{
		return !pressed && !IsLongPress() && finalTime == Time.time;
	}

	public bool IsLongPress()
	{
		return totalTime >= 0.2f;
	}

	public SwipeDirection GetSwipeDirection()
	{
		// If this touch is not a swipe, return 'None' as a swipe direction to indicate an error
		if(!IsSwipe())
			return SwipeDirection.None;

		// If this touch travelled primarily to the left, consider it a left-swipe
		if(deltaX < 0f && Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
			return SwipeDirection.Left;
		// If this touch travelled primarily to the right, consider it a right-swipe
		else if(deltaX > 0f && Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
			return SwipeDirection.Right;
		// If this touch travelled primarily upwards, consider it an upward-swipe
		else if(deltaY > 0f && Mathf.Abs(deltaY) > Mathf.Abs(deltaX))
			return SwipeDirection.Up;
		// Else, if this touch travelled downwards, return that it is a downward swipe
		else if(deltaY < 0f && Mathf.Abs(deltaY) > Mathf.Abs(deltaX))
			return SwipeDirection.Down;

		// If this statement is reached, this touch is not a swipe. Thus, return 'None' as a direction
		return SwipeDirection.None;
	}

	/// <summary>
	/// True if this instance is a swipe.
	/// </summary>
	public bool IsSwipe()
	{
		// If this touch was not held long and its squared velocity exceeds
		// the minimum requirement, this touch is a swipe
		return !IsLongPress() && velocitySquared >= InputManager.SWIPE_VELOCITY_SQUARED;
	}

	/// <summary>
	/// Reset this touch. Called when the touch is released and its internal
	/// data is no longer useful
	/// </summary>
	public void Reset()
	{
		totalTime = 0f;
		startTime = (finalTime = 0f);
		deltaX = (deltaY = (velocitySquared = 0f));
		startPosition.Set(0f, 0f);
		currentPosition.Set(0f, 0f);
		finalPosition.Set(0f, 0f);
		goTouchInitial = (goTouchFinal = null);
		gameObjectsTouched.Clear();
		pressed = false;
	}

	/// <summary>
	/// Returns the information about the GameObject hit by the given pixel position.
	/// </summary>
	private RaycastHit2D GetHitInfo(Vector2 touchPosition)
	{
		// Shoot a ray from the touch point to the game and store the 
		Ray ray = Camera.main.ScreenPointToRay(touchPosition);
		RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, 1000f, TouchInfo.touchableLayers);

		// Stores the closest collider that was hit by the given touch coordinates
		RaycastHit2D closestHit = default(RaycastHit2D);

		// Cycle through each collider hit by the touch. Note that the hits are sorted from closest to the screen to the furthest from the screen.
		for(int i = 0; i < hits.Length; i++)
		{
			Collider2D touchedCollider = hits[i].collider;

			Debug.Log ("Object touched: " + touchedCollider);

			// Retrieve the 'TouchRegion' component attached to the touched collider. Each touchable collider has a TouchRegion component. 
			// It helps the 'TouchInfo' class determine information about the GameObject it touched.
			TouchRegion touchedRegion = touchedCollider.GetComponent<TouchRegion>();

			Debug.Log ("Region touched: " + touchedRegion);

			// If the region that was touched belongs to a player or an enemy
			if(touchedRegion.objectType == ObjectType.Player || touchedRegion.objectType == ObjectType.Enemy)
			{
				// Get the Character component of the character that was hit.
				Character touchedCharacter = touchedRegion.character;

				// If the touched character is not dead, he is elligible to be returned by this method. However, if the character is dead,
				// it should ignore user input. In this case, this character will be ignored by the raycast.
				if(!touchedCharacter.CharacterStats.IsDead ())
				{
					// Store this character as the closest GameObject touched by the given touch position.
					closestHit = hits[i];
					// The object touched by the given touch position has been found. Break the loop and return the touched character.
					break;
				}
			}
		}

		// Return the information about the closest 
		return closestHit;
	}
}

/// <summary>
/// Denotes the direction of a swipe
/// </summary>
public enum SwipeDirection
{
	None,
	Up,
	Down,
	Left,
	Right,
	Vertical,
	Horizontal,
	Any
}
