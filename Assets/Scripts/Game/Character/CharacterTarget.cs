using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Container which keeps track of empty GameObjects. These denote positions of interest for the character.
/// For instance, one may represent the position the player should be at in order to perform a melee
/// move on this character
/// </summary>
public class CharacterTarget : MonoBehaviour 
{
	/** The Character instance to which this script belongs. */
	private Character character;

	/** Dictionary which maps each target to a Transform. The Transform denotes the position of
	  * the desired target. */
	private Dictionary<Target,Transform> targets;

	void Start () 
	{
		// Cache the Character instance controlling this component. Avoids excessive runtime lookups
		character = GetComponent<Character>();

		// Cache the components and GameObjects required to create the 'targets' Dictionary
		Transform transform = character.Transform;
		Transform graphicsObject = transform.FindChild ("Graphics");
		Transform targetsParent = graphicsObject.FindChild("Targets"); // Parent of every target GameObject stored in the dictionary

		// Populate the dictionary which maps each target to a Transform which denotes the target's position
		targets = new Dictionary<Target,Transform>();
		targets[Target.Front] = targetsParent.FindChild ("Walk_Front");
		targets[Target.Back] = targetsParent.FindChild ("Walk_Back");
	}

	/// <summary>
	/// Returns the target that this character should walk to in order to reach the given character. 
	/// This is necessary because characters shouldn't walk directly to the center position of another
	/// character. That would put them in an awckward position. They should walk to the same y-position
	/// as the other character, but slightly to the left or right of him.
	/// </summary>
	public Target GetWalkTargetTo(Character targetCharacter)
	{
		// Stores the target this character will walk to in order to reach the given character
		Target targetToMoveTo = Target.None;

		//If the given character is facing the character performing the walking
		if(targetCharacter.CharacterMovement.IsFacing(this.character))
		{
			// Move this character to the front of his target to perform this action
			targetToMoveTo = Target.Front;
		}
		//Else, if the given character is not facing the player, move this character to the back of his target
		else
		{
			// Move this character to the back of his target
			targetToMoveTo = Target.Back;
		}
		// Return the target this character should walk to in order to reach the given target
		return targetToMoveTo;
	}

	/// <summary>
	/// Returns the Transform corresponding to the given target. In fact, each Target enumeration constant
	/// corresponds to a child Transform attached to this character. This function returns the Transform
	/// corresponding to the given target.
	/// </summary>
	public Transform GetTransform(Target target)
	{
		return targets[target];
	}

	/// <summary>
	/// Returns the position of the given target on this character.
	/// </summary>
	public Vector2 GetTargetPosition(Target target)
	{
		// Return the position of the target, as stored in the target<Target,Transform> dictionary.
		return targets[target].position;
	}
}

/// <summary>
/// Refers to a position of intereset on a character. For instance 'Target.MeleeFront' may refer to a 
/// child Transform of the character, which denotes the position another character should be to melee
/// the character from the front
/// </summary>
public enum Target
{
	Front,
	Back,
	None
}
