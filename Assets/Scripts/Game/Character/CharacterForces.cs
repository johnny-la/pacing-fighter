﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the forces acting on the character
/// </summary>
public class CharacterForces : MonoBehaviour 
{
	/** Stores the character this component belongs to. */
	private Character character;

	/** Stores the force currently being applied on the character. */
	private Force currentForce;

	void Awake () 
	{
		// Cache the character this component belongs to, for efficiency purposes
		character = GetComponent<Character>();
	}

	/// <summary>
	/// Perform the action by moving the character according to the action's forces
	/// </summary>
	public void Play(Action action, GameObject touchedObject, Vector2 touchPosition)
	{
		// Apply each force designated by the given action
		for(int i = 0; i < action.forces.Length; i++)
			ApplyForce(action.forces[i], touchedObject, touchPosition);
	}

	/// <summary>
	/// Called when the character has reached one of its move targets. This is raised by the 'MoveToTarget' script
	/// when the character has reached one of its targets set by calling the 'CharacterMovement.MoveTo()' function.
	/// </summary>
	public void OnTargetReached()
	{
		// If a force is being applied on the character, the force has just finished being applied on the character
		// since the character has just reached his move target, which was set by the current force.
		if(currentForce != null)
		{
			// Inform this script that the current force has just ended so that any necessary further actions can be taken.
			OnForceEnd (currentForce);

			// The current force has just finished being applied. Thus, set it to null
			currentForce = null;
		}
	}

	/// <summary>
	/// Called when the force is done being applied on the character. If the force has any events that should 
	/// occur once it has ended, they will be performed here.
	/// </summary>
	public void OnForceEnd(Force force)
	{
		// Stores the action that should be completed once the given force is done being applied
		Brawler.Event eventOnComplete = force.onCompleteEvent;

		Debug.Log("Force is done being applied on " + character.name);

		// If the event requires an action to be performed
		if(eventOnComplete.type == Brawler.EventType.PerformAction)
		{
			// Make the character affected by this force perform the action that should be performed once the force is done being applied
			character.CharacterControl.PerformAction (eventOnComplete.actionToPerform.action);
		}
		// Else, if the event requires a basic action to be performed
		else if(eventOnComplete.type == Brawler.EventType.PerformBasicAction)
		{
			// Retrieve the Action instance which denotes the basic action to perform once the given force is done being applied.
			Action basicAction = character.CharacterControl.ActionSet.basicActions.GetBasicAction(eventOnComplete.basicActionToPerform);

			// Make the character affected by this force perform the basic action specified by the given force
			character.CharacterControl.PerformAction (basicAction);
		}

		// If the given force is the same as the force being currently applied on the character
		if(currentForce == force)
			// Nullify the current force acting on the character, since the force has just finished being applied if this statement is reached.
			currentForce = null;
	}
	
	/// <summary>
	/// Applies the given force on this character.
	/// </summary>
	/// <param name="touchedObject">The GameObject targetted by this action</param>
	/// <param name="touchPosition">The touch position when this action was activated</param>
	public void ApplyForce(Force force, GameObject touchedObject, Vector2 touchPosition)
	{
		// Stores the time at which the force will start being applied
		float startTime = GetStartTime(force);
		
		// Stores the amount of time the force lasts, in seconds
		float duration = GetDuration (force, startTime);

		// The position where the force will move the character
		Vector2 targetPosition = Vector2.zero;
			
		// Stores the direction in which the character will face once the force is applied. By default, the character keeps his current direction
		Direction newFacingDirection = character.CharacterMovement.FacingDirection;
		
		// If the force requires to character to move to a designated position
		if(force.forceType == ForceType.Position)
		{
			
			// If the force requires moving the character to the touched object
			if(force.target == TargetPosition.TouchedObject)
			{
				// Retrieve the Character component from the GameObject the character touched. The character will move towards this character
				Character characterTarget = touchedObject.GetComponent<Character>();
				
				// Get the position this character must move to in order to reach the 'characterTarget'
				Target targetToMoveTo = this.character.CharacterTarget.GetWalkTargetTo (characterTarget);
				targetPosition = characterTarget.CharacterTarget.GetTargetPosition (targetToMoveTo);
				
				// If the force makes the character look at his target
				if(force.faceTarget)
				{
					// If the target is to the right of the character, make the character face to the right
					if (characterTarget.Transform.position.x > transform.position.x)
						newFacingDirection = Direction.Right;
					// Else, if the target is to the left of the character, make the character face left
					else
						newFacingDirection = Direction.Left;
				}
			}
			// Else, if the force requires the character to move to a target position
			else if(force.target == TargetPosition.TouchedPosition || force.target == TargetPosition.CustomPosition)
			{
				// If the force requires the character look at his target position
				if(force.faceTarget)
				{
					// If the force needs to move this entity to the touch position which activated this force
					if(force.target == TargetPosition.TouchedPosition)
						// The force should move the character to the touch position which activated this force
						targetPosition = touchPosition;
					// Else, if this force moves this entity to a custom position
					else if(force.target == TargetPosition.CustomPosition)
						// The force's target position is specified by the custom position field in this force instance
						targetPosition = force.customTargetPosition;

					// If the character's target position is to the right of him, make him face right
					if (targetPosition.x > transform.position.x)
						newFacingDirection = Direction.Right;
					// Else, if the target position is to the left of the character, make him left
					else
						newFacingDirection = Direction.Left;
				}
			}
			
		}

		// Start the coroutine which applies the given force using the properties calculated above
		StartCoroutine (ApplyForceCoroutine (force, targetPosition, startTime, duration, newFacingDirection));
	}

	/// <summary>
	/// Applies the given force. Moves the character to given position or applies a velocity depending on the
	/// type of the given force.
	private IEnumerator ApplyForceCoroutine(Force force, Vector2 targetPosition, float startTime, float duration, Direction facingDirection)
	{
		// Wait for 'startTime' seconds before applying the force
		yield return StartCoroutine (Wait (startTime));

		// Update the current force being applied on the character
		currentForce = force;
		
		// Move the character to the designated position in the given amount of time
		character.CharacterMovement.MoveTo (targetPosition, facingDirection);

		// If the force requires to character to move to a designated position
		if(force.forceType == ForceType.Position)
		{
			// If the character should move at the speed of his walking speed, make the force last as long as it takes
			// for his walking speed to get him there
			if(force.duration.type == DurationType.UsePhysicsData)
			{
				// Move the character to his target position using his physics-defined walking speeds
				character.CharacterMovement.MoveTo (targetPosition,facingDirection);
			}
			// Else, if the force lasts 'duration' amount of seconds instead of using the character's physics values
			else
			{
				// Move the character to his target position in the specified amount of time
				character.CharacterMovement.MoveTo (targetPosition,duration,facingDirection);
			}
		}
		// Else, if the force applies a constant velocity
		else if(force.forceType == ForceType.Velocity)
		{
			// Set the character's velocity to a constant velocity for the specified amount of time
			character.CharacterMovement.SetVelocity (force.velocity,duration);
		}

		// If the duration type of the force is not set to 'UsePhysicsData', the 'duration' float is specified.
		// Thus, wait a set amount of seconds before calling 'OnForceEnd()'. Otherwise, the 'OnForceEnd()' will
		// be manually called once the force is done being applied
		if(force.duration.type != DurationType.UsePhysicsData)
		{
			// Wait 'duration' seconds for the force to finish be applied
			yield return StartCoroutine (Wait (duration));
			
			// Set the current force to null, since no more forces are being applied on the character, since the last force just ended
			currentForce = null;
			
			// Inform this CharacterForces instance that this force is done being applied. If the force has any 'OnEnd' events, they will be performed
			OnForceEnd(force);
		}
	}
	
	/// <summary>
	/// Returns the time at which the force starts relative to the current time. That is, if this method returns '0.5',
	/// the force will start being applied in 0.5 seconds
	/// </summary>
	private float GetStartTime(Force force)
	{
		// Determines the time at which the force starts
		switch(force.startTime.type)
		{
		case DurationType.Frame:
			// The start time is specified by the n-th frame of the character's current animation
			return force.startTime.nFrames / CharacterAnimator.FRAME_RATE;
		case DurationType.WaitForAnimationComplete:
			// The force will start when 'animationToWaitFor' is complete
			return character.CharacterAnimator.GetEndTime(force.startTime.animationToWaitFor);
		default:
			Debug.LogError ("Incorrect start time specified for a force: " + force.startTime.type.ToString ());
			break;
		}

		// If this statement is reached, the force does not specify the time at which it should start. Return a default of zero.
		return 0;
	}

	/// <summary>
	/// Returns the amount of time for which the force is applied.
	/// </summary>
	private float GetDuration(Force force, float startTime)
	{
		// Determines the time at which the force starts
		switch(force.duration.type)
		{
		case DurationType.Frame:
			// The duration is specified by the amount of time elapsed from n frames of the character's current animation
			return force.duration.nFrames / CharacterAnimator.FRAME_RATE;
		case DurationType.WaitForAnimationComplete:
			// The force will end when 'animationToWaitFor' is complete
			return character.CharacterAnimator.GetEndTime(force.duration.animationToWaitFor) - startTime;
		case DurationType.UsePhysicsData:
			// Use the character's walking physics values when applying the force. The force will last as long as it
			// takes for his walking speed to get him there
			break;
		}

		// If this statement is reached, the force object does not directly specify its duration. The force
		// instead uses the character's physics values, and the force will last as long as it takes for the
		// user to reach its target. Thus, return 0 since the force has no specified duration
		return 0.0f;
	}

	
	/** Waits for the given amount of seconds */
	private IEnumerator Wait(float duration)
	{
		// Yield every frame until the timer reached the designated number of seconds
		for(float timer = 0; timer < duration; timer += Time.deltaTime)
			yield return null;
	}

	/// <summary>
	/// The force currently acting on the character. 
	/// </summary>
	public Force CurrentForce
	{
		get { return currentForce; }
		set { currentForce = value; }
	}
}