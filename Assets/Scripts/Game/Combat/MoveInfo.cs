using UnityEngine;
using System.Collections;

/// <summary>
/// Data container for a move 
/// </summary>
public class MoveInfo : ScriptableObject
{
	/// <summary>
	/// The possible animation sequences played
	/// when the move is performed
	/// </summary>
	public AnimationSequence[] animationSequences = {new AnimationSequence()};

}

/// <summary>
/// A container for a sequence of consecutively-played animations
/// </summary>
public class AnimationSequence
{
	/** The animations which are played consecutively */
	public string[] animations = new string[]{""};
}
