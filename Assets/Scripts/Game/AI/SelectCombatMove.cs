using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// Selects a random combat move from the character's action list and stores it in a shared variable.
/// The combat move is chosen from the character being controlled by this behaviour tree.
/// </summary>

public class SelectCombatMove : BehaviorDesigner.Runtime.Tasks.Action
{
	/** Stores the chosen combat move */
	public SharedObject combatMove;
	
	/** Stores the character instance from which a combat action is selected. */
	private Character character;
	
	public override void OnAwake()
	{
		// Cache the character component for the GameObject executing this action
		character = transform.GetComponent<Character>();
	}
	
	public override void OnStart()
	{
		// Choose a random combat action from this character's action set
		combatMove.SetValue (ArrayUtils.RandomElement (character.CharacterControl.ActionSet.combatActionScriptableObjects));
	}
	
	public override TaskStatus OnUpdate()
	{
		// The melee move has been performed in OnStart. Thus, return success
		return TaskStatus.Success;
	}
}
