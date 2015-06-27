using UnityEngine;
using System.Collections;
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
	/// <param name="targetObject">The GameObject targetted by this action</param>
	/// /// <param name="targetPosition">The position targetted by this action</param>
	public void PerformAction(Action action, GameObject targetObject, Vector2 targetPosition)
	{
        // If the given action is null, ignore this call
        if (action == null)
            return;

		// If the character is not performing a action 
		// OR if the current action can be cancelled midway
		// OR if the given action can cancel any action
		if(currentAction == null || currentAction.cancelable || action.overrideCancelable)
		{
			// Play the animations required to perform the action
			character.CharacterAnimator.Play (action);
			// Play a sound when the action starts
			character.Sound.PlayRandomSound(action.startSounds);

			// Apply each force designated by the given action
			for(int i = 0; i < action.forces.Length; i++)
				ApplyForce(action.forces[i], action, targetObject, targetPosition);

			// Update the character's current action
			currentAction = action;
		}
	}

	/// <summary>
	/// Applies the given force on this character.
	/// </summary>
	/// <param name="action">The action which generated this force.</param>
	/// <param name="targetObject">The GameObject targetted by this action</param>
	/// /// <param name="targetPosition">The position targetted by this action</param>
	public void ApplyForce(Force force, Action action, GameObject targetObject, Vector2 targetPosition)
	{
		// Stores the time at which the force will start being applied
		float startTime = 0.0f;

		// Determines the time at which the force starts
		switch(force.startTime.type)
		{
		case DurationType.Frame:
			// The start time is specified by the n-th frame of the character's current animation
			startTime = force.startTime.nFrames / CharacterAnimator.FRAME_RATE;
			break;
		case DurationType.WaitForAnimationComplete:
			// The force will start when 'animationToWaitFor' is complete
			startTime = character.CharacterAnimator.GetEndTime(force.startTime.animationToWaitFor);
			break;
		}

		// Stores the amount of time the force lasts, in seconds
		float duration = 0.0f;

		// Determines the time at which the force starts
		switch(force.duration.type)
		{
		case DurationType.Frame:
			// The duration is specified by the amount of time elapsed from n frames of the character's current animation
			duration = force.duration.nFrames / CharacterAnimator.FRAME_RATE;
			break;
		case DurationType.WaitForAnimationComplete:
			// The force will end when 'animationToWaitFor' is complete
			duration = character.CharacterAnimator.GetEndTime(force.duration.animationToWaitFor) - startTime;
			break;
		}

		if(action.name == "Melee_Player")
			Debug.Log ("Start: " + startTime + ", Duration: " + duration);

		// Switch the force's type to determine the way the character should be affected by the force
		switch(force.forceType)
		{
		case ForceType.Position: // If the force requires to character to move to a designated position
			// If the force requires moving the character to the touched object
			if(force.target == TargetPosition.TouchedObject)
			{
				// Retrieve the Character component from the GameObject the character touched. The character will move towards this character
				Character characterTarget = targetObject.GetComponent<Character>();

				// Stores the target location the player will move to because of this force
				Target targetToMoveTo = Target.None;

				//If the character targetted by this action is facing the character performing this action
				if(characterTarget.CharacterMovement.IsFacing(character))
				{
					// Move the character to the front of his target to perform this action
					targetToMoveTo = Target.MeleeFront;
				}
				//Else, if the target character is not facing the player, the character will have to move to the back of his target to perform this action 
				else
				{
					// Move the character to the back of his target to perform this action
					targetToMoveTo = Target.MeleeBack;
				}

				// Get the position of the chosen target, which is stored as a child Transform on the character targetted by this action.
				// This is the position that the force will push this character to.
				targetPosition = characterTarget.CharacterTarget.GetTarget (targetToMoveTo);

				// Start the coroutine which moves the character to the target position designated by the force
				StartCoroutine (MoveTo (targetPosition,startTime,duration));
			}
			// Else, if the force requires the character to move to the position he touched to activate the force
			else if(force.target == TargetPosition.TouchedPosition)
			{
				// Start the coroutine which moves the character to his touched position
				StartCoroutine (MoveTo (targetPosition,startTime,duration));
			}
			break;
		case ForceType.Velocity: // If the force applies a constant velocity
			// Start the coroutine which moves the character at the given velocity for the specified amount of time
			StartCoroutine (ApplyVelocity (force.velocity,startTime,duration));
			break;
		}
	}

	/// <summary>
	/// Moves the character to the target position at the given times
	/// </summary>
	private IEnumerator MoveTo(Vector2 targetPosition, float startTime, float duration)
	{
		// Wait for 'startTime' seconds before applying the force
		yield return StartCoroutine (Wait (startTime));

		// Move the character to the designated position in the given amount of time
		character.CharacterMovement.MoveTo (targetPosition, duration);

	}

	/// <summary>
	/// Apply a constant velocity for the specified amount of time
	/// </summary>
	private IEnumerator ApplyVelocity(Vector2 velocity, float startTime, float duration)
	{
		// Wait for 'startTime' seconds before applying the force
		yield return StartCoroutine (Wait (startTime));
		
		// Move the character at the given velocity for the given amount of time
		character.CharacterMovement.SetVelocity (velocity, duration);
	}

	/** Waits for the given amount of seconds */
	private IEnumerator Wait(float duration)
	{
		// Yield every frame until the timer reached the designated number of seconds
		for(float timer = 0; timer < duration; timer += Time.deltaTime)
			yield return null;
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
		Action validAction = actionSet.GetValidAction(inputType, inputRegion, swipeDirection);

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
}