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
		targets[Target.MeleeFront] = targetsParent.FindChild ("Melee_Front");
		targets[Target.MeleeBack] = targetsParent.FindChild ("Melee_Back");
	}

	/// <summary>
	/// Returns the position of the given target on this character.
	/// </summary>
	public Vector2 GetTarget(Target target)
	{
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
	MeleeFront,
	MeleeBack,
	None
}
