using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Container which keeps track of empty GameObjects. These denote positions of interest for the character.
/// For instance, one may represent the position the player should be at in order to perform a melee
/// move on this character
/// </summary>
public class CharacterAnchor : MonoBehaviour 
{
	/** The Character instance to which this script belongs. */
	private Character character;

	/** Dictionary which maps each anchor to a Transform. The Transform denotes the position of
	  * the desired anchor. */
	private Dictionary<Anchor,Transform> anchors;

	void Start () 
	{
		// Cache the Character instance controlling this component. Avoids excessive runtime lookups
		character = GetComponent<Character>();

		// Cache the components and GameObjects required to create the 'anchors' Dictionary
		Transform transform = character.Transform;
		Transform graphicsObject = transform.FindChild ("Graphics");
		Transform anchorParent = graphicsObject.FindChild("Anchors"); // Parent of every anchor GameObject stored in the dictionary

		// Populate the dictionary which maps each anchor to a Transform which denotes the anchor's position
		anchors = new Dictionary<Anchor,Transform>();
		anchors[Anchor.Front] = anchorParent.FindChild ("Walk_Front");
		anchors[Anchor.Back] = anchorParent.FindChild ("Walk_Back");
		anchors[Anchor.DamageLabelSpawn] = anchorParent.FindChild ("Damage_Label_Spawn");
	}

	/// <summary>
	/// Returns the anchor that this character should walk to in order to reach the given character. 
	/// This is necessary because characters shouldn't walk directly to the center position of another
	/// character. That would put them in an awckward position. They should walk to the same y-position
	/// as the other character, but slightly to the left or right of him.
	/// </summary>
	public Anchor GetWalkTargetTo(Character targetCharacter)
	{
		// Stores the anchor this character will walk to in order to reach the given character
		Anchor anchorToMoveTo = Anchor.None;

		//If the given character is facing the character performing the walking
		if(targetCharacter.CharacterMovement.IsFacing(this.character))
		{
			// Move this character to the front of his anchor to perform this action
			anchorToMoveTo = Anchor.Front;
		}
		//Else, if the given character is not facing the player, move this character to the back of his anchor
		else
		{
			// Move this character to the back of his anchor
			anchorToMoveTo = Anchor.Back;
		}
		// Return the anchor this character should walk to in order to reach the given anchor
		return anchorToMoveTo;
	}

	/// <summary>
	/// Returns the Transform corresponding to the given anchor. In fact, each anchor enumeration constant
	/// corresponds to a child Transform attached to this character. This function returns the Transform
	/// corresponding to the given anchor.
	/// </summary>
	public Transform GetTransform(Anchor anchor)
	{
		return anchors[anchor];
	}

	/// <summary>
	/// Returns the position of the given anchor on this character.
	/// </summary>
	public Vector2 GetAnchorPosition(Anchor anchor)
	{
		// Return the position of the anchor, as stored in the anchor<anchor,Transform> dictionary.
		return anchors[anchor].position;
	}
}

/// <summary>
/// Refers to a position of intereset on a character. For instance 'anchor.MeleeFront' may refer to a 
/// child Transform of the character, which denotes the position another character should be to melee
/// the character from the front
/// </summary>
public enum Anchor
{
	None,
	Front,
	Back,
	DamageLabelSpawn
}
