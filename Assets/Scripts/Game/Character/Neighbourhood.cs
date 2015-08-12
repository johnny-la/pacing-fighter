using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A Neighbourhood is a circle trigger collider which holds an array containing all GameObjects presently
/// inside the trigger volume. Allows entities to keep track of the  
/// </summary>
public class Neighbourhood : MonoBehaviour
{
    /** Stores all GameObjects inside this trigger collider */
    private List<GameObject> neighbours = new List<GameObject>();

	/** The radius of the neighbourhood. Only collider's within 'radius' meters of the Neighbourhood are added to the list of neighbours */
	public float radius;

    /** Colliders on this layer will be added to the neighbourhood. */
    public LayerMask layerToDetect;

	/** The trigger volume that detects if a GameObject is inside this neighbourhood or not. */
	private CircleCollider2D circleCollider;

	void Awake()
	{
		// Create a circle collider to detect which colliders enter this neighbourhood
		circleCollider = gameObject.AddComponent<CircleCollider2D>();
		// Set the collider's radius to the neighbourhood's radius
		circleCollider.radius = radius;

		// Make the collider a trigger so that it doesn't collide with any GameObjects.
		circleCollider.isTrigger = true;
	}

	/*void Update()
	{
		string log = "Neighbours: ";

		for(int i = 0; i < neighbours.Count; i++)
			log += neighbours[i].name + ", ";

		Debug.Log (log);
	}*/

    void OnTriggerEnter2D(Collider2D collider)
    {
        // If the collider which entered the trigger volume is on the correct layer
        if(((1 << collider.gameObject.layer) & layerToDetect) == layerToDetect)
        {
			//Debug.LogWarning("Collider entered in " + transform.parent.name + "'s neighbourhood: " + collider.transform.name);

            // Add the GameObject to the neighbourhood, since it is inside this script's trigger collider 
            neighbours.Add(collider.gameObject);    
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        // If the collider which left the trigger volume belongs to the layer which is tracked by this neighbourhood
		if(((1 << collider.gameObject.layer) & layerToDetect) == layerToDetect)
        {
            // Remove the GameObject from this neighbourhood since it has just left the neighbourhood's collider
            neighbours.Remove(collider.gameObject);
        } 
    }

	/// <summary>
	/// Returns the neighbour at the given index of the neighbourhood's internal array of neighbours.
	/// </summary>
	public GameObject GetNeighbour(int index)
	{
		return neighbours[index];
	}

	/// <summary>
	/// Update the neighbourhood's radius. Only colliders within 'radius' meters from the Neighbourhood's 
	/// center is elligible to be added to the neighbourhood's list of neighbours.
	/// </summary>
	public void SetRadius(float radius)
	{
		this.radius = radius;

		// Update the trigger circle's radius to the given value. This changes the neighbourhood's bounding volume in the physics engine.
		circleCollider.radius = radius;
	}

    /// <summary>
    /// The number of GameObjects which are inside this Neighbourhood's trigger collider
    /// </summary>
    public int NeighbourCount 
    {
        get { return neighbours.Count; }
    }
    
    /// <summary>
    /// Only GameObjects on this layer will be added and kept track of in this neighbourhood
    /// </summary>
    public LayerMask LayerToDetect
    {
        get { return layerToDetect; }
        set { layerToDetect = value; }
    }
}