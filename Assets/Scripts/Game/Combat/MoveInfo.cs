using UnityEngine;
using System.Collections;

/// <summary>
/// Data container for a move 
/// </summary>
[System.Serializable]
public class MoveInfo : ScriptableObject
{
	/// <summary>
	/// The possible animation sequences played
	/// when the move is performed
	/// </summary>
	public AnimationSequence[] animationSequences = new AnimationSequence[0];

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
}

/// <summary>
/// A container for a sequence of consecutively-played animations
/// </summary>
[System.Serializable]
public class AnimationSequence
{
	/** The animations which are played consecutively */
	public string[] animations = new string[0];
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
	Self
}