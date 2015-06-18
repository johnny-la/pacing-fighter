using System;
using UnityEngine;

public abstract class CharacterAnimator : MonoBehaviour
{
	/// <summary>
	/// Governs character logic	
	/// </summary>
	private Character character;

	/// <summary>
	/// We manipulate this Transform instead of that of the skeletal mesh to avoid unwanted behaviour.
	/// </summary>
	protected Transform graphicsObject;
	/// <summary>
	/// The GameObject's skeleton, used to control the entity's Spine animations	
	/// </summary>
	protected SkeletonAnimation skeleton;

	/** Helper quaternions used to avoid allocation */
	private readonly Quaternion faceLeftRotation = Quaternion.Euler(0f, 180f, 0f);
	private readonly Quaternion faceRightRotation = Quaternion.Euler(0f, 0f, 0f);

	protected void Start()
	{
		// Caches the entity's Character component to avoid excessive runtime lookups
		character = (Character)GetComponent<Character>();
		// Retrieves the Graphics GameObject, used to manipulate the character skeleton
		graphicsObject = transform.FindChild("Graphics");
		// Caches the entity's Spine skeleton, used to control animation
		skeleton = graphicsObject.GetComponentInChildren<SkeletonAnimation>();

		// Begin in setup pose to ensure the animations play correctly
		skeleton.Skeleton.SetToSetupPose();
	}

	protected void Update()
	{
		// If the player is facing left
		if (character.CharacterMovement.FacingDirection == Direction.Left)
			// Flip the character to look left
			graphicsObject.localRotation = faceLeftRotation;
		// Else, if the player is facing right
		else
			// Flip the character to face right
			graphicsObject.localRotation = faceRightRotation;

		//skeleton.skeletonDataAsset.GetSkeletonData(true).FindAnimation ("Hit");
	}

	/// <summary>
	/// Plays the animations needed for the character to perform the move
	/// </summary>
	public void Play(MoveInfo move)
	{
		// Choose a random animation sequence with which to perform the move
		AnimationSequence animationSequence = ArrayUtils.RandomElement(move.animationSequences);

		// Cache the array of animations to be played sequentially
		string[] animations = animationSequence.animations;

		// Set the character skeleton to play the first animation in the chosen sequence
		skeleton.state.SetAnimation (0, animations[0], false);

		// Cycle through all the animations in the chosen animation sequence
		for(int i = 1; i < animations.Length; i++)
		{
			// Cache the animation being cycled through
			string animation = animations[i];

			// Sets the animation to play right after the previous animation at track index 0
			skeleton.state.AddAnimation(0, animation, false, 0.0f);
		}

		// Set the 'Idle' animation to play right after the move animations are performed
		skeleton.state.AddAnimation (0, "Idle", true, 0.0f); 
	}

}
