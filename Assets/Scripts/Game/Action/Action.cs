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
		forces = ArrayUtils.Copy<Force>(template.forces);

		listensToInput = template.listensToInput;
		inputType = template.inputType;
		inputRegion = template.inputRegion;
		swipeDirection = template.swipeDirection;

		startSounds = ArrayUtils.Copy<AudioClip>(template.startSounds);
		impactSounds = ArrayUtils.Copy<AudioClip>(template.impactSounds);

		cancelable = template.cancelable;
		overrideCancelable = template.overrideCancelable;
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
/// A force applied on the character whilst performing a move 
/// </summary>
[System.Serializable]
public class Force
{
	/// <summary>
	/// Determines whether the force is specified by a velocity or a target position
	/// </summary>
	public ForceType forceType;

	/// <summary>
	/// The velocity at which the character moves. Only used if this force is specified by a velocity.
	/// </summary>
	public Vector2 velocity;

	/// <summary>
	/// Specifies the type of target the character is trying to move towards. Used if 'forceType=Position'
	/// </summary>
	public TargetPosition target = TargetPosition.None;

	/// <summary>
	/// The target position that the force will move an entity to. Only used if 'target==TargetPosition.CustomPosition'
	/// </summary>
	public Vector2 customTargetPosition;

	/// <summary>
	/// The event to perform once the force is done being applied. For instance, if the event requires an action
    /// to be performed, the action is performed by the same entity that was affected by this force
	/// </summary>
	public Brawler.Event onCompleteEvent = new Brawler.Event();

	/// <summary>
	/// If true, the entity performing this action will face towards his TargetPosition when this force is applied
	/// Note: Only applies when 'target != TargetPosition.None'
	/// </summary>
	public bool faceTarget = false;

	/// <summary>
	/// The time at which the force activates
	/// </summary>
	public CastingTime startTime = new CastingTime();
	
	/// <summary>
	/// The amount of time the force is active.
	/// </summary>
	public CastingTime duration = new CastingTime();

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
	/// The amount of time the force is applied, in frames. Only used if 'durationType' is set to 'Frames'
	/// </summary>
	public int nFrames;
}

/// <summary>
/// The type of force applied onto a character. Either he can move at a specified velocity,
/// or move towards a specified location when the force is applied
/// </summary>
public enum ForceType
{
	Velocity,
	Position // Move towards a specified target position
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
	UsePhysicsData
}

/// <summary>
/// Denotes the target position the character moves towards when a force is applied on him
/// </summary>
public enum TargetPosition
{
	TouchedObject,
	TouchedPosition,
	CustomPosition,
	None
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