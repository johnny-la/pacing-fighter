using UnityEngine;
using System.Collections;

// TODO: (Optimization) There are a lot of local Vector2s created in method calls and garbage collected
// once the method returns. Get rid of these local instantiations/deallocations to reduce GC overhead.

/// <summary>
/// Manages the steering behaviours for the GameObject this script is attached to.
/// (All steering behaviors implemented according to the tutorials on GameDevelopment Tuts+)
/// </summary>
public class Steerable : MonoBehaviour
{
	/** The maximum speed at which the GameObject can move */
	public float maxSpeed;

	/** The maximum amount of steering force that can be applied on this entity every frame. */
	public float maxForce;

	/** The angle used in the 'Wander' steering behavior. Denotes an angle relative to the entity's 'wander circle'.
	  * Updated every frame the wander behavior is applied */
	private float wanderAngle;
	
	/** Stores the steering force to be applied on this object this frame. 
	  * This vector accumulates all steering forces before applying it to
	  * this GameObject's velocity. */
	private Vector2 steeringForce = Vector2.zero;
	
	/** Cache this GameObject's components for efficiency purposes. */
	private new Transform transform;
	private new Rigidbody2D rigidbody;

	private Vector2 dampVelocity = Vector2.zero;
	private float timeToReachDesiredVelocity = 0.5f;
	
	void Awake()
	{
		// Cache this GameObject's Transform component
		transform = GetComponent<Transform>();
		rigidbody = GetComponent<Rigidbody2D>();
	}

	public void OnDrawGizmos()
	{
		/*Gizmos.color = Color.green;
		Gizmos.DrawLine((Vector2)Transform.position, (Vector2)transform.position + rigidbody.velocity);
		Gizmos.color = Color.red;
		Gizmos.DrawLine((Vector2)Transform.position, (Vector2)transform.position + steeringForce);*/
	}

	/// <summary>
	/// Applies all the steering forces set by the AddSteeringForce() function. Must be called every
	/// frame for the steering behaviors to update correctly. 
	/// </summary>
	public void ApplyForces()
	{
		// TODO Optimize. Truncate returns a new Vector2
		// Clamp the steering force's magnitude to 'maxForce'. This way, the steering force can never act too dramatically.
		steeringForce = steeringForce.Truncate(maxForce); 
		// Divide the steering force by this steerable's mass. This way, the higher the mass, the slower the steerable turns.
		steeringForce /= rigidbody.mass;

		// Add the steering force to the rigidbody's current velocity, and clamp his new velocity
		// to his 'maxSpeed' constant
		/*rigidbody.velocity = Vector2.SmoothDamp((Vector2)rigidbody.velocity, 
		                                        Vector2.ClampMagnitude(rigidbody.velocity + steeringForce, maxSpeed),
		                                        ref dampVelocity, timeToReachDesiredVelocity).SetMagnitude(maxSpeed);*/
		rigidbody.velocity = (rigidbody.velocity + steeringForce).Truncate(maxSpeed);

		// Reset the steering forces back to zero.
		steeringForce = Vector2.zero;
	}

	/// <summary>
	/// Cancel all forces on the Steerable and stop its movement
	/// </summary>
	public void StopMoving()
	{
		// Set the steering forces to zero
		steeringForce = Vector2.zero;
		// Stop the Steerable from moving.
		rigidbody.velocity = Vector2.zero;
	}

	/// <summary>
	/// Adds the desired steering behavior force to the GameObject. This method must be called every
	/// frame in order to apply the steering behavior continuously. Call ApplyForces() every frame once
	/// all steering forces have been added to this steerable entity.
	/// Can apply following steering behaviors: Seek, Flee
	/// </summary>
	/// <param name="steeringBehavior">The steering behavior to apply.</param>
	/// <param name="targetPosition">The position to seek or to flee.</param>
	public void AddSteeringForce(SteeringBehaviorType steeringBehavior, Vector2 targetPosition)
	{
		// Switch the desired steering behavior, and add the associated force to the total steering force
		switch(steeringBehavior)
		{
		case SteeringBehaviorType.Seek:
			steeringForce += SeekForce(targetPosition);
			break;
		case SteeringBehaviorType.Flee:
			steeringForce += FleeForce (targetPosition);
			break;
		default:
			Debug.LogError("AddSteeringForce(SteeringBehavior, targetPosition:Vector2) called" +
			               "for an invalid steering behavior: " + steeringBehavior.ToString () +
			               "This method can only be called for the following: Evade, Flee");
			break;
		}
	}

	/// <summary>
	/// Adds the desired steering behavior force to the GameObject. This method must be called every
	/// frame in order to apply the steering behavior continuously. Call ApplyForces() every frame once
	/// all steering forces have been added to this steerable entity.
	/// Can apply following steering behaviors: Arrival
	/// </summary>
	/// <param name="steeringBehavior">The steering behavior to apply.</param>
	/// <param name="targetPosition">The position to seek or to flee.</param>
	/// /// <param name="slowingRadius">The seekable will slow down once he gets within 'slowingRadius' units of his target position.</param>
	public void AddSteeringForce(SteeringBehaviorType steeringBehavior, Vector2 targetPosition, float slowingRadius)
	{
		// Switch the desired steering behavior, and add the associated force to the total steering force
		switch(steeringBehavior)
		{
		case SteeringBehaviorType.Arrival:
			steeringForce += ArrivalForce(targetPosition, slowingRadius);
			break;
		default:
			Debug.LogError("AddSteeringForce(SteeringBehavior, targetPosition:Vector2) called" +
			               "for an invalid steering behavior: " + steeringBehavior.ToString () +
			               "This method can only be called for the following: Arrival");
			break;
		}
	}

	/// <summary>
	/// Adds the desired steering behavior force to the GameObject. This method must be called every
	/// frame in order to apply the steering behavior continuously. Call ApplyForces() every frame once
	/// all steering forces have been added to this steerable entity.
	/// Can apply following steering behaviors: Pursue, Evade
	/// </summary>
	/// <param name="steeringBehavior">The steering behavior to apply.</param>
	/// <param name="steerableTarget">The steerable entity to pursue or evade. Sometimes a Steerable instance
	/// is needed for certain steering behaviors, instead of a target position.</param>
	public void AddSteeringForce(SteeringBehaviorType steeringBehavior, Steerable steerableTarget)
	{
		// Switch the desired steering behavior, and add the associated force to the total steering force
		switch(steeringBehavior)
		{
		case SteeringBehaviorType.Pursue:
			steeringForce += PursueForce (steerableTarget);
			break;
		case SteeringBehaviorType.Evade:
			steeringForce += EvadeForce (steerableTarget);
			break;
		default:
			Debug.LogError("AddSteeringForce(SteeringBehavior, targetPosition:Vector2) called" +
			               "for an invalid steering behavior: " + steeringBehavior.ToString () +
			               "This method can only be called for the following: Pursue, Evade");
			break;
		}
	}

	/// <summary>
	/// Adds the desired steering behavior force to the GameObject. This method must be called every
	/// frame in order to apply the steering behavior continuously. Call ApplyForces() every frame once
	/// all steering forces have been added to this steerable entity.
	/// Can apply following steering behaviors: Wander
	/// </summary>
	/// <param name="steeringBehavior">The steering behavior to apply.</param>
	/// <param name="circleDistance">The distance from the entity to the wander circle. The greater this value
	/// , the stronger the wander force, and the more likely the entity will change directions.</param>
	/// <param name="circleRadius">The greater the radius, the stronger the wander force, and the more likely
	/// the entity will change directions.</param>
	/// <param name="angleChange">The maximum angle in degrees that the wander force can change next frame.</param>
	public void AddSteeringForce(SteeringBehaviorType steeringBehavior, float circleDistance, float circleRadius, float angleChange)
	{
		// Switch the desired steering behavior, and add the associated force to the total steering force
		switch(steeringBehavior)
		{
		case SteeringBehaviorType.Wander:
			steeringForce += WanderForce (circleDistance, circleRadius, angleChange);
			break;
		default:
			Debug.LogError("AddSteeringForce(SteeringBehavior, targetPosition:Vector2) called" +
			               "for an invalid steering behavior: " + steeringBehavior.ToString () +
			               "This method can only be called for the following: Wander");
			break;
		}
	}

	/// <summary>
	/// Returns the steering force required to make this GameObject seek the given target
	/// </summary>
	public Vector2 SeekForce(Vector2 target)
	{
		// Compute the distance vector to the target position
		Vector2 desiredVelocity = (Vector2)target - (Vector2)transform.position;
		// Store the distance from this entity to his target position
		float distanceToTarget = desiredVelocity.magnitude;

		// Compute the direction the GameObject must travel to reach his target
		desiredVelocity = desiredVelocity.normalized;

		// Calculate the velocity at which this object should move to reach his target
		desiredVelocity *= maxSpeed;

		// Compute the steering force applied to make this object seek his target
		Vector2 steeringForce = desiredVelocity - rigidbody.velocity;
		
		return steeringForce;
		
	}

	/// <summary>
	/// Returns the steering force required to make this GameObject arrive at the given target. This steering behavior is 
	/// identical to the 'Seek' behavior, except that the steerable slows down and stops when he reaches his target.
	/// </summary>
	/// <param name="slowingRadius">When the entity gets this close to his target, he will start slowing down.</param>
	public Vector2 ArrivalForce(Vector2 target, float slowingRadius)
	{
		// Compute the distance vector to the target position
		Vector2 desiredVelocity = (Vector2)target - (Vector2)transform.position;
		// Store the distance from this entity to his target position
		float distanceToTarget = desiredVelocity.magnitude;
		
		// Compute the direction the GameObject must travel to reach his target
		desiredVelocity = desiredVelocity.normalized;
		
		// Calculate the velocity at which this object should move to reach his target
		desiredVelocity *= maxSpeed;
		
		// If this entity is less than 'slowingRadius' units away from his target, slow down this steerable so that he doesn't overshoot his target
		if(distanceToTarget < slowingRadius)
		{
			// Multiply the desired velocity by 'distanceToTarget/slowingRadius'. This way, the closer this steerable is to his
			// target, the slower the steering force. This way, the steerable will slow down as he gets closer to his target.
			desiredVelocity *= (distanceToTarget / slowingRadius);
		}
		
		// Compute the steering force applied to make this object arrive at his target
		Vector2 steeringForce = desiredVelocity - rigidbody.velocity;
		
		return steeringForce;
	}
	
	/** Returns a steering force which veers this object towards the given target's future position.
	  * This can be seen as a more intelligent version of the 'Seek' behaviour */
	public Vector2 PursueForce(Steerable target)
	{
		// Compute the distance from this GameObject to his target
		float distanceToTarget = (target.Transform.position - Transform.position).magnitude;
		// Calculate the amount of time required for this object to reach his target
		float timeToReachTarget = distanceToTarget / target.MaxSpeed;
		
		// Determines the target's future position, in 'timeToReachTarget' amount of time
		// This is the position this object will try to pursue to stay ahead of his target
		Vector2 futureTargetPosition = (Vector2)target.Transform.position + target.Rigidbody.velocity * timeToReachTarget;
		
		// The resulting steering force is the 'Seek' force to the target's future location
		Vector2 pursueForce = SeekForce(futureTargetPosition);
		
		return pursueForce;
	}
	
	/** Returns a steering force which makes this objec flee the given position */
	public Vector2 FleeForce(Vector2 fleeTarget)
	{
		// The fleeing force is simply the negative of the seek force to the given fleeTarget.
		return -SeekForce(fleeTarget);
	}

	/// <summary>
	/// Returns a steering force which makes this object evade the given target.
	/// The target's SteeringManager component is passed to access the target's
	/// max speed easily.
	/// </summary>
	public Vector2 EvadeForce(Steerable targetToEvade)
	{
		// Compute the distance from this GameObject to his target to evade
		float distanceToTarget = (targetToEvade.Transform.position - Transform.position).magnitude;
		// Calculate the amount of time required for this object to reach his target
		float timeToReachTarget = distanceToTarget / targetToEvade.MaxSpeed;
		
		// Determines the target's future position, in 'timeToReachTarget' amount of time
		// This is the position this object will try to evade to stay away from his target
		Vector2 futureTargetPosition = (Vector2)targetToEvade.Transform.position + targetToEvade.Rigidbody.velocity * timeToReachTarget;
		
		// Calculate the steering force needed to veer away from the target's future position
		Vector2 evadeForce = FleeForce(futureTargetPosition);

		// Return the steering force used to evade the given target
		return evadeForce;
	}

	/// <summary>
	/// Returns a steering force which makes this entity wander around in random directions.
	/// </summary>
	/// <param name="circleDistance">The distance from the entity to the wander circle. The greater this value
	/// , the stronger the wander force, and the more likely the entity will change directions.</param>
	/// <param name="circleRadius">The greater the radius, the stronger the wander force, and the more likely
	/// the entity will change directions.</param>
	/// <param name="angleChange">The maximum angle in degrees that the wander force can change next frame.</param>
	public Vector2 WanderForce(float circleDistance, float circleRadius, float angleChange)
	{
		// Find the center of the 'wander circle'. This is a vector pointing in the direction of the entity's velocity,
		// with a length of 'circleDistance'
		Vector2 circleCenter = rigidbody.velocity.normalized * circleDistance;

		// Computes a displacement vector, which has a magnitude of 'circleRadius' and an angle of 'wanderAngle'
		// This determines the displacement of the wander force relative to the 'wander circle'
		Vector2 displacement = Vector2.up;
		displacement = displacement.SetMagnitude(circleRadius).SetAngle(wanderAngle);

		// Modifies the wander angle by a value between +/-(angleChange/2). This way, the entity has a chance of 
		// slightly changing directions next frame
		wanderAngle += ((Random.value * (angleChange*0.5f)) - (Random.value * (angleChange*0.5f))) * Mathf.Deg2Rad;

		// The final steering force is the center of the wander circle, plus the computed 'displacement' vector.
		Vector2 wanderForce = circleCenter + displacement;

		return wanderForce;
	}
	
	/** The max speed at which the GameObject can move. The larger the value,
	  * the larger the steering forces? */
	public float MaxSpeed
	{
		get { return maxSpeed; }
		set { maxSpeed = value; }
	}

	/// <summary>
	/// The max amount of steering force that can be applied on the entity every frame
	/// </summary>
	public float MaxForce
	{
		get { return maxForce; }
		set { maxForce = value; }
	}
	
	/** Returns a cached version of the Transform component attached to the GameObject to which 
	  * this SteeringManager belongs. */
	public Transform Transform
	{
		get { return transform; }
	}
	
	/** Returns a cached version of the Rigidbody component attached to the GameObject to which 
	  * this SteeringManager belongs. */
	public Rigidbody2D Rigidbody
	{
		get { return rigidbody; }
	}
	
}
