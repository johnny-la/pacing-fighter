using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Flashes a Spine skeleton a given color.
/// </summary>
public class SkeletonFlasher : MonoBehaviour 
{
	/// <summary>
	/// The color to flash the Spine skeleton.
	/// </summary>
	private Color flashColor = new Color(1,1,1,0);

	/// <summary>
	/// The amount of time for which the skeleton flashes.
	/// </summary>
	private float flashTime;

	/// <summary>
	/// The skeleton flashed by this script.
	/// </summary>
	private SkeletonAnimation skeleton;

	/// <summary>
	/// The ghosting effect used to create the flash.
	/// </summary>
	private Brawler.SkeletonGhost skeletonGhost;

	/// <summary>
	/// If true, the skeleton ghost flashes above the skeleton and follows its movement and animation. 
	/// Useful for illuminating a character when he is hit.
	/// </summary>
	private bool flashGhost;

	public void Init(SkeletonAnimation skeleton, bool flashGhost)
	{
		// Store the new skeleton in a member variable
		this.skeleton = skeleton;

		// If the skeleton ghost should flash on the skeleton
		if(flashGhost)
		{
			// Create a new SkeletonGhostFlasher to make the skeleton flash with color
			skeletonGhost = skeleton.gameObject.AddComponent<SkeletonGhostFlasher>();

		}
		else
		{
			// Create a new SkeletonGhost to create the color ghost
			skeletonGhost = skeleton.gameObject.AddComponent<Brawler.SkeletonGhost>();
			skeletonGhost.color = flashColor;
			skeletonGhost.spawnRate = 0.2f;//0.05f;
			skeletonGhost.fadeSpeed = 5f;//5.0f;
			skeletonGhost.maximumGhosts = 3;
			skeletonGhost.additive = true;
		}

		// Disable the skeleton ghosting 
		skeletonGhost.ghostingEnabled = false;
	}

	/// <summary>
	/// Flash the spine skeleton a certain color.
	/// </summary>
	public void Flash()
	{
		// Flash the skeleton using a coroutine
		StartCoroutine (ColorFlashCoroutine());
	}

	/// <summary>
	/// Flashes the skeleton using a coroutine.
	/// </summary>
	private IEnumerator ColorFlashCoroutine()
	{
		//SetColor (flashColor);
		skeletonGhost.ghostingEnabled = true;

		yield return new WaitForSeconds(flashTime);

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
	public Color Color
	{
		get { return flashColor; }
		set 
		{ 
			// If the skeleton ghost is additive, the alpha of the color must be set to zero
			if(skeletonGhost.additive)
				value.a = 0;

			// Update the color of the flashing
			flashColor = value;

			//Change the ghost's color
			skeletonGhost.color = flashColor;
		}
	}

	/// <summary>
	/// The amount of time the skeleton flashes before returning to its original color.
	/// </summary>
	public float FlashTime
	{
		get { return flashTime; }
		set { flashTime = value; }
	}

	/// <summary>
	/// If true, the color flash is rendered in front of the character. Otherwise, it is rendered in back.
	/// </summary>
	public bool RenderInFront
	{
		get { return skeletonGhost.renderInFront; }
		set { skeletonGhost.renderInFront = value; }
	}

}
