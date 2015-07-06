using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class ChooseCombatMove : BehaviorDesigner.Runtime.Tasks.Action
{
	/** Stores the chosen combat move */
	public SharedVariable combatMove;
	
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
		combatMove.SetValue (ArrayUtils.RandomElement (character.CharacterControl.ActionSet.combatActions);
	}
	
	public override TaskStatus OnUpdate()
	{
		// The melee move has been performed in OnStart. Thus, return success
		return TaskStatus.Success;
	}
}
