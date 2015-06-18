using UnityEngine;
using System.Collections;
using Brawler;


public class CharacterControl : MonoBehaviour 
{
	/** Caches the Character component that this script is controlling. */
	private Character character;

	/** The move set containing the moves the character can perform */
	private MoveSet moveSet;

	/** The move the character is currently performing */
	private MoveInfo currentMove;
		
	public void Awake()
	{
		// Caches the Character component this script is controlling
		character = GetComponent<Character>();

		// Cache the character's components for easy access
		moveSet = GetComponent<MoveSet>();
	}

	/// <summary>
	/// Performs the given move
	/// </summary>
	/// <param name="move">Move.</param>
	public void PerformMove(MoveInfo move)
	{
        // If the given move is null, ignore this call
        if (move == null)
            return;

		// Play the animations required to perform the move
		character.CharacterAnimator.Play (move);

		// Update the move the 
		currentMove = move;
	}
		
	/// <summary>
	/// Called when the user performs a touch that is interpreted
	/// as being performed by this character. Depending on the input
	/// and the character's move set, an appropriate action will be
	/// performed
	/// </summary>
	public void OnTouch(TouchInfo touch, InputType inputType, GameObject pressedObject,
		                SwipeDirection swipeDirection)
	{
		// Stores the region which was pressed by this touch
		InputRegion inputRegion = InputRegion.EmptySpace;
			
		// If the touch did not press a GameObject
		if(pressedObject == null)
		{
			// Inform the touch it has pressed empty space
			inputRegion = InputRegion.EmptySpace;
		}
		// Else, if the touch did not press any object
		else if(pressedObject != null)
		{
			// If the touch pressed the same GameObject that received this event 
			if(pressedObject == gameObject)
			{
				// The GameObject touched itself
				inputRegion = InputRegion.Self;
			}
			// Else, if the touch pressed an enemy
			else if(pressedObject.layer == Layer.Enemy)
			{
				// Inform the touch that it pressed an enemy
				inputRegion = InputRegion.Enemy;
			}
		}
			
		// Retrieve a move that can be performed from the given touch
		MoveInfo validMove = moveSet.GetValidMove(inputType, inputRegion, swipeDirection);

        // Perform the move which corresponds to the user's input
        PerformMove(validMove);
	}
}