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

	/** The next action that will play right after the current action. Used to add input leniency */
	private Action queuedAction;
		
	public void Awake()
	{
		// Caches the Character component this script is controlling
		character = GetComponent<Character>();

		// Cache the character's components for easy access
		actionSet = GetComponent<ActionSet>();
	}

	/// <summary>
	/// Performs the given action. The action is given in the form of an ActionScriptableObject, which
	/// is a serializable container for an action. When the character is created, his ActionSet creates
	/// an Action instance for each ActionScriptableObject he is given in the inspector. Therefore, a 
	/// search must be done to find the Action instance corresponding to the given scriptable object
	/// before actually performing the action.
	/// </summary>
	public void PerformAction(ActionScriptableObject actionScriptableObject)
	{
		// Stores the action corresponding to the given scriptable object
		Action action = actionSet.FindAction(actionScriptableObject);

		// Perform the action
		PerformAction (action);
	}

	/// <summary>
	/// Performs the given action.
	/// Call this ONLY if you have a reference to an Action stored inside CharacterControl.ActionSet.combatActions/basicActions.
	/// This is because Action instances vary from character to character. Calling PerformAction() using the Action instance
	/// belonging to another character will cause wonky behaviour, as the Action may reference to another character.
	/// </summary>
	/// <param name="action">The action to perform.</param>
	public void PerformAction(Action action)
	{
        // If the given action is null, ignore this call
        if (action == null)
            return;

		/*if(tag == "Player")
		{
			if(currentAction != null)
			{
				Debug.Log ("Current action: " + currentAction.name);

				Debug.Log ("Cancelable? " + currentAction.cancelable);
			}
			else
			{
				Debug.Log ("Current action: null");
			}
			Debug.Log ("New action: " + action.name + " Override cancelable? " + action.overrideCancelable);
		}*/

		Debug.Log ("Make " + this.name + " perform " + action.name);

		// Play the animations required to perform this action. Stores the index of the chosen animation sequence
		int animationSequenceIndex = character.CharacterAnimator.Play (action);
		// Apply the forces on the character required to perform the action
		character.CharacterForces.Play(action);
		// Activate the hit boxes specified this action
		character.CharacterCollider.Play (action, animationSequenceIndex);
		// Play a sound when the action starts
		character.Sound.PlayRandomSound(action.startSounds);

		// Update the character's current action
		currentAction = action;
	}

	/// <summary>
	/// Queues the given action. It will be performed once the current action is done being performed.
	/// </summary>
	public void QueueAction(Action action)
	{
		// Update the 'queuedAction' member variable
		queuedAction = action;
	}

	/// <summary>
	/// Make this character perform the event specified in the given object
	/// </summary>
	public void PerformEvent(Brawler.Event e)
	{
		// If the event requires an action to be performed
		if(e.type == Brawler.EventType.PerformAction)
		{
			// Make this character perform the action specified in the event
			PerformAction (e.actionToPerform.action);
		}
		// Else, if the event requires a basic action to be performed
		else if(e.type == Brawler.EventType.PerformBasicAction)
		{
			// Retrieve the Action instance which corresponds to the basic action specified in the event
			Action basicAction = character.CharacterControl.ActionSet.basicActions.GetBasicAction(e.basicActionToPerform);
			
			// Make the character affected by this force perform the basic action specified by the given force
			PerformAction (basicAction);
		}
		// Else, if the event requires the game to enter slow motion
		else if(e.type == Brawler.EventType.SlowMotion)
		{

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

		// Get rid of the character's queued action, which was set to perform after the current action. It should
		// no longer be performed, since the current action was canceled.
		queuedAction = null;
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

		// If the character has a action queued up, perform it, since the current action is complete.
		if(queuedAction != null)
		{
			// Perform the queuedAction
			PerformAction (queuedAction);

			// Set the queued action to null. In fact, the queued action has just been played, and thus needs to be deleted from the queue. 
			queuedAction = null;
		}
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

		// If the given input can perform an action 
		if(validAction != null)
		{
			// The action's target object is the same object that was pressed when this action was performed
			validAction.targetObject = pressedObject;
			// The action's target position is the last position that was pressed by the touch that performed this action
			validAction.targetPosition = touch.finalWorldPosition;

			// If the character is not performing a action 
			// OR if the current action can be cancelled midway
			// OR if the action to perform can cancel any action
			if(currentAction == null || currentAction.cancelable || validAction.overrideCancelable)
			{
				// Perform the action which corresponds to the user's input
				PerformAction(validAction);
			}
			// Else, if the character is already performing an action that can't be canceled
			else
			{
				// Queue the given action. It will be performed when the current action is done being performed.
				QueueAction(validAction);
			}
		}
			
	}

	/// <summary>
	/// Returns the exact time in seconds specified by this starting time. The parameter is a CastingTime instance,
	/// which specifies a time in several different ways. This method returns the time in seconds represented by this
	/// instance.
	/// </summary>
	public float GetStartTime(CastingTime startTime)
	{
		// Determines the time at which the force starts
		switch(startTime.type)
		{
		case DurationType.Frame:
			// The start time is specified by the n-th frame of the character's current animation
			return startTime.nFrames / CharacterAnimator.FRAME_RATE;
		case DurationType.WaitForAnimationComplete:
			// The force will start when 'animationToWaitFor' is complete
			return character.CharacterAnimator.GetEndTime(startTime.animationToWaitFor);
		default:
			Debug.LogError ("Incorrect start time specified: " + startTime.type.ToString ());
			break;
		}
		
		// If this statement is reached, the force does not specify the time at which it should start. Return a default of zero.
		return 0;
	}
	
	/// <summary>
	/// Returns the amount of time (in seconds) that this CastingTime instance represents. The startTime for the event must be
	/// given to determine the duration, and not the end time of the event
	/// </summary>
	public float GetDuration(CastingTime duration, float startTime)
	{
		// Determines the time at which the force starts
		switch(duration.type)
		{
		case DurationType.Frame:
			// The duration is specified by the amount of time elapsed from n frames of the character's current animation
			return duration.nFrames / CharacterAnimator.FRAME_RATE;
		case DurationType.WaitForAnimationComplete:
			// The force will end when 'animationToWaitFor' is complete
			return character.CharacterAnimator.GetEndTime(duration.animationToWaitFor) - startTime;
		case DurationType.UsePhysicsData:
			// Use the character's walking physics values when applying a force. The force will last as long as it
			// takes for his walking speed to get him there
			break;
		}
		
		// If this statement is reached, the force object does not directly specify its duration. If this duration
		// is used for a force, the duration is not directly specified in seconds. Instead, the force moves the 
		// character using his physics values. Therefore, the force will last as long as it takes for his physics
		// values to reach its target. Thus, return 0 since the CastingTime instance has no specified duration
		return 0.0f;
	}

	/// <summary>
	/// The action currently being performed by the character
	/// </summary>
	public Action CurrentAction
	{
		get { return currentAction; }
		set { if(value == null && gameObject.layer == Brawler.Layer.Player) Debug.LogError ("Set player current action to NULL"); 
			this.currentAction = value; }
	}

	/// <summary>
	/// The data container containing a list of all the actions the character can perform.
	/// </summary>
	public ActionSet ActionSet
	{
		get { return actionSet; }
	}
}