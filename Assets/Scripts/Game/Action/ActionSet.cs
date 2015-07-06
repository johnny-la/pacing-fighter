using UnityEngine;
using System.Collections;

/// <summary>
/// The set of actions and moves a character can perform
/// </summary>
public class ActionSet : MonoBehaviour 
{
	// The character to which this action set belongs
	[System.NonSerialized] 
	private Character character;

	/// <summary>
	/// A set of actions every character can perform by default. 
	/// </summary>
	public BasicActions basicActions = new BasicActions();

	/// <summary>
	/// The set of combat actions a character can perform. These are assets created in the project panel.
	/// </summary>
	public ActionScriptableObject[] combatActionScriptableObjects = new ActionScriptableObject[0];
	/// <summary>
	/// The combat actions a character can perform. These are Action instances, the raw data container for an action.
	/// </summary>
	public Action[] combatActions = new Action[0];

	void Awake()
	{
		// Caches the Character component to which this action set is attached. Allows
		// the actions to know which character triggered them
		character = GetComponent<Character>();

		// Cycle through each of the character's combat actions
		for(int i = 0; i < combatActionScriptableObjects.Length; i++)
		{
			// Retrieve the Action instance from the scriptable object. This contains the action's properties
			combatActions[i] = combatActionScriptableObjects[i].action;
			// Inform the action which character it belongs to. This character is the one performing the action being cycled through
			combatActions[i].character = character;
		}

		// Initialize the properties of the character's basic actions
		basicActions.Init (character);
	}

	/// <summary>
	/// Returns an action from this move set that can performed from the given
	/// input
	/// </summary>
	public Action GetActionFromInput(InputType inputType, InputRegion inputRegion,
	                             SwipeDirection swipeDirection)
	{
        Debug.Log("Touch: " + inputType + ", " + inputRegion + ", " + swipeDirection);

		// Cycle through each basic action present in this action set
		for(int i = 0; i < basicActions.actions.Length; i++)
		{
			// Cache the basic move being cycled through
			Action action = basicActions.actions[i];

			// If the action listens to user input to be performed, check if the given input satisfies the action's requirements
			if(action.listensToInput)
			{
				Debug.Log("Action to test: " + action.name + ": " + action.inputType + ", " + action.inputRegion + ", " 
				          + action.swipeDirection + "... Can perform? " + CanPerform(action,inputType,inputRegion,swipeDirection));
				
				// If the given touch information satisfies the action's required input
				if(CanPerform (action,inputType,inputRegion,swipeDirection))
				{
					// Return this basic action, since it can be performed given the input
					return action;
				}
			}
		}

		// Cycle through each combat action present in this action set
		for(int i = 0; i < combatActions.Length; i++)
		{
			// Cache the attack move being cycled through
			Action action = combatActions[i];

			// If the action can be performed through user input, check if the given input satisfies the action's requirements
			if(action.listensToInput)
			{
				Debug.Log("Move to test: " + action.inputType + ", " + action.inputRegion + ", " + action.swipeDirection + " = " + Equals(swipeDirection, action.swipeDirection));

				// If the given touch information satisfies the action's required input
				if(CanPerform (action,inputType,inputRegion,swipeDirection))
				{
					// Return this attack move, since it can be performed given the input
					return action;
				}
			}
		}

		// If this statement is reached, no move can be performed from the given input
		return null;
	}

	/// <summary>
	/// Returns true if this action can be performed with the given input.
	/// </summary>
	private bool CanPerform(Action action, InputType inputType, InputRegion inputRegion, SwipeDirection swipeDirection)
	{
		// Tests whether the input corresponds to the action's properties
		bool validInputRegion = Equals(action.inputRegion, inputRegion);
		bool validInputType = action.inputType == inputType; 
		bool validSwipeDirection = (action.inputType == InputType.Swipe)? Equals(action.swipeDirection, swipeDirection):true;

		// Return true if all of the given input matches the action's required input
		return validInputRegion && validInputType && validSwipeDirection;
	}

	/// <summary>
	/// Returns true if the two input regions are equivalent. That is, they both correspond to the same region
	/// </summary>
	public bool Equals(InputRegion r1, InputRegion r2)
	{
		if(r1 == r2)
			return true;

		if(r1 == InputRegion.Any || r2 == InputRegion.Any)
			return true;

		// If this statement is reached, the two input regions are not equivalent. Thus, return false
		return false;
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