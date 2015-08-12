using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

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
	ObstacleAvoidance,
	Separation,
	Alignment,
	Cohesion,
	None
}

public class Steer : BehaviorDesigner.Runtime.Tasks.Action
{
	/** The steerable component attached to the GameObject performing this action. */
	private Steerable steerable;

	// The steering behaviors to apply every frame
	public SteeringBehavior[] steeringBehaviors;

	// The condition that must be met for this action to return 'Success'
	public StoppingCondition stoppingCondition;

	public override void OnAwake()
	{
		// Cache the 'Steerable' component attached to the GameObject performing this action
		steerable = transform.GetComponent<Steerable>();

		// Set the stopping condition's transform to the same Transform that is performing this 'Steer' action.
		// This way, the stopping condition will be tested using this GameObject's position
		stoppingCondition.SetTransform(base.transform); 
	}

	public override void OnStart()
	{
		// Reset the stopping condition. The stopping condition now knows that the 'Steer' action just started.
		stoppingCondition.Init();
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
				steerable.AddSeekForce (steeringBehavior.targetTransform.Value.position, steeringBehavior.strengthMultiplier);
				break;
			case SteeringBehaviorType.Arrival:
				steerable.AddArrivalForce (steeringBehavior.targetTransform.Value.position, steeringBehavior.slowingRadius, steeringBehavior.strengthMultiplier);
				break;
			case SteeringBehaviorType.Pursue:
				steerable.AddPursueForce (steeringBehavior.targetSteerable, steeringBehavior.strengthMultiplier);
				break;
			case SteeringBehaviorType.Flee:
				steerable.AddFleeForce (steeringBehavior.targetTransform.Value.position, steeringBehavior.strengthMultiplier);
				break;
			case SteeringBehaviorType.Evade:
				steerable.AddEvadeForce (steeringBehavior.targetSteerable, steeringBehavior.strengthMultiplier);
				break;
			case SteeringBehaviorType.Wander:
				steerable.AddWanderForce (steeringBehavior.circleDistance, steeringBehavior.circleRadius, 
				                          steeringBehavior.angleChange, steeringBehavior.strengthMultiplier);
				break;
			case SteeringBehaviorType.ObstacleAvoidance:
				steerable.AddObstacleAvoidanceForce (steeringBehavior.obstacleAvoidanceForce, steeringBehavior.maxObstacleViewDistance, 
				                                     steeringBehavior.obstacleLayer, steeringBehavior.strengthMultiplier);
				break;
			case SteeringBehaviorType.Separation:
				steerable.AddSeparationForce(steeringBehavior.strengthMultiplier);
				break;
			case SteeringBehaviorType.Alignment:
				steerable.AddAlignmentForce (steeringBehavior.strengthMultiplier);
				break;
			case SteeringBehaviorType.Cohesion:
				steerable.AddCohesionForce (steeringBehavior.strengthMultiplier);
				break;
			}
		}
	
		// Apply the forces on the steerable that is performing this action
		steerable.ApplyForces (Time.deltaTime);

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
	/// Before the steering force is added, it is multiplied by this value so that all steering behaviors can
	/// be controlled in terms of their impact on the steerable's final velocity.
	/// </summary>
	public float strengthMultiplier = 1.0f;
	
	/// <summary>
	/// The steerable that this steering behavior is targetting
	/// </summary>
	public Steerable targetSteerable;
	/// <summary>
	/// The transform that this steering behavior is targetting
	/// </summary>
	public SharedTransform targetTransform;
	
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

	/// .................................,
	///   OBSTACLE AVOIDANCE PROPERTIES  . 
	/// .................................,

	/// <summary>
	/// The magnitude of the obstacle avoidance force. The higher this value, the faster it is to avoid obstacles and the less
	/// chance there is of hitting them.
	/// </summary>
	public float obstacleAvoidanceForce;

	/// <summary>
	/// The length (in meters) of the line of sight used for obstacle avoidance. If this value is large, obstacles from very
	/// far away from a steerable can be seen. For instance, if set to '2', obstacles as far as 2 meters from the steerable
	/// can be seen and avoided
	/// </summary>
	public float maxObstacleViewDistance;

	/// <summary>
	/// Only colliders on this layer will be avoided if applying the 'ObstacleAvoidance' steering behavior.
	/// </summary>
	public LayerMask obstacleLayer;
	
}

/// <summary>
/// The condition that must be met for a 'Steer' action to return 'Success'
/// </summary>
[System.Serializable]
public class StoppingCondition
{
	[BehaviorDesigner.Runtime.Tasks.Tooltip("The target that the steerable must reach for the 'Steer' action to return success.")]
	public SharedTransform target;

	[BehaviorDesigner.Runtime.Tasks.Tooltip("The distance this steerable must be from his target for the 'Steer' action to return success.")]
	public SharedFloat stoppingDistance;

	/** Caches the squared stopping distance for efficiency purposes. */
	private float stoppingDistanceSquared;

	[BehaviorDesigner.Runtime.Tasks.Tooltip("Once the 'Steer' action is active, 'waitTime' seconds will elapse before" +
		"the action returns 'Success'. If this is set to zero, the 'Steer' action will run until the target is reached.")]
	public SharedFloat waitTime;

	/** The amount of time that has passed since the 'Steer' action started. */
	private float timeElapsed;

	/** The Transform component that is performing the 'Steer' action this instance belongs to. The stopping condition
	 *  tested against this Transform's position. */
	private Transform transform;

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
	/// Initializes the stopping condition whenever the 'Steer' action starts.
	/// </summary>
	public void Init()
	{
		// Caches the squared stopping distance for efficiency purposes
		stoppingDistanceSquared = stoppingDistance.Value * stoppingDistance.Value;
		
		// Reset the amount of time elapsed to zero.
		timeElapsed = 0;
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
			if((transform.position - target.Value.position).sqrMagnitude <= stoppingDistanceSquared)
			{
				// Return true, since the stopping condition has been met
				return true;
			}
		}

		// If the wait time is zero, the 'Steer' action can run for an infinite amount of time. If it is nonzero, the 
		// 'Steer' action can be performed for at most 'waitTime' seconds.
		if(waitTime.Value != 0)
		{	
			// If at least 'waitTime' seconds have elapsed since the 'Steer' action started
			if(timeElapsed >= waitTime.Value)
			{
				// Return true, since the stopping condition was met, and the 'Steer' action this stopping condition controls should stop
				return true;
			}
		}

		// If this statement is reached, the stopping condition has not yet been met. Thus, return 'false'
		return false;
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
