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
        Debug.Log("Touch to test: " + inputType + ", " + inputRegion + ", " + swipeDirection);

		// Cycle through each attack move in this move set
		for(int i = 0; i < attackMoves.Length; i++)
		{
			// Cache the attack move being cycled through
			MoveInfo move = attackMoves[i];

            Debug.Log("Move to test: " + move.inputType + ", " + move.inputRegion + ", " + move.swipeDirection + " = " + Equals(swipeDirection, move.swipeDirection));

			// If the given touch information matches the move's required input
			if(move.inputRegion == inputRegion && move.inputType == inputType
                && Equals(move.swipeDirection, swipeDirection))
			{
				// Return this attack move, since it can be performed given the input
				return move;
			}
		}

		// If this statement is reached, no move can be performed from the given input
		return null;
	}

    /// <summary>
    /// Returns true if the two swipe directions correspond to equivalent directions.
    /// For instance, two swipes are equivalent if one of them is 'SwipeDirection.Horizontal'
    /// and the other is 'SwipeDirection.Left'
    /// </summary>
    /// <returns></returns>
    public static bool Equals(SwipeDirection d1, SwipeDirection d2)
    {
        if(d1 == d2)
            return true;

        if (d1 == SwipeDirection.Horizontal)
        {
            if (d2 == SwipeDirection.Left || d2 == SwipeDirection.Right)
                return true;
        }
        else if (d2 == SwipeDirection.Horizontal)
        {
            if (d1 == SwipeDirection.Left || d1 == SwipeDirection.Right)
                return true;
        }

        if (d1 == SwipeDirection.Vertical)
        {
            if (d2 == SwipeDirection.Up || d2 == SwipeDirection.Down)
                return true;
        }
        else if (d2 == SwipeDirection.Vertical)
        {
            if (d1 == SwipeDirection.Up || d1 == SwipeDirection.Down)
                return true;
        }

        if (d1 == SwipeDirection.Any || d2 == SwipeDirection.Any)
        {
            if (d1 != SwipeDirection.None && d2 != SwipeDirection.None)
                return true;
        }

        // If this statement is reached, the swipe directions are not equal. Thus, return false
        return false;
        
    }
}
