using UnityEngine;
using System.Collections;

/// <summary>
/// Processes user input when the player is in combat mode by modifying
/// the game state
/// </summary>
public class CombatTouchProcessor : ITouchProcessor
{
	/** Stores the player character in order to alter his state according to user input. */
	private Character player;

	public CombatTouchProcessor()
	{
		// Caches the player character instance for efficiency
		// TODO: Inject dependency by passing player as constructor argument
		player = GameObject.Find ("Player").GetComponent<Character>();
	}

	public void OnClick(TouchInfo touch, GameObject gameObject)
	{
		// If the player has clicked empty space
		//if(gameObject == null)
			// Make the player move the position of his click
		player.CharacterMovement.MoveTo (touch.finalWorldPosition);

        // Inform the player's CharacterControl script that the character has swiped an object
        player.CharacterControl.OnTouch(touch, InputType.Click, gameObject, SwipeDirection.None);
	}

	public void LongPressUp(TouchInfo touch)
	{

	}

	public void ShortPressDown(TouchInfo touch)
	{

	}

	public void LongPress(TouchInfo touch)
	{

	}

	public void OnSwipe(TouchInfo touch, GameObject gameObject)
	{
		Debug.Log ("Swipe : " + touch.id + ", Direction: " + touch.GetSwipeDirection ().ToString ());
		//playerMeleeScript.Melee();

        // Inform the player's CharacterControl script that the character has swiped an object
        player.CharacterControl.OnTouch(touch, InputType.Swipe, gameObject, touch.GetSwipeDirection());
	}

}

