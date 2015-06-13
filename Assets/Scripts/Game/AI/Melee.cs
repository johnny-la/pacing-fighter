using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class Melee : Action
{
	/** Stores the zombie instance which is performing the melee action. */
	private ZombieMelee zombieMeleeScript;

	public override void OnAwake()
	{
		// Cache the character component for the GameObject executing this action
		zombieMeleeScript = transform.GetComponent<ZombieMelee>();
	}

	public override void OnStart()
	{
		// Attack
		zombieMeleeScript.Melee();
	}
	
	public override TaskStatus OnUpdate()
	{
		// The melee move has been performed in OnStart. Thus, return success
		return TaskStatus.Success;
	}
}
