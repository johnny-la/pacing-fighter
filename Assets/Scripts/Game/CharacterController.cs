using UnityEngine;
using System.Collections;

namespace Brawler
{
	public class CharacterController : MonoBehaviour 
	{
		/** The move set containing the moves the character can perform */
		private MoveSet moveSet;

		public void Awake()
		{
			// Cache the character's components for easy access
			moveSet = GetComponent<MoveSet>();
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
			InputRegion inputRegion;

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

		}
	}
}
