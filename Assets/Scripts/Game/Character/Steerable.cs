using UnityEngine;
using System.Collections;

// TODO: (Optimization) There are a lot of local Vector2s created in method calls and garbage collected
// once the method returns. Get rid of these local instantiations/deallocations to reduce GC overhead.

/// <summary>
/// Manages the steering behaviours for the GameObject this script is attached to.
/// (All steering behaviors implemented based on the tutorials on GameDevelopment Tuts+)
/// </summary>
public class Steerable : MonoBehaviour
{
	/** The maximum speed at which the GameObject can move */
	public float maxSpeed;

	/** The maximum amount of steering force that can be applied on this entity every frame. */
	public float maxForce;

	/** The steerable's radius in meters. When trying to avoid obstacles, the steerable will take into account the fact that
	 *  it has a radius and leave more space between him and the obstacle. */
	public float bodyRadius;

	/** The collider which detects the nearest obstacle in the steerable's line of sight. Used in the obstacle avoidance behavior. */
	public ObstacleDetector obstacleDetector;

	/** The neighbourhood used for group behaviours. Determines which entities are neighbours with this steerable. */
	public Neighbourhood neighbourhood;

	/** If true, the steerable's speed is clamped to 'MaxSpeed'. The entity does not accelerate nor decelerate, but stay at a constant speed. */
	public bool constantSpeed;
	
	/** Stores the steering force to be applied on this object this frame. 
	  * This vector accumulates all steering forces before applying it to
	  * this GameObject's velocity. */
	private Vector2 steeringForce;

	/** The angle used in the 'Wander' steering behavior. Denotes an angle relative to the entity's 'wander circle'.
	  * Updated every frame the wander behavior is applied */
	private float wanderAngle;

	/** The steerable's velocity the last time Steerable.ApplyForces() was called. This allows the steering behavors
	 *  to use the velocity which the steerable had before any physics collisions occurred. For instance, if the steerable
	 *  collides with an obstacle he was trying to avoid, his real velocity will point in the opposite direction as the 
	 *  obstacle because of the bounce back, and his line of sight will look away from the obstacle. Thus, if the obstacle 
	 *  avoidance force is applied again, the obstacle will not be seen since the steerable's velocity points away from 
	 *  the obstacle. Thus, this velocity allows us to ignore physics collisions and assume that the steerable is still
	 *  looking at the obstacle. */
	private Vector2 previousVelocity;
	
	/** Caches the steerable's normalized velocity. Only computed once a frame when a steering force is added. */
	private Vector2 normalizedVelocity;

	/** The last time a steering force was added to this Steerable instance. */
	private float lastUpdateTime;
	
	/** Cache this GameObject's components for efficiency purposes. */
	private new Transform transform;
	private new Rigidbody2D rigidbody;

	/** A force vector drawn as a gizmo on-screen. */
	private Vector2 debugForce;

	//private Vector2 dampVelocity = Vector2.zero;
	//public float timeToReachDesiredVelocity = 0.5f;
	
	void Awake()
	{
		// Cache this GameObject's Transform component
		transform = GetComponent<Transform>();
		rigidbody = GetComponent<Rigidbody2D>();
	}

	// NOTE: This method is called after the physics update. Therefore, the ApplyForces() has already been called by the time this is called.
	/*public void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawLine((Vector2)Transform.position, (Vector2)transform.position + rigidbody.velocity);
		Gizmos.color = Color.red;
		Gizmos.DrawLine((Vector2)Transform.position, (Vector2)transform.position + steeringForce);
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine((Vector2)Transform.position, (Vector2)transform.position + debugForce);
	}*/

	/// <summary>
	/// Applies all the steering forces set by the AddSteeringForce() function. Must be called every
	/// frame for the steering behaviors to update correctly. 
	/// </summary>
	/// <param name="deltaTime">The amount of time that has passed since the last call to this method. </param>
	public void ApplyForces(float deltaTime)
	{
		// TODO: Optimize. Truncate returns a new Vector2
		// Clamp the steering force's magnitude to 'maxForce'. This way, the steering force can never act too dramatically.
		steeringForce = steeringForce.Truncate(maxForce); 
		// Divide the steering force by this steerable's mass. Effectively converts the force into an acceleration using 'f = ma'
		Vector2 steeringAcceleration = steeringForce / rigidbody.mass;

		// Add the steering force to the rigidbody's current velocity, and truncate the steerable's speed so that it doesn't exceed his max speed
		/*rigidbody.velocity = Vector2.SmoothDamp((Vector2)previousVelocity, 
		                                        Vector2.ClampMagnitude(previousVelocity + steeringForce, maxSpeed),
		                                        ref dampVelocity, timeToReachDesiredVelocity).Truncate(maxSpeed);*/
		Vector2 newVelocity = (previousVelocity + (steeringAcceleration * deltaTime)).Truncate (MaxSpeed);

		// Update this entity's previous velocity. This allows us to keep track of the steerable's velocity before future physics collisions.
		previousVelocity = newVelocity;

		// If the steerable must always travels at a constant speed
		if(constantSpeed)
		{
			// Clamp the steerable's speed to his maxSpeed. Like this, the entity always moves at the same speed
			newVelocity = newVelocity.SetMagnitude (MaxSpeed);
		}

		// Update the steerable's velocity in the physics engine.
		rigidbody.velocity = newVelocity;

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
		// Reset the steerable's previous velocity to zero. This way, if he starts moving again, he won't restart at his old velocity.
		previousVelocity = Vector2.zero;
	}

	/// <summary>
	/// Adds the 'Seek' steering force to this instance. This behavior makes this steerable smoothly move to the given position.
	/// </summary>
	/// <param name="multiplier">The amount by which the seek force is multiplied before being added to the steerable's steering force
	public void AddSeekForce(Vector2 target, float multiplier)
	{
		steeringForce += SeekForce (target) * multiplier;
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
		Vector2 steeringForce = desiredVelocity - previousVelocity;
		
		return steeringForce;
		
	}

	/// <summary>
	/// Adds the 'Arrival' steering force to this instance. This behavior makes this steerable smoothly move to the given position and stop when
	/// he gets within 'slowingRadius' meters from his target.
	/// </summary>
	/// <param name="multiplier">The amount by which the seek force is multiplied before being added to the steerable's steering force
	public void AddArrivalForce(Vector2 target, float slowingRadius, float multiplier)
	{
		steeringForce += ArrivalForce (target, slowingRadius) * multiplier;
	}

	/// <summary>
	/// Returns the steering force required to make this GameObject arrive at the given target. This steering behavior is 
	/// identical to the 'Seek' behavior, except that the steerable slows down and stops when he reaches his target.
	/// </summary>
	/// <param name="slowingRadius">When the entity gets this close to his target, he will start slowing down.</param>
	public Vector2 ArrivalForce(Vector2 target, float slowingRadius)
	{
		// Compute the distance vector to the target position
		Vector2 distance = (Vector2)target - (Vector2)transform.position;
		// Store the distance from this entity to his target position
		float distanceToTarget = distance.magnitude;
		
		// Calculate the velocity at which this object should move to reach his target fasteste
		Vector2 desiredVelocity = distance.normalized * maxSpeed;
		
		// If this entity is less than 'slowingRadius' units away from his target, slow down this steerable so that he doesn't overshoot his target
		if(distanceToTarget < slowingRadius)
		{
			// Multiply the desired velocity by 'distanceToTarget/slowingRadius'. This way, the closer this steerable is to his
			// target, the slower the steering force. This way, the steerable will slow down as he gets closer to his target.
			desiredVelocity *= (distanceToTarget / slowingRadius);
		}
		
		// Compute the steering force applied to make this object arrive at his target
		Vector2 steeringForce = desiredVelocity - previousVelocity;
		
		return steeringForce;
	}

	/// <summary>
	/// Pursue the given target. Moves the steerable to his target's future position.
	/// </summary>
	/// <param name="multiplier">The amount by which the seek force is multiplied before being added to the steerable's steering force
	public void AddPursueForce(Steerable target, float multiplier)
	{
		steeringForce += PursueForce (target) * multiplier;
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

	/// <summary>
	/// Add a force which flees the given target. Only applied once 'Steerable.ApplySteeringForces()' is called.
	/// </summary>
	/// <param name="multiplier">The amount by which the seek force is multiplied before being added to the steerable's steering force
	public void AddFleeForce(Vector2 fleeTarget, float multiplier)
	{
		steeringForce += FleeForce (fleeTarget) * multiplier;
	}
	
	/** Returns a steering force which makes this objec flee the given position */
	public Vector2 FleeForce(Vector2 fleeTarget)
	{
		// The fleeing force is simply the negative of the seek force to the given fleeTarget.
		return -SeekForce(fleeTarget);
	}

	/// <summary>
	/// Adds a force which evades the given target. Instead of simply moving away from his target, this seekable flees
	/// his target's future position, making him smarter at dodging his target.
	/// </summary>
	/// <param name="multiplier">The amount by which the seek force is multiplied before being added to the steerable's steering force
	public void AddEvadeForce(Steerable targetToEvade, float multiplier)
	{
		steeringForce += EvadeForce (targetToEvade) * multiplier;
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
	/// Adds a wander force to this steerable. Make him wander around in a a random direction/pattern.
	/// </summary>
	/// <param name="circleDistance">The distance from the entity to the "wander" circle. The greater this value,
	/// the stronger the wander force, and the more likely the entity will change directions.</param>
	/// <param name="circleRadius">The greater this radius, the stronger the wander force, and the more likely
	/// the entity will change directions.</param>
	/// <param name="angleChange">The maximum angle in degrees that the wander force can change next frame.</param>
	/// <param name="multiplier">The amount by which the seek force is multiplied before being added to the steerable's steering force
	public void AddWanderForce(float circleDistance, float circleRadius, float angleChange, float multiplier)
	{
		steeringForce += WanderForce (circleDistance, circleRadius, angleChange) * multiplier;
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
		// Update the steerable's 'normalizedVelocity' variable
		CheckDirty ();

		// Find the center of the 'wander circle'. This is a vector pointing in the direction of the entity's velocity,
		// with a length of 'circleDistance'
		Vector2 circleCenter = normalizedVelocity * circleDistance;

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

	/// <summary>
	/// Adds a force which avoids the nearest obstacle in the steerable's line of sight.
	/// </summary>
	/// <param name="avoidanceForce">The amount of force applied in order to avoid the nearest obstacle.</param>
	/// <param name="maxViewDistance">Only obstacles within 'maxViewDistance' meters of this steerable can be avoided.</param>
	/// <param name="obstacleLayer">The layer which contains the colliders that can be avoided.</param>
	/// <param name="multiplier">The amount by which the seek force is multiplied before being added to the steerable's steering force
	public void AddObstacleAvoidanceForce(float avoidanceForce, float maxViewDistance, LayerMask obstacleLayer, float multiplier)
	{
		steeringForce += ObstacleAvoidanceForce (avoidanceForce, maxViewDistance, obstacleLayer) * multiplier;
	}

	/// <summary>
	/// Returns a force which allows this steerable to avoid the obstacles on the given layer. Only the closest obstacle
	/// in the steerable's line of sight is avoided.
	/// </summary>
	/// <param name="avoidanceForce">The amount of force applied in order to avoid the nearest obstacle.</param>
	/// <param name="maxViewDistance">Only obstacles within 'maxViewDistance' meters of this steerable can be avoided.</param>
	/// <param name="obstacleLayer">The layer which contains the colliders that can be avoided.</param>
	public Vector2 ObstacleAvoidanceForce(float avoidanceForce, float maxViewDistance, LayerMask obstacleLayer)
	{
		// Update the steerable's 'normalizedVelocity' variable
		CheckDirty ();

		// The obstacle avoidance force returned by this method.
		Vector2 avoidForce = Vector2.zero;

		// Computes the vector which lies 'maxViewDistance' meters away from the steerable, looking in the direction of his velocity
		Vector2 ahead = (Vector2)transform.position + (previousVelocity.normalized * maxViewDistance);

		// Shoot a circle-cast from the steerable's position in the direction of his velocity. The ray has a distance of 'maxViewDistance',
		// so that obstacles can be detected as far as the viewing distance permits. Returns a hit if an obstacle is in his line of sight
		//RaycastHit2D obstacleHit = Physics2D.CircleCast (transform.position, bodyRadius, previousVelocity, maxViewDistance, obstacleLayer);

		// Cache the Transform for the first obstacle in the steerable's path
		Transform nearestObstacle = obstacleDetector.GetNearestObstacle();

		// If an obstacle is in the steerable's way
		if(nearestObstacle != null)
		{
			// Calculate the force needed to avoid the obstacle. This is the vector from the center of the obstacle to the 'ahead' vector
			avoidForce = ahead - (Vector2)nearestObstacle.position;
			// Clamp the force's magnitude to the given value
			avoidForce = avoidForce.SetMagnitude (avoidanceForce);
		}	

		debugForce = (ahead - (Vector2)transform.position);

		//Debug.Log ("Avoid the obstacle: " + obstacleHit.transform + " With force: " + avoidForce);

		// Returns a force which will avoid the nearest obstacle in the steerable's line of sight.
		return avoidForce;
	}

	/// <summary>
	/// Adds a force which separates this agent from his neighbours. This steerable's neighbours are defined by the GameObjects
	/// which lie in the 'Neighbourhood' variable assigned to this steerable.
	/// </summary>
	/// <param name="multiplier">The amount by which the seek force is multiplied before being added to the steerable's steering force
	public void AddSeparationForce(float multiplier)
	{
		steeringForce += SeparationForce() * multiplier;
	}

	/// <summary>
	/// Returns the 'Separation' steering force. This force pushes this steerable away from his neighbours, as
	/// defined by his 'Neighbourhood' component.
	/// </summary>
	public Vector2 SeparationForce()
	{
		// Store the final steering force
		Vector2 separationForce = Vector2.zero;

		// Cycle through each neighbour of this steerable
		for(int i = 0; i < neighbourhood.NeighbourCount; i++)
		{
			// Cache the neighbour's Transform
			Transform neighbour = neighbourhood.GetNeighbour(i).transform;

			// Compute the vector from the neighbour to this steerable's position.
			Vector2 toSteerable = Transform.position - neighbour.position;

			// The further the neighbour is from this steerable, the weaker the separation force. Using the normalized 'toSteerable' vector
			// ensures that the steerable is pushed away from his neighbour.
			separationForce += toSteerable.normalized / toSteerable.magnitude;
		}

		// Return the final separation force, as computed above.
		return separationForce;
	}

	/// <summary>
	/// Adds a force which aligns this steerable to his neighbours. This makes him move in the same direction as his neighbours.
	/// Imagine cars on a highway. They are all moving in the same direction, and are thus exhibiting alignment behavior.
	/// </summary>
	/// <param name="multiplier">The amount by which the seek force is multiplied before being added to the steerable's steering force
	public void AddAlignmentForce(float multiplier)
	{
		steeringForce += AlignmentForce () * multiplier;
	}

	/// <summary>
	/// Returns a force corresponding to the 'Alignment' group behavior. This behavior makes the steerable move in parallel with his
	/// neighbours. Imagine cars on a highway. They are all moving in the same direction, thus exhibiting alignment behavior.
	/// </summary>
	public Vector2 AlignmentForce()
	{
		// Update the steerable's 'normalizedVelocity' variable
		CheckDirty ();

		// The final alignment force
		Vector2 alignmentForce = Vector2.zero;
		// Stores the average velocity of the neighbours.
		Vector2 averageNeighbourVelocity = Vector2.zero;

		// Cycle through each neighbour for this steerable. This agent will try to align himself with his neighbours
		for(int i = 0; i < neighbourhood.NeighbourCount; i++)
		{
			// Cache the neighbour's Rigidbody
			Rigidbody2D neighbour = neighbourhood.GetNeighbour(i).GetComponent<Rigidbody2D>();

			// Add the neighbour's velocity to the average. 
			averageNeighbourVelocity += neighbour.velocity;
		}

		// If this steerable has one or more neighbours
		if(neighbourhood.NeighbourCount > 0)
		{
			// Take the average of all the neighbours' forward direction
			averageNeighbourVelocity /= (float)neighbourhood.NeighbourCount;

			// The final alignment force is neighbours' average velocity minus the steerable's current velocity.
			alignmentForce = averageNeighbourVelocity - previousVelocity;

			debugForce = alignmentForce;
		}

		// Return the alignment force computed above
		return alignmentForce;
	}

	/// <summary>
	/// Add a cohesion force. This steers the agent to the center of mass of his neighbours.
	/// Imagine a sheep trying to run back to the center of his group. He is demonstrating cohesion.  	
	/// </summary>
	/// <param name="multiplier">The amount by which the seek force is multiplied before being added to the steerable's steering force
	public void AddCohesionForce(float multiplier)
	{
		steeringForce += CohesionForce() * multiplier; 
	}

	/// <summary>
	/// Returns a steering force for the 'Cohesion' group behavior. This steers the agent to the center of mass of his neighbours.
	/// Imagine a sheep trying to run back to the center of his group. He is demonstrating cohesion. 
	/// </summary>
	public Vector2 CohesionForce()
	{
		// The final cohesion force
		Vector2 cohesionForce = Vector2.zero;

		// Stores the neighbours' positional center of mass. This is where this agent will try to move
		Vector2 neighbourCenterOfMass = Vector2.zero;

		// Cycle through each neighbour of this steerable
		for(int i = 0; i < neighbourhood.NeighbourCount; i++)
		{
			// Cache the neighbour's Transform
			Transform neighbour = neighbourhood.GetNeighbour (i).transform;

			// Add the neighbour's position to the neighbourhood's center of mass
			neighbourCenterOfMass += (Vector2)neighbour.position;
		}

		// If the neighbourhood has as least one neighbour, apply the cohesion force.
		if(neighbourhood.NeighbourCount > 0)
		{
			// Take an average of all the neighbours' positions. This gives us their center of mass
			neighbourCenterOfMass /= neighbourhood.NeighbourCount;

			// Seek the neighbourhood's center of mass. This seeking force is the applied cohesion force 
			cohesionForce = SeekForce (neighbourCenterOfMass);
		}

		// Return the cohesion force computed above.
		return cohesionForce;

	}

	/// <summary>
	/// Called every time a steering force is calculated. If this is the first steering force added this frame, the steerable's
	/// values are updated, such as the steerable's normalized velocity.
	/// </summary>
	private void CheckDirty()
	{
		// If this is the first time the CheckDirty() method is called this frame, update the Steerable's values
		if((Time.time - lastUpdateTime) != 0)
		{
			// Cache the steerable's normalized velocity at most once a frame.
			normalizedVelocity = rigidbody.velocity.normalized;
			
			// Update the last time at which the steerable's values were updated
			lastUpdateTime = Time.time;
		}
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
