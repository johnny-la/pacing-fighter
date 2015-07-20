using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class Steer : BehaviorDesigner.Runtime.Tasks.Action
{
	/** The steerable component attached to the GameObject performing this action. */
	private Steerable steerable;

	// The steering behaviors to perform
	public SteeringBehavior[] steeringBehaviors;

	// The condition that must be met for this action to return 'Success'
	public StoppingCondition stoppingCondition;

	public override void OnAwake()
	{
		// Cache the 'Steerable' component attached to the GameObject performing this action
		steerable = transform.GetComponent<Steerable>();

		// Set the stopping condition's transform to the same Transform that is performing this 'Steer' action.
		// This way, the stopping condition will be tested against this GameObject's position
		stoppingCondition.SetTransform(transform); 
	}

	public override void OnStart()
	{
		// Reset the stopping condition. The stopping condition now knows that the 'Steer' action just started.
		stoppingCondition.Reset();
	}

	public override TaskStatus OnUpdate()
	{
		// Cycle through each steering behavior to apply on this GameObject
		for(int i = 0; i < steeringBehaviors.Length; i++)
		{
			// Stores the steering behavior to apply
			SteeringBehavior steeringBehavior = steeringBehaviors[i];

			// Switch the type of the steering behavior and add it to the GameObject
			switch(steeringBehavior.type)
			{
			case SteeringBehaviorType.Seek:
				steerable.AddSteeringForce (steeringBehavior.type, steeringBehavior.targetTransform.position);
				break;
			case SteeringBehaviorType.Arrival:
				steerable.AddSteeringForce (steeringBehavior.type, steeringBehavior.targetTransform.position, steeringBehavior.slowingRadius);
				break;
			case SteeringBehaviorType.Pursue:
				steerable.AddSteeringForce (steeringBehavior.type, steeringBehavior.targetSteerable);
				break;
			case SteeringBehaviorType.Flee:
				steerable.AddSteeringForce (steeringBehavior.type, steeringBehavior.targetTransform.position);
				break;
			case SteeringBehaviorType.Evade:
				steerable.AddSteeringForce (steeringBehavior.type, steeringBehavior.targetSteerable);
				break;
			case SteeringBehaviorType.Wander:
				steerable.AddSteeringForce (steeringBehavior.type, steeringBehavior.circleDistance, 
				                            steeringBehavior.circleRadius, steeringBehavior.angleChange);
				break;
			}
		}
	
		// Apply the forces on the steerable that is performing this action
		steerable.ApplyForces ();

		// If the stopping condition has been met, return success
		if(stoppingCondition.Complete ())
		{
			// Once the stopping condition is met, stop the steerable movement
			steerable.StopMoving();

			return TaskStatus.Success;
		}
		// Else, if the stopping condition has not yet been met
		else 
		{
			// Return 'TaskStatus.Running', so that the 'Steer' action can run until the stopping condition is met
			return TaskStatus.Running;
		}
	}
}

/// <summary>
/// Denotes a steering behavior that a Steerable instance can perform
/// </summary>
[System.Serializable]
public class SteeringBehavior
{
	/// <summary>
	/// Stores the type of steering behavior to apply
	/// </summary>
	public SteeringBehaviorType type;
	
	/// <summary>
	/// The steerable that this steering behavior is targetting
	/// </summary>
	public Steerable targetSteerable;
	/// <summary>
	/// The transform that this steering behavior is targetting
	/// </summary>
	public Transform targetTransform;
	
	/// ......................,
	///   ARRIVAL PROPERTIES  . 
	/// ......................,
	
	/// <summary>
	/// When the entity gets this close to his target, he will start slowing down. Only applies to the 'Arrival' behavior
	/// </summary>
	public float slowingRadius;
	
	/// .....................,
	///   WANDER PROPERTIES  . 
	/// .....................,
	
	/// <summary>
	/// The distance from the entity to the wander circle. The greater this value, the stronger the wander force, 
	/// and the more likely the entity will change directions. 
	/// </summary>
	public float circleDistance;
	/// <summary>
	/// The greater the radius, the stronger the wander force, and the more likely the entity will change directions
	/// </summary>
	public float circleRadius;
	/// <summary>
	/// The maximum angle in degrees that the wander force can change next frame
	/// </summary>
	public float angleChange;
	
}

/// <summary>
/// The types of steering behaviors a Steerable instance can perform
/// </summary>
public enum SteeringBehaviorType
{
	Seek,
	Arrival,
	Pursue,
	Flee, 
	Evade,
	Wander,
	None
}

/// <summary>
/// The condition that must be met for a 'Steer' action to return 'Success'
/// </summary>
[System.Serializable]
public class StoppingCondition
{
	/** The Transform component that is performing the 'Steer' action this instance belongs to. The stopping condition
	 *  tested against this Transform's position. */
	private Transform transform;

	[BehaviorDesigner.Runtime.Tasks.Tooltip("The target that the steerable must reach for the 'Steer' action to return success.")]
	public Transform target;

	[BehaviorDesigner.Runtime.Tasks.Tooltip("The distance this steerable must be from his target for the 'Steer' action to return success.")]
	public float stoppingDistance;

	/** Caches the squared stopping distance for efficiency purposes. */
	private float stoppingDistanceSquared;

	[BehaviorDesigner.Runtime.Tasks.Tooltip("Once the 'Steer' action is active, 'waitTime' seconds will elapse before" +
		"the action returns 'Success'. If this is set to zero, the 'Steer' action will run until the target is reached.")]
	public float waitTime;

	/** The amount of time that has passed since the 'Steer' action started. */
	private float timeElapsed;

	/// <summary>
	/// Call this every frame that the 'Steer.OnUpdate()' function is called. The stopping condition is updated
	/// so that the 'Steer' action can stop at the right time
	/// </summary>
	public void Update()
	{
		// Update the amount of time that the 'Steer' action was active for
		timeElapsed += Time.deltaTime;
	}

	/// <summary>
	/// Returns true if the stopping condition has been met and the 'Steer' action should return 'Success'
	/// </summary>
	public bool Complete()
	{
		// If the stopping condition is met when the GameObject reaches his target
		if(target != null)
		{
			// If this GameObject is at least 'stoppingDistance' units away from his target
			if((transform.position - target.position).sqrMagnitude <= stoppingDistanceSquared)
			{
				// Return true, since the stopping condition has been met
				return true;
			}
		}

		// If the wait time is zero, the 'Steer' action can run for an infinite amount of time. If it is nonzero, the 
		// 'Steer' action can be performed for at most 'waitTime' seconds.
		if(waitTime != 0)
		{	
			// If at least 'waitTime' seconds have elapsed since the 'Steer' action started
			if(timeElapsed >= waitTime)
			{
				// Return true, since the stopping condition was met, and the 'Steer' action this stopping condition controls should stop
				return true;
			}
		}

		// If this statement is reached, the stopping condition has not yet been met. Thus, return 'false'
		return false;
	}

	/// <summary>
	/// Resets the stopping condition for when the 'Steer' action first starts.
	/// </summary>
	public void Reset()
	{
		// Caches the squared stopping distance for efficiency purposes
		stoppingDistanceSquared = stoppingDistance * stoppingDistance;

		// Reset the amount of time elapsed to zero.
		timeElapsed = 0;
	}

	/// <summary>
	/// Sets the transform whose position is tested against the 'target' Transform to test if the stopping condition is met.
	/// Set this to the same Transform that is performing the 'Seek' action.
	/// </summary>
	public void SetTransform(Transform transform)
	{
		this.transform = transform;
	}
}
