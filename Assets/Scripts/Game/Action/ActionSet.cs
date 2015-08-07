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

		// Create a new array of combat actions, which matches the number of ActionScriptableObjects dragged onto the inspector.
		combatActions = new Action[combatActionScriptableObjects.Length];

		// Cycle through each of the character's combat actions
		for(int i = 0; i < combatActionScriptableObjects.Length; i++)
		{
			// Retrieve the Action instance from the scriptable object. A duplicate is created to avoid two characters
			// referencing to the same HitBoxes, which would conflicts
			combatActions[i] = new Action(combatActionScriptableObjects[i].action);
			// Inform the action which character it belongs to. This character is the one performing the action being cycled through
			combatActions[i].character = character;

			if(combatActions[i].hitBoxes[0] != null)
			{
				//Debug.Log (combatActions[i].name);
				//Debug.Log (combatActions[i].hitBoxes[0].hitInfo.adversaryEvents.Length);

				if(combatActions[i].hitBoxes[0].hitInfo.adversaryEvents.Length >= 1)
					Debug.Log (combatActions[i].hitBoxes[0].hitInfo.adversaryEvents[0].type.ToString ());
			}
		}

		// Cycle through each of the character's combat actions once again
		for(int i = 0; i < combatActions.Length; i++)
		{
			// Create an array for linkableCombatActions that is the same size as the scriptableObjects array in order to populate it.
			combatActions[i].linkableCombatActions = new Action[combatActions[i].linkableCombatActionScriptableObjects.Length];

			// Cycle through each combat action that can be linked while this combat action is performed
			for(int j = 0; j < combatActions[i].linkableCombatActions.Length; j++)
			{
				// Store the 'Action' instance which corresponds to every scriptable object assigned as 
				// a linkable combat action. This avoids multiple runtime lookups
				combatActions[i].linkableCombatActions[j] = FindAction (combatActions[i].linkableCombatActionScriptableObjects[j]);
			}
		}

		// Tell the 'BasicActions' instance that its basic actions will be performed using this character (the character this script is attached to)
		basicActions.SetCharacter (character);
	}

	/// <summary>
	/// Returns an action from this move set that can performed from the given
	/// input
	/// </summary>
	public Action GetActionFromInput(InputType inputType, InputRegion inputRegion,
	                             SwipeDirection swipeDirection)
	{
        Debug.Log("Touch: " + inputType + ", " + inputRegion + ", " + swipeDirection);

		// Stores the action this character is currently performing
		Action currentAction = character.CharacterControl.CurrentAction;

		// If this character is currently performing an action, the action's linkable moves have priority over other moves
		if(currentAction != null)
		{
			Debug.Log("Current Action: " + currentAction.name + " Linkable moves : " + currentAction.linkableCombatActions.Length);
			// Cycle through each combat move that the character can link to from the current move
			for(int i = 0; i < currentAction.linkableCombatActions.Length; i++)
			{
				// Stores the action that can be linked from the character's current action
				Action linkableAction = currentAction.linkableCombatActions[i];

				Debug.Log("Test linkable action: " + linkableAction.name);

				// If the action which can be linked from the current action listens to input and can be performed
				if(linkableAction.listensToInput && CanPerform(linkableAction,inputType,inputRegion,swipeDirection))
				{
					// Return this linkable action, since it can be performed given the input, and given the current action
					// the character is performing
					return linkableAction;
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
				Debug.Log("Move to test: " + action.name + " " + action.inputType + ", " + action.inputRegion + ", " + action.swipeDirection 
				          + " = " + CanPerform(action, inputType, inputRegion, swipeDirection));
				
				// If the given touch information satisfies the action's required input
				if(CanPerform (action,inputType,inputRegion,swipeDirection))
				{
					// Return this attack move, since it can be performed given the input
					return action;
				}
			}
		}

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
	/// Returns the Action instance corresponding to the given ActionScriptableObject. An ActionScriptableObject
	/// is a serializable container for an action. When an ActionSet is created, it creates a new Action instance
	/// for each ActionScriptableObject it is given in the inspector. Therefore, a search must be done to find the
	/// Action instance corresponding to the given scriptable object.
	/// Note: Runs in O(n) time, where n is the number of combat actions assigned to this action set
	/// </summary>
	public Action FindAction(ActionScriptableObject actionScriptableObject)
	{
		// TODO: Create a dictionary which maps ActionScriptableObjects to Actions for quicker searching.
		// Cycle through each scriptableObject for the character's combat actions. The given scriptable object should be
		// contained in this list, if the character was assigned this action in the inspector
		for(int i = 0; i < combatActionScriptableObjects.Length; i++)
		{
			// If the element being iterated through is equal to the given actionScriptableObject, the correct action
			// has been found in the character's action-set
			if(combatActionScriptableObjects[i] == actionScriptableObject)
			{
				// Return the corresponding combatAction, which is stored in the same index as the 
				return combatActions[i];
			}
		}

		// If this statement is reached, no actions corresponding to the given scriptable object could be found. Return null
		return null;
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