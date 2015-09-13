using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Applies effects onto a Spine skeleton, such as color flashing and ghosting.
/// </summary>
public class SkeletonEffects : MonoBehaviour 
{
	/// <summary>
	/// The color to flash the Spine skeleton.
	/// </summary>
	private Color flashColor = new Color(1,1,1,0);
	/// <summary>
	/// The color of the ghosting effect.
	/// </summary>
	private Color ghostColor = new Color(1,1,1,0);

	/// <summary>
	/// The object used to create a colour flash on the skeleton.
	/// </summary>
	private SkeletonGhostFlasher skeletonFlasher;
	/// <summary>
	/// The ghosting effect used to create the flash.
	/// </summary>
	private Brawler.SkeletonGhost skeletonGhost;

	/// <summary>
	/// The skeleton flashed by this script.
	/// </summary>
	private SkeletonAnimation skeleton;

	public void Init(SkeletonAnimation skeleton)
	{
		// Store the new skeleton in a member variable
		this.skeleton = skeleton;

		// Create a new SkeletonGhostFlasher to make the skeleton flash with color
		skeletonFlasher = skeleton.gameObject.AddComponent<SkeletonGhostFlasher>();
		skeletonFlasher.color = flashColor;
		skeletonFlasher.additive = true;

		// Create a new SkeletonGhost which generates the ghosting
		skeletonGhost = skeleton.gameObject.AddComponent<Brawler.SkeletonGhost>();
		skeletonGhost.color = ghostColor;
		skeletonGhost.spawnRate = 0.2f;//0.05f;
		skeletonGhost.fadeSpeed = 5f;//5.0f;
		skeletonGhost.maximumGhosts = 3;
		skeletonGhost.additive = true;

		// Disable the skeleton ghosting and flashing 
		skeletonFlasher.ghostingEnabled = skeletonGhost.ghostingEnabled = false;
	}

	/// <summary>
	/// Flash the spine skeleton a certain color for the given amount of time
	/// </summary>
	public void ColorFlash(float duration)
	{
		// Flash the skeleton using a coroutine
		StartCoroutine (ColorFlashCoroutine(duration));
	}

	/// <summary>
	/// Flashes the skeleton using a coroutine.
	/// </summary>
	private IEnumerator ColorFlashCoroutine(float duration)
	{
		// Activate the flashing
		skeletonFlasher.EnableFlash ();

		yield return new WaitForSeconds(duration);

		// Disable the flashing
		skeletonFlasher.DisableFlash();
	}

	/// <summary>
	/// Activates the ghosting effect on the skeleton for the specified amount of time
	/// </summary>
	public void EnableGhosting(float duration)
	{
		// Delegate the call to a coroutine
		StartCoroutine(EnableGhostingCoroutine(duration));
	}

	/// <summary>
	/// Displays the ghosting effect for 'duration' seconds.
	/// </summary>
	private IEnumerator EnableGhostingCoroutine(float duration)
	{
		// Enable the ghosting
		skeletonGhost.ghostingEnabled = true;

		// Leave the ghosting activated for 'duration' seconds.
		yield return new WaitForSeconds(duration);

		// Disable the ghosting effect after the timer has elapsed.
		skeletonGhost.ghostingEnabled = false;
	}

	/// <summary>
	/// The skeleton which is flashed a certain colour by this script.
	/// </summary>
	public SkeletonAnimation Skeleton
	{
		get { return skeleton; }
		set 
		{ 
			skeleton = value; 

			// Initialize the color flasher for the given skeleton
			Init(value);
		}
	}

	/// <summary>
	/// The color that the skeleton flashes.
	/// </summary>
	public Color FlashColor
	{
		get { return flashColor; }
		set 
		{ 
			// If the skeleton flash is additive, the alpha of the color must be set to zero
			if(skeletonFlasher.additive)
				value.a = 0;

			// Update the color of the flashing
			flashColor = value;

			//Change the flash's color
			skeletonGhost.color = flashColor;
		}
	}
	/// <summary>
	/// The color that the ghost flashes.
	/// </summary>
	public Color GhostColor
	{
		get { return ghostColor; }
		set 
		{ 
			// If the skeleton ghost is additive, the alpha of the color must be set to zero
			if(skeletonGhost.additive)
				value.a = 0;
			
			// Update the color of the ghosting
			ghostColor = value;
			
			//Change the ghost's color
			skeletonGhost.color = ghostColor;
		}
	}

	/// <summary>
	/// If true, the ghosts are rendered in front of the character. Otherwise, they are rendered in back.
	/// </summary>
	public bool RenderGhostInFront
	{
		get { return skeletonGhost.renderInFront; }
		set { skeletonGhost.renderInFront = value; }
	}
	/// <summary>
	/// If true, the flash is rendered in front of the character. Otherwise, it is rendered in back.
	/// </summary>
	public bool RenderFlashInFront
	{
		get { return skeletonFlasher.renderInFront; }
		set { skeletonFlasher.renderInFront = value; }
	}

}
