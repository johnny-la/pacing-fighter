using UnityEngine;
using System.Collections;

/// <summary>
/// Data container for an action
/// </summary>
[System.Serializable]
public class Action
{
	/// <summary>
	/// The action's identifier, used for debugging purposes
	/// </summary>
	public string name;

	/// <summary>
	/// The character who is performing this action.
	/// </summary>
	[System.NonSerialized]
	public Character character;

	/// <summary>
	/// The GameObject this action is targetting. May be null if the action does not target any GameObject.
	/// </summary>
	[System.NonSerialized]
	public GameObject targetObject;
	
	/// <summary>
	/// The position this action is targetting. May be zero, if the action does not require a target position.
	/// </summary>
	[System.NonSerialized]
	public Vector2 targetPosition = Vector2.zero;

	/// <summary>
	/// The possible animation sequences played
	/// when the action is performed
	/// </summary>
	public AnimationSequence[] animationSequences = new AnimationSequence[1]{new AnimationSequence()};

	/// <summary>
	/// The hit boxes which can harm the enemy when this move is performed.
	/// </summary>
	public HitBox[] hitBoxes = new HitBox[0];

	/// <summary>
	/// The forces applied on the character that performs this action
	/// </summary>
	public Force[] forces = new Force[0];

	/// <summary>
	/// The events performed right when the action starts being performed. The character which performs
	/// this action is the same one performing these events 
	/// </summary>
	public Brawler.Event[] onStartEvents = new Brawler.Event[0];

	/// <summary>
	/// The combat actions which can cancel this action and be performed instead. Useful for creating combos.
	/// Note that any combat action can be performed after this action. However, if a move is in this list,
	/// it has higher priority, and will be chosen first.
	/// </summary>
	public ActionScriptableObject[] linkableCombatActionScriptableObjects = new ActionScriptableObject[0];
	public Action[] linkableCombatActions = new Action[0];

	/// <summary>
	/// If true, the move can be performed through user input. If false, the move is performed only through code
	/// </summary>
	public bool listensToInput = false;

	/// <summary>
	/// The type of input required to activate the move (tap/swipe)
	/// </summary>
	public InputType inputType;

	/// <summary>
	/// The region to touch to activate the move
	/// </summary>
	public InputRegion inputRegion;

	/// <summary>
	/// The swipe direction required to activate the move.
	/// </summary>
	public SwipeDirection swipeDirection;

	/// <summary>
	/// The sounds which can play when this action is activated
	/// </summary>
	public AudioClip[] startSounds = new AudioClip[0];
	
	/// <summary>
	/// The sounds which can be played when this action hits an adversary
	/// </summary>
	public AudioClip[] impactSounds = new AudioClip[0];

	/// <summary>
	/// If true, this move can be canceled midway to perform another move or action. 
	/// </summary>
	public bool cancelable;

	/// <summary>
	/// If true, this move can interrupt any move, and be performed instead.
	/// It can even cancel a move marked as 'cancelable'
	/// </summary>
	public bool overrideCancelable;

	/// <summary>
	/// Default constructor. Use when the fields will be populated by the inspector.
	/// </summary>
	public Action()
	{
	}

	/// <summary>
	/// Create an Action and copy all the values from the given template
	/// </summary>
	public Action(Action template)
	{
		// Copy the values from the given template
		name = template.name;

		animationSequences = ArrayUtils.Copy<AnimationSequence>(template.animationSequences);
		hitBoxes = ArrayUtils.DeepCopy(template.hitBoxes);
		forces = ArrayUtils.DeepCopy(template.forces);
		onStartEvents = ArrayUtils.DeepCopy(template.onStartEvents);
		linkableCombatActionScriptableObjects = ArrayUtils.Copy<ActionScriptableObject>(template.linkableCombatActionScriptableObjects);

		listensToInput = template.listensToInput;
		inputType = template.inputType;
		inputRegion = template.inputRegion;
		swipeDirection = template.swipeDirection;

		startSounds = ArrayUtils.Copy<AudioClip>(template.startSounds);
		impactSounds = ArrayUtils.Copy<AudioClip>(template.impactSounds);

		cancelable = template.cancelable;
		overrideCancelable = template.overrideCancelable;
	}

	/// <summary>
	/// Update the action's events so that their member variables correctly correspond to the action's
	/// current state. For instance, some CameraMovement events require the camera to look at the position
	/// which was touched when the event was triggered. In this case, the touched position of each CameraMovement
	/// has to be updated to correspond to the position of the touch that triggered the CameraMovement event.
	/// </summary>
	public void UpdateEvents()
	{
		// Cycle through each 'onStartEvent' in the action
		for(int i = 0; i < onStartEvents.Length; i++)
		{
			Brawler.Event e = onStartEvents[i];

			// Update the event so that its member variables are set to the correct information
			UpdateEvent (e);
		}
		
		// Cycle through each 'Force' applied in the action
		for(int i = 0; i < forces.Length; i++)
		{
			// Retrieve the force's 'OnComplete' event
			Brawler.Event e = forces[i].onCompleteEvent;
			
			// Update the event so that its member variables are set to the correct information
			UpdateEvent (e);
		}
		
		// Cycle through each 'onStartEvent' in the action
		for(int i = 0; i < hitBoxes.Length; i++)
		{
			// Stores the events performed on the same character that performed this action.
			Brawler.Event[] selfEvents = hitBoxes[i].hitInfo.selfEvents;
			
			// Cycle through each 'selfEvent' for the hit 
			for(int j = 0; j < selfEvents.Length; j++)
			{
				Brawler.Event e = selfEvents[j];
				
				// Update the event so that its member variables are set to the correct information
				UpdateEvent (e);
			}

			// Stores the events performed by the adversary hit by the hit box.
			Brawler.Event[] adversaryEvents = hitBoxes[i].hitInfo.adversaryEvents;
			
			// Cycle through each event for the adversary hit by this hit box has to perform
			for(int j = 0; j < adversaryEvents.Length; j++)
			{
				Brawler.Event e = adversaryEvents[j];
				
				// Update the event so that its member variables are set to the correct information
				UpdateEvent (e);
			}
			
		}
	}

	/// <summary>
	/// Update the given event. Updates the event's member variables to the correct info. For instance,
	/// if the event's type is set to 'CameraMovement', the target position of the camera must be updated.
	/// For instance, if the camera must move to the position of the targetted enemy, the event must be
	/// updated to store the Transform or position of this enemy
	/// </summary>
	private void UpdateEvent(Brawler.Event e)
	{
		// If the event is a CameraMovement event
		if(e.type == Brawler.EventType.CameraMovement)
		{
			// If the camera movement requires the camera to move to the location of the event which triggered the event
			if(e.cameraMovement.target == TargetPosition.Self)
				// Set the camera to follow this character's Transform
				e.cameraMovement.targetTransform = character.Transform;
			// Else, if the camera needs to move to the position of the object that was touched
			else if(e.cameraMovement.target == TargetPosition.TouchedObject)
				// Set the camera to follow the targetObject which is targetted by the action that activated this event
				e.cameraMovement.targetTransform = targetObject.transform;
			// Else, if the camera needs to move to the position where the user touched to activate this event
			else if(e.cameraMovement.target == TargetPosition.TouchedPosition)
				// Tell the camera to move to the action's 'targetPosition', the position where the user last touched
				e.cameraMovement.targetPosition = targetPosition;
		}
	}
}

/// <summary>
/// A container for a sequence of consecutively-played animations
/// </summary>
[System.Serializable]
public class AnimationSequence
{
	/** The animations which are played consecutively */
	public string[] animations = new string[1]{""};

	/** If true, the last animation in this sequence is set to loop. */
	public bool loopLastAnimation = false;
}

/// <summary>
/// Denotes the time at which a certain event starts or lasts.
/// The time can be specified to last as long as an animation, 
/// or simply be specified to last a certain number of frames.
/// </summary>
[System.Serializable]
public class CastingTime
{
	/// <summary>
	/// The type of duration used to specify this time. Does it last as long as an animation, or does it
	/// last a certain number of frames?
	/// </summary>
	public DurationType type;

	/// <summary>
	/// The force will last as long as this animation. The animation is specified by its number, as
	/// given in the "Animation Sequences" dropdown.
	/// </summary>
	public int animationToWaitFor;

	/// <summary>
	/// The amount of time denoted by this instance, in frames. Only used if 'durationType' is set to 'Frames'
	/// </summary>
	public int nFrames;

	/// <summary>
	/// The amount of time specified by this CastingTime instance, in seconds. Only used if 'durationType' is set to 'Seconds'
	/// </summary>
	public float seconds;
}

/// <summary>
/// Specifies the way in which a duration is specified. Does this duration last as long as an animation,
/// or does it last a certain number of frames?
/// </summary>
public enum DurationType
{
	Frame,
	/** The force lasts as long as the duration of a specified animation */
	WaitForAnimationComplete, 
	/** Use the character's default physics data when making him move. The force will last as long as it takes for his physics values to get him there */
	UsePhysicsData,
	Seconds
}

/// <summary>
/// Denotes a target position. This may denot where a character moves towards when a force is applied on him, 
/// or where the camera follows when a CameraMovement event is triggered.
/// </summary>
public enum TargetPosition
{
	None,
	TouchedObject,
	TouchedPosition,
	Self,
	CustomPosition
}

/// <summary>
/// Denotes the type of input required to perform a move (tap or swipe)
/// </summary>
public enum InputType
{
	Click,
	Swipe
}

/// <summary>
/// Denotes the region that needs to be touched to perform a move
/// </summary>
public enum InputRegion
{
	EmptySpace,
	Enemy,
	Self,
	Any
}