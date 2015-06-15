using UnityEngine;
using System.Collections;

public class MoveSet : MonoBehaviour 
{
	/// <summary>
	/// The set of attack moves a character can perform.
	/// </summary>
	public MoveInfo[] attackMoves = new MoveInfo[0];

	/// <summary>
	/// Returns a move from this move set that can performed from the given
	/// input
	/// </summary>
	public MoveInfo GetValidMove(InputType inputType, InputRegion inputRegion,
	                             SwipeDirection swipeDirection)
	{
		// Cycle through each attack move in this move set
		for(int i = 0; i < attackMoves.Length; i++)
		{
			MoveInfo move = attackMoves[i];

			// If the given touch information matches the move's required input
			if(move.inputRegion == inputRegion && move.inputType == inputType
			   && move.swipeDirection == swipeDirection)
			{
				// Return this attack move, since it can be performed given the input
				return move;
			}
		}
	}
}
