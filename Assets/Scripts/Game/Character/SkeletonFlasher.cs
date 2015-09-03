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
	/// The original colors for each region attachment on the skeleton.
	/// </summary>
	private Color[] originalColors;

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

	public void Init(SkeletonAnimation skeleton)
	{
		// Store the new skeleton in a member variable
		this.skeleton = skeleton;

		// Create a new SkeletonGhost to create the color flash
		skeletonGhost = skeleton.gameObject.AddComponent<Brawler.SkeletonGhost>();
		skeletonGhost.color = flashColor;
		skeletonGhost.spawnRate = 0.2f;//0.05f;
		skeletonGhost.fadeSpeed = 5f;//5.0f;
		skeletonGhost.maximumGhosts = 3;
		skeletonGhost.additive = true;
		skeletonGhost.ghostingEnabled = false;

		// Creates the array which stores the skeleton's original colors.
		CreateOriginalColorsArray();
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
		//RevertColors ();
	}

	/// <summary>
	/// Sets the color of each region attachment on the Spine skeleton.
	/// </summary>
	private void SetColor(Color color)
	{
		// Stores the list of slots on the spine skeleton
		List<Spine.Slot> slots = skeleton.Skeleton.Slots;
		
		// Cycle through each slot
		for(int i = 0; i < slots.Count; i++)
		{
			Spine.Slot slot = slots[i];
			
			// Determine the type of attachment on the current slot.
			if(slot.Attachment is Spine.RegionAttachment)
			{
				Spine.RegionAttachment attachment = (Spine.RegionAttachment)slot.Attachment;

				// Set the original colors for the attachment before its color is changed
				originalColors[i].r = attachment.R;
				originalColors[i].g = attachment.G;
				originalColors[i].b = attachment.B;
				originalColors[i].a = attachment.A;
				
				// Set the attachment's color to the desired value
				attachment.SetColor(color);
			}
		}
	}

	/// <summary>
	/// Sets the color of the skeleton to its original colours before ColorFlash() was called.
	/// </summary>
	private void RevertColors()
	{
		// Stores the list of slots on the spine skeleton
		List<Spine.Slot> slots = skeleton.Skeleton.Slots;
		
		// Cycle through each slot
		for(int i = 0; i < slots.Count; i++)
		{
			Spine.Slot slot = slots[i];
			
			// Determine the type of attachment on the current slot.
			if(slot.Attachment is Spine.RegionAttachment)
			{
				Spine.RegionAttachment attachment = (Spine.RegionAttachment)slot.Attachment;
				
				// Set the attachment back to its original color
				attachment.SetColor(originalColors[i]);
			}
		}
	}

	/// <summary>
	/// Creates the array which stores the skeleton's original colors.
	/// </summary>
	private void CreateOriginalColorsArray()
	{
		// Create an array which stores the color of each slot.
		originalColors = new Color[skeleton.Skeleton.Slots.Count];

		// Populate the array with Color instances
		for(int i = 0; i < originalColors.Length; i++)
			originalColors[i] = new Color();
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
