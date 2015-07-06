using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Brawler;


public class CharacterControl : MonoBehaviour 
{
	/** Caches the Character component that this script is controlling. */
	private Character character;

	/** The action set containing the actions the character can perform */
	private ActionSet actionSet;

	/** The action the character is currently performing */
	private Action currentAction;
		
	public void Awake()
	{
		// Caches the Character component this script is controlling
		character = GetComponent<Character>();

		// Cache the character's components for easy access
		actionSet = GetComponent<ActionSet>();
	}

	/// <summary>
	/// Performs the given action. Assumes that the action does not target a GameObject or a specific location
	/// </summary>
	public void PerformAction(Action action)
	{
		// Delegate the call to a helper method. Note: Vector2.zero doesn't cause GC
		PerformAction (action, null, Vector2.zero);
	}

	/// <summary>
	/// Performs the given action
	/// </summary>
	/// <param name="action">The action to perform.</param>
	/// <param name="touchedObject">The GameObject targetted by this action</param>
	/// /// <param name="touchPosition">The touch position when this action was activated</param>
	public void PerformAction(Action action, GameObject touchedObject, Vector2 touchPosition)
	{
        // If the given action is null, ignore this call
        if (action == null)
            return;

		// If the character is not performing a action 
		// OR if the current action can be cancelled midway
		// OR if the given action can cancel any action
		if(currentAction == null || currentAction.cancelable || action.overrideCancelable)
		{
			// Play the animations required to perform this action. Stores the index of the chosen animation sequence
			int animationSequenceIndex = character.CharacterAnimator.Play (action);
			// Apply the forces on the character required to perform the action
			character.CharacterForces.Play(action, touchedObject, touchPosition);
			// Activate the hit boxes specified this action
			character.CharacterCollider.Play (action, touchedObject, animationSequenceIndex);
			// Play a sound when the action starts
			character.Sound.PlayRandomSound(action.startSounds);

			// Update the character's current action
			currentAction = action;
		}
	}

	/// <summary>
	/// Stops the character midway in his current action
	/// </summary>
	public void CancelCurrentAction()
	{
		// Disable the hit boxes associated with the current action so that it can't inflict further damage
		character.CharacterCollider.DisableHitBoxes (currentAction);

		// Set the character's current action to null, since his current action has been cancelled.
		currentAction = null;
	}

	/// <summary>
	/// Delegates once the character finishes performing an action
	/// </summary>
	public void OnActionComplete()
	{
		// Disable the hit boxes associated with the action that is finished being performed
		character.CharacterCollider.DisableHitBoxes (currentAction);
		// The character has finished his action. Thus, set his current action to null
		currentAction = null;
	}
		
	/// <summary>
	/// Called when the user performs a touch that is interpreted
	/// as being performed by this character. Depending on the input
	/// and the character's action set, an appropriate action will be
	/// performed
	/// </summary>
	public void OnTouch(TouchInfo touch, InputType inputType, GameObject pressedObject,
		                SwipeDirection swipeDirection)
	{
		// Stores the region which was pressed by this touch
		InputRegion inputRegion = InputRegion.EmptySpace;
			
		// If the touch did not press a GameObject
		if(pressedObject == null)
		{
			// Inform the touch it has pressed empty space
			inputRegion = InputRegion.EmptySpace;
		}
		// Else, if the touch did not press any object
		else if(pressedObject != null)
		{
			// If the touch pressed the same GameObject that received this event 
			if(pressedObject == gameObject)
			{
				// The GameObject touched itself
				inputRegion = InputRegion.Self;
			}
			// Else, if the touch pressed an enemy
			else if(pressedObject.layer == Layer.Enemy)
			{
				// Inform the touch that it pressed an enemy
				inputRegion = InputRegion.Enemy;
			}
		}

		// Retrieve an action that can be performed from the given touch
		Action validAction = actionSet.GetActionFromInput(inputType, inputRegion, swipeDirection);

		if(validAction != null)
			Debug.Log ("Make " + this.name + " perform " + validAction.name);

        // Perform the action which corresponds to the user's input
        PerformAction(validAction, pressedObject, touch.finalWorldPosition);
	}

	/// <summary>
	/// The action currently being performed by the character
	/// </summary>
	public Action CurrentAction
	{
		get { return currentAction; }
		set { this.currentAction = value; }
	}

	/// <summary>
	/// The data container containing a list of all the actions the character can perform.
	/// </summary>
	public ActionSet ActionSet
	{
		get { return actionSet; }
	}
}