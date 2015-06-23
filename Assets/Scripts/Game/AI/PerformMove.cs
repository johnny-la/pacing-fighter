using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class PerformMove : BehaviorDesigner.Runtime.Tasks.Action
{
	/** The move that will be performed when this action is executed in the behaviour tree. */
	public Action move;

	/** Stores the zombie instance which is performing the melee action. */
	private Character zombie;

	public override void OnAwake()
	{
		// Cache the character component for the GameObject executing this action
		zombie = transform.GetComponent<Character>();
	}

	public override void OnStart()
	{
		// Attack
		zombie.CharacterControl.PerformAction (move);
	}
	
	public override TaskStatus OnUpdate()
	{
		// The melee move has been performed in OnStart. Thus, return success
		return TaskStatus.Success;
	}
}
