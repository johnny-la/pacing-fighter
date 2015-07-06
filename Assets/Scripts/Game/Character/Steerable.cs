using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the steering behaviours for the GameObject this script is attached to
/// </summary>
public class Steerable : MonoBehaviour
{
	/** The maximum speed at which the GameObject can move */
	private float maxSpeed;
	
	/** Stores the steering force to be applied on this object this frame. 
	  * This vector accumulates all steering forces before applying it to
	  * this GameObject's velocity. */
	private Vector2 steeringForce;
	
	/** Cache this GameObject's components for efficiency purposes. */
	private new Transform transform;
	private new Rigidbody2D rigidbody;
	
	void Awake()
	{
		// Cache this GameObject's Transform component
		transform = GetComponent<Transform>();
		rigidbody = GetComponent<Rigidbody2D>();
	}
	
	/// <summary>
	/// Seek the specified target.
	/// </summary>
	public void Seek(Vector2 target)
	{
		// Add a seeking force to this object's final steering force
		steeringForce += SeekForce(target);
	}
	
	/** Returns the steering force required to make this GameObject seek the given target */
	public Vector2 SeekForce(Vector2 target)
	{
		// Compute the distance vector to the target position
		Vector2 distance = (Vector2)target - (Vector2)transform.position;
		// Compute the direction the GameObject must travel to reach his target
		Vector2 direction = distance.normalized;
		
		// Calculate the velocity at which this object should move to reach his target
		Vector2 desiredVelocity = direction * maxSpeed;
		
		// Compute the steering force applied to make this object seek his target
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
	
	/** The max speed at which the GameObject can move. The larger the value,
	  * the larger the steering forces? */
	public float MaxSpeed
	{
		get { return maxSpeed; }
		set { maxSpeed = value; }
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
