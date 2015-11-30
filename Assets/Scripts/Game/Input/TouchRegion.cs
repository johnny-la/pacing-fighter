using UnityEngine;
using System.Collections;

// TODO: Create a custom inspector which displays only one draggable component (Character, InteractableObject, etc.)
// depending on the TouchRegion's 'objectType'.

/// <summary>
/// A component attached to a collider which can be touched by the user. That is, every GameObject on the 'TouchRegion' 
/// layer can be detected by a raycast from a touch point to the game. If this GameObject is touched, this component is
/// used to determine which type of object was touched (i.e., was a Character touched, or was an interactable object touched?)
/// </summary>
public class TouchRegion : MonoBehaviour 
{
	/// <summary>
	/// Stores the type of object that this touch region belongs to. For instance, if 'objectType == Enemy', then the 
	// 'TouchInfo' class will know that it touched an enemy by accessing this member variable.
	/// </summary>
	public ObjectType objectType;

	/// <summary>
	/// The character component that this TouchRegion belongs to. Can be null if this TouchRegion collider does not belong to a character
	/// </summary>
	public Character character;
}

/// <summary>
/// The type of object a TouchRegion belongs to. This enum is used to answer the question: Does the TouchRegion denote
/// the clickable hit box for a player, an enemy, or something else? 
/// </summary>
public enum ObjectType
{
	Player,
	Enemy
}
