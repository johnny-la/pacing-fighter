using UnityEngine;
using System.Collections;

/// <summary>
/// Flashes a ghost in front of a skeleton as long as this component is active. The ghost follows 
/// the skeleton's movement and animation.
/// </summary>

public class SkeletonGhostFlasher : Brawler.SkeletonGhost
{
	/// <summary>
	/// Make the ghost flash above the skeleton every 'FlashUpdateRate' seconds. 
	/// </summary>
	private const float FlashUpdateRate = 1 / 60.0f;

	void Start()
	{
		base.renderInFront = true; // Render the flash in front of the skeleton
		base.fadeSpeed = 0.1f;	// Fade very slowly
		base.maximumGhosts = 1;	// Only allow one ghost to follow the skeleton at a time
		base.spawnRate = FlashUpdateRate; // Allow the flash to update quickly to follow the skeleton's position and animation

		// Initialize the ghosting
		base.Start ();
	}

	void Update()
	{

	}
}
