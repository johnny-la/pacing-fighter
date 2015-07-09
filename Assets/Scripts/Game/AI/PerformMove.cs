using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class PerformMove : BehaviorDesigner.Runtime.Tasks.Action
{
	/** The move that will be performed when this action is executed in the behaviour tree. */
	public SharedObject actionScriptableObject;

	/** Stores the action to perform. This is simply the 'actionScriptableObject' variable casted to the appropriate class. */
	private ActionScriptableObject actionToPerform;

	/** Stores the character instance which is performing the melee action. */
	private Character character;

	public override void OnAwake()
	{
		// Cache the character component for the GameObject executing this action
		character = transform.GetComponent<Character>();
	}

	public override void OnStart()
	{
		// Casts the inspector object into an ActionScriptableObject. This is the action the character will perform
		actionToPerform = (ActionScriptableObject) (actionScriptableObject.Value);

		// Perform the action
		character.CharacterControl.PerformAction (actionToPerform.action);
	}
	
	public override TaskStatus OnUpdate()
	{
		// The melee move has been performed in OnStart. Thus, return success
		return TaskStatus.Success;
	}
}
