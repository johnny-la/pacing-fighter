using System;
using System.Collections.Generic;
using UnityEngine;
public class InputManager : MonoBehaviour
{
	/** The max number of touches that can be registered on-screen at once. */
	public const int MAX_TOUCHES = 5;

	/** The minimum amount of time a touch must be held to be qualified as a long press */
	public const float LONG_PRESS_DURATION = 0.2f;
	/** The minimum swipe velocity (squared) in pixels**2/second required to qualify a click as a swipe. */
	public const float SWIPE_VELOCITY_SQUARED = 100.0f * 100.0f;

	/** The different touch processors which can alter game state. One is chosen depending on the current game mode */
	private CombatTouchProcessor combatProcessor;
	/** The touch processor currently assigned to process user input and call game state methods. */
	private ITouchProcessor touchProcessor;
	/** A dictionary which maps touch-ids to TouchInfo instances (containers for touch data). */
	private Dictionary<int, TouchInfo> touches;

	public ITouchProcessor TouchProcessor
	{
		get { return this.touchProcessor; }
		set { this.touchProcessor = value; }
	}

	public void Awake()
	{
		// Create a dictionary linking touch ids to touch objects
		this.touches = new Dictionary<int, TouchInfo>();

		// Creates the InputProcessors responsible for receiving input events
		this.combatProcessor = new CombatTouchProcessor();

		// The currently activate TouchProcessor which receives input events and modifies game state
		this.touchProcessor = this.combatProcessor;

		// Cycles through all of the possible touch ids that can exist
		for (int id = 0; id < MAX_TOUCHES; id++)
		{
			// Creates a data container for each possible touch id
			this.touches.Add(id, new TouchInfo(id));
		}
	}

	public void Update()
	{
		if (Input.touchSupported)
		{
			// Store all the currently-active touches 
			Touch[] array = Input.touches;

			// Cycle through each touch being held by the user
			for (int i = 0; i < array.Length; i++)
			{
				// Return if this touch exceeds the max number of touches to process
				if(i >= MAX_TOUCHES -1)
					return;

				Touch touch = array[i];
				Debug.Log("Touch " + touch.fingerId + " registered.");

				switch(touch.phase)
				{
				case TouchPhase.Began:
					// Inform the touch it was pressed
					touches[touch.fingerId].Pressed(touch.position.x, touch.position.y);
					break;
				case TouchPhase.Moved:
				case TouchPhase.Stationary:
					// Inform the touch it was dragged
					touches[touch.fingerId].Dragged(touch.position.x, touch.position.y);
					break;
				case TouchPhase.Ended:
				case TouchPhase.Canceled:
					// Tell the touch it was released
					touches[touch.fingerId].Released(touch.position.x, touch.position.y);
					break;
				}

				// Process the touch 
				ProcessTouch(touches[touch.fingerId]);
			}
		}
		else
		{
			this.ProcessMouseInput();
		}
	}

	public void ProcessMouseInput()
	{
		if (Input.GetMouseButtonDown(0))
		{
			this.touches[0].Pressed(Input.mousePosition.x, Input.mousePosition.y);
			this.ProcessTouch(this.touches[0]);
		}
		else if (Input.GetMouseButton(0))
		{
			this.touches[0].Dragged(Input.mousePosition.x, Input.mousePosition.y);
			this.ProcessTouch(this.touches[0]);
		}
		else if (Input.GetMouseButtonUp(0))
		{
			this.touches[0].Released(Input.mousePosition.x, Input.mousePosition.y);
			this.ProcessTouch(this.touches[0]);
		}
	}
	private void ProcessTouch(TouchInfo touch)
	{
		if (touch.pressed)
		{
			if (touch.IsLongPress())
			{
				this.touchProcessor.LongPress(touch);
			}
			else
			{
				this.touchProcessor.ShortPressDown(touch);
			}
		}
		else
		{
			if (touch.IsLongPress())
			{
				this.touchProcessor.LongPressUp(touch);
			}
			else if (touch.IsSwipe())
			{
				this.Swipe(touch);
			}
			else
			{
				this.touchProcessor.OnClick(touch, null);
			}
			touch.Reset();
		}
	}
	private void Swipe(TouchInfo touch)
	{
		float num = 0f;
		GameObject gameObject = null;
		for (int i = 0; i < touch.GameObjectsTouched.Count; i++)
		{
			GameObject gameObject2 = touch.GameObjectsTouched[i];
			float num2 = 10f;
			if (num2 > num)
			{
				gameObject = gameObject2;
				num = num2;
			}
		}
		Debug.Log("Swiped object " + gameObject);

		// Inform the Touch Processor of a swipe so that it can alter game state accordingly
		touchProcessor.OnSwipe(touch, gameObject);

	}
}
