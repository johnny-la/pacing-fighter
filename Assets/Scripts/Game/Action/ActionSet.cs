using UnityEngine;
using System.Collections;

/// <summary>
/// The set of actions and moves a character can perform
/// </summary>
public class ActionSet : MonoBehaviour 
{
	// The character to which this action set belongs
	private Character character;

	/// <summary>
	/// A set of actions every character can perform by default. 
	/// </summary>
	public BasicActions basicActions = new BasicActions();

	/// <summary>
	/// The set of combat actions a character can perform.
	/// </summary>
	public Action[] combatActions = new Action[0];

	void Awake()
	{
		// Caches the Character component to which this action set is attached. Allows
		// the actions to know which character triggered them
		character = GetComponent<Character>();

		// Cycle through each of the character's combat actions
		for(int i = 0; i < combatActions.Length; i++)
		{
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
	public Action GetValidAction(InputType inputType, InputRegion inputRegion,
	                             SwipeDirection swipeDirection)
	{
        //Debug.Log("Touch: " + inputType + ", " + inputRegion + ", " + swipeDirection);

		// Cycle through each combat action present in this action set
		for(int i = 0; i < combatActions.Length; i++)
		{
			// Cache the attack move being cycled through
			Action action = combatActions[i];

            //Debug.Log("Move to test: " + action.inputType + ", " + action.inputRegion + ", " + action.swipeDirection + " = " + Equals(swipeDirection, action.swipeDirection));

			// If the given touch information matches the move's required input
			if((action.inputRegion == inputRegion || action.inputRegion == InputRegion.Any)
			    && action.inputType == inputType && Equals(action.swipeDirection, swipeDirection))
			{
				// Return this attack move, since it can be performed given the input
				return action;
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

/// <summary>
/// A set of actions every character can perform by default. 
/// </summary>
[System.Serializable]
public class BasicActions
{
	/** The basic actions any character can perform */
	public Action walk;
	public Action hit;

	/// <summary>
	/// Initialializes the basic moves' default properties. Accepts the Character which is performing these basic actions
	/// </summary>
	public void Init(Character character)
	{
		// Sets the default properties for the walking action
		walk.character = character;
		walk.cancelable = true;
		walk.inputType = InputType.Click;
		walk.inputRegion = InputRegion.Any;
		
		// Sets the default properties for the idle action
		hit.character = character;
		hit.cancelable = false;
		hit.overrideCancelable = true;
	}
	
	/// <summary>
	/// The default walking animation for the character
	/// </summary>
	public string WalkAnimation
	{
		get { return walk.animationSequences[0].animations[0]; }
		set { walk.animationSequences[0].animations[0] = value; }
	}
	
	/// <summary>
	/// The default hit animation for the character
	/// </summary>
	public string HitAnimation
	{
		get { return hit.animationSequences[0].animations[0]; }
		set { hit.animationSequences[0].animations[0] = value; }
	}
}