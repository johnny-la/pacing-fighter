using UnityEngine;
using System.Collections;

/// <summary>
/// Data container for an action
/// </summary>
[System.Serializable]
public class Action : ScriptableObject
{
	/// <summary>
	/// The action's identifier, used for debugging purposes
	/// </summary>
	public string name;

	/// <summary>
	/// The character who is set to perform this action instance.
	/// </summary>
	[HideInInspector]
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

}

/// <summary>
/// A container for a sequence of consecutively-played animations
/// </summary>
[System.Serializable]
public class AnimationSequence
{
	/** The animations which are played consecutively */
	public string[] animations = new string[1]{""};
}

/// <summary>
/// A force applied on the character whilst performing a move 
/// </summary>
[System.Serializable]
public class Force
{
	/// <summary>
	/// The time at which the force activates
	/// </summary>
	public CastingTime castingFrame;

	/// <summary>
	/// The amount of time the force is active.
	/// </summary>
	public CastingTime duration;

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
	public Target target;

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
	public DurationType durationType;

	/// <summary>
	/// The force will last as long as this animation. The animation is specified by its number, as
	/// given in the "Animation Sequences" dropdown.
	/// </summary>
	public int animationToWaitFor;

	/// <summary>
	/// The amount of time the force is applied, in frames. Only used if 'durationType' is set to 'Frames'
	/// </summary>
	public int duration;
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
	AnimationComplete, // The force lasts as long as a specified animation
	Frames
}

/// <summary>
/// Denotes the target position the character moves towards when a force is applied on him
/// </summary>
public enum Target
{
	TouchedObject,
	TouchPosition,
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