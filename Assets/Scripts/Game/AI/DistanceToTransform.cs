using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class DistanceToTransform : BehaviorDesigner.Runtime.Tasks.Action
{
	/// <summary>
	/// The transform to find the distance to.
	/// </summary>
	public SharedTransform targetTransform;

	/// <summary>
	/// The shared variable in which the distance is stored 
	/// </summary> 
	public SharedFloat storeDistance;

	public override TaskStatus OnUpdate()
	{
		// Return failure if the target transform has not been set
		if(targetTransform.Value == null)
			return TaskStatus.Failure;

		// Store the distance between this Transform and the target inside the 'storeDistance' variable.
		storeDistance.Value = Vector2.Distance (transform.position, targetTransform.Value.position);

		// Return success since the distance has been computed.
		return TaskStatus.Success;
	}
}

