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

		Debug.Log ("Make " + this.name + " perform " + action.name);

		// Play the animations required to perform this action. Stores the index of the chosen animation sequence
		int animationSequenceIndex = character.CharacterAnimator.Play (action);
		// Apply the forces on the character required to perform the action
		character.CharacterForces.Play(action);
		// Activate the hit boxes specified this action
		character.CharacterCollider.Play (action, animationSequenceIndex);
		// Play a sound when the action starts
		character.Sound.PlayRandomSound(action.startSounds);

		// Make this character perform all the 'onStartEvents' in the action. These are the events which play when the action starts
		for(int i = 0; i < action.onStartEvents.Length; i++)
			PerformEvent (action.onStartEvents[i]);

		// Update the character's current action
		currentAction = action;
	}

	/// <summary>
	/// Makes this character perform the given basic action.
	/// </summary>
	public void PerformAction(BasicActionType basicAction)
	{
		// Retrieve the character's basic action from his 'actionSet' and perform the move
		PerformAction (actionSet.basicActions.GetBasicAction (basicAction));
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
		// Stores true if the event specifies a starting time
		bool requiresStartTime = (e.type != Brawler.EventType.None);
		// Holds true if the event requires a duration to be specified
		bool requiresDuration = (e.type == Brawler.EventType.SlowMotion);

		// The starting time and duration of the event, in seconds
		float startTime = 0;
		float duration = 0;

		// If the event requires a starting time or duration to be specified, compute them and store them in their respective variables.
		if(requiresStartTime)
			startTime = GetStartTime (e.startTime);
		if(requiresDuration)
			duration = GetDuration (e.duration, startTime);

		// Call a coroutine. Start the given event in 'startTime' seconds. The event will last 'duration' seconds
		StartCoroutine (PerformEventCoroutine(e, startTime, duration));
	}

	/// <summary>
	/// Performs the given event. The event will start at 'startTime' seconds, and last a total of 'duration' seconds.
	/// </summary>
	/// <returns>The event coroutine.</returns>
	/// <param name="startTime">The time in seconds relative to the time this function is called at which the event 
	/// starts. Some events don't require this field.</param>
	/// <param name="duration">The time in seconds that this event lasts. Some events don't require this field, and thus 
	/// this value is ignored.</param>
	private IEnumerator PerformEventCoroutine(Brawler.Event e, float startTime, float duration)
	{
		// Wait 'startTime' seconds before performing the given event
		yield return StartCoroutine (Wait (startTime));

		Debug.Log ("Perform event: " + e.type.ToString() + " on " + character.name + " in " + startTime + " seconds");

		// Perform the given event.
		if(e.type == Brawler.EventType.PerformAction)
		{
			PerformAction (e.actionToPerform);
		}
		else if(e.type == Brawler.EventType.PerformBasicAction)
		{
			PerformAction (e.basicActionToPerform);
		}
		else if(e.type == Brawler.EventType.SoundEffect)
		{
			// Play the sound effect specified by the event
			character.Sound.Play (e.soundEffect);
		}
		else if(e.type == Brawler.EventType.SlowMotion)
		{
			// TODO: Activate slow motion
		}
		else if(e.type == Brawler.EventType.ParticleEffect)
		{
			// If the particle effect should spawn at this character's position
			if(e.particleEvent.spawnPoint == ParticleSpawnPoint.Self)
			{
				// Calculate the position where the particles should spawn. They should spawn at this character's
				// position, plus the offset specified by the particle event
				Vector3 position = character.Transform.position;

				// Stores the offset of the particle effect, relative to the character's position
				Vector3 offset = e.particleEvent.offset;

				// If the character is facing left
				if(character.CharacterMovement.FacingDirection == Direction.Left)
					// Flip the x-direction of the particle effect's offset. This way, the offset is relative to the character
					offset.x *= -1;

				// Add the pre-computed offset to the particle effect's spawn position.
				position += offset;

				// Play the particle effect at the position computed above.
				ParticleManager.Instance.Play (e.particleEvent.effect, position);
			}
		}
		else if(e.type == Brawler.EventType.Die)
		{
			OnDeath();
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

		// If the character just finished performing his knockback action
		if(currentAction == actionSet.basicActions.GetBasicAction (BasicActionType.Knockback))
		{
			// If the character died while being knocked back
			if(character.CharacterStats.IsDead ())
			{
				// Play the death action after the knockback action, since the character died
				queuedAction = actionSet.basicActions.GetBasicAction (BasicActionType.Death);
			}
			// Else, if the character is still alive after the knockback
			else
			{
				// Play the rising animation after being knocked back since the character is not dead yet
				queuedAction = actionSet.basicActions.GetBasicAction (BasicActionType.KnockbackRise);
			}
		}
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
	/// Called the instant this character dies and is supposed to be erased from the screen. This is only
	/// called once the death animation completes and the character can disappear
	/// </summary>
	public void OnDeath()
	{
		// TODO: Optimize via pooling.
		GameObject.Destroy(this.gameObject);
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
	/// A coroutine that waits 'duration' seconds before returning 
	/// </summary>
	private IEnumerator Wait(float duration)
	{
		for(float timer = 0.0f; timer < duration; timer += Time.deltaTime)
			yield return null;
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