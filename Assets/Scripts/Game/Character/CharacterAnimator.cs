using System;
using UnityEngine;
using System.Collections;

public class CharacterAnimator : MonoBehaviour
{
	/// <summary>
	/// Governs character logic	
	/// </summary>
	private Character character;

	/// <summary>
	/// The framerate of each Spine animation in frames/second.
	/// </summary>
	public const float FRAME_RATE = 30.0f;

	/// <summary>
	/// We manipulate this Transform instead of that of the skeletal mesh to avoid unwanted behaviour.
	/// </summary>
	protected Transform graphicsObject;
	/// <summary>
	/// The GameObject's skeleton, used to control the entity's Spine animations	
	/// </summary>
	protected SkeletonAnimation skeleton;

	/** The last animation which plays for the character's current move. Note: currently unused. */
	private string finalMoveAnimation;

	/** Helper quaternions used to avoid runtime allocation */
	private readonly Quaternion faceLeftRotation = Quaternion.Euler(0f, 180f, 0f);
	private readonly Quaternion faceRightRotation = Quaternion.Euler(0f, 0f, 0f);

	protected void Awake()
	{
		// Caches the entity's Character component to avoid excessive runtime lookups
		character = (Character)GetComponent<Character>();
		// Retrieves the Graphics GameObject, used to manipulate the character's skeleton
		graphicsObject = transform.FindChild("Graphics");
	}

	protected void Start()
	{
		// Caches the entity's Spine skeleton, used to control animation
		skeleton = graphicsObject.GetComponentInChildren<SkeletonAnimation>();

		// Tells the character's skeleton to call the 'OnComplete' method when it finishes playing an animation
		skeleton.state.End += OnComplete;

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
	/// Plays the animations needed for the character to perform the move.
	/// Each action has a list of animation sequences. The character choses to
	/// play one of these sequences when this action is performed. This method
	/// returns the index of the chosen animation sequence to inform the character
	/// which animations were chosen. This affects, for instance, the time at which
	/// the action's hit boxes are activated, as controlled by the CharacterCollider
	/// component
	/// </summary>
	public int Play(Action action)
	{
		// Choose a random animation sequence with which to perform the action
		int animationSequenceIndex = ArrayUtils.RandomIndex(action.animationSequences);
		AnimationSequence animationSequence = action.animationSequences[animationSequenceIndex];

		// Cache the array of animations to be played sequentially
		string[] animations = animationSequence.animations;

		// Stores the total time it will take to perform the action
		float duration = 0.0f;

		// Cycle through all the animations in the chosen animation sequence
		for(int i = 0; i < animations.Length; i++)
		{
			// Cache the animation being cycled through
			string animation = animations[i];

			// If true, this animation will loop until it is told to stop
			bool loop = false;

			// If this is the last animation in the sequence
			if(i == animations.Length-1)
			{
				// Stores the final animation which plays when this action is performed
				finalMoveAnimation = animation;

				// If the animation sequence is supposed to loop on the last animation
				if(animationSequence.loopLastAnimation)
					// Tell this last animation to loop when it plays
					loop = true;
			}

			// If this is the first animation in the sequence
			if(i == 0)
			{
				// Set the character skeleton to play the first animation in the chosen sequence
				skeleton.state.SetAnimation (0, animation, loop);
			}
			// Else, if this is not the last animation in the sequence
			else
			{
				// Sets the animation to play right after the previous animation at track index 0
				skeleton.state.AddAnimation(0, animation, loop, 0.0f);
			}

			// Increment the total duration of the action by this animation's duration.
			// TODO: Eliminate call to 'FindAnimation()' (expensive)
			duration += skeleton.state.Data.SkeletonData.FindAnimation(animation).Duration;
		}

		// Start a coroutine that will wait for the action to be done being performed. Subsequently, the character's current action is set to null.
		StartCoroutine(WaitForActionComplete(action, duration));
		// Tell the action how long it will take to finish performing it.
		//action.Duration = duration;

		// Only play the 'Idle' animation after this animation sequence if the last animation is not set to loop.
		// Otherwise, the 'Idle' animation would cancel the looping of the last animation. Also, don't play the 
		// idle animation if the character is dead.
		if(!animationSequence.loopLastAnimation && !character.CharacterStats.IsDead ())
			// Set the 'Idle' animation to play right after the move animations are performed
			skeleton.state.AddAnimation (0, character.CharacterControl.ActionSet.basicActions.IdleAnimation, true, 0.0f); 

		// Return the index of the chosen animation sequence so that the other character components can be informed of the choice
		return animationSequenceIndex;
	}

	/// <summary>
	/// At any given time, a character is told to play 'n' animations, one after the other. This method returns 
	/// the amount of time required to play the first 'animationIndex+1' of these animations to completion. 
	/// We add a '1' to 'animationIndex' because this integer is zero-based. Therefore, if 'animationIndex=1',
	/// we want to know how long it will take for the character to finish the first two animations in his current
	/// timeline
	/// </summary>
	public float GetEndTime(int animationIndex)
	{
		// Stores the track entry corresponding to animation for the given animation index
		Spine.TrackEntry trackEntry = skeleton.state.GetCurrent (0);

		// The time required to finish the animationIndex-th animation queued up for the character to play
		float endTime = trackEntry.Animation.Duration;

		// Cycle through the track entries until we get to the animationIndex-th track entry.
		for(int i = 1; i <= animationIndex; i++)
		{
			// Cycle to the next track entry
			trackEntry = trackEntry.next;
			// Increment the end time by the duration of this animation
			endTime += trackEntry.Animation.Duration;
		}

		// Return the time it will take to play the first 'animationIndex'
		return endTime;

	}

	/** Called when the character finishes playing an animation. */
	private void OnComplete(Spine.AnimationState state, int trackIndex)
	{
		// Stores the animation which was just completed. 
		string animation = state.GetCurrent(trackIndex).Animation.Name;
	}

	/// <summary>
	/// Raised when this character finishes performing the given action.
	/// </summary>
	/// <param name="action">Action.</param>
	private void OnActionComplete(Action action)
	{
		// Inform the CharacterControl script that the character has finished a performing his current action
		character.CharacterControl.OnActionComplete();
	}

	private IEnumerator WaitForActionComplete(Action action, float duration)
	{
		// Wait 'duration' seconds before the action is complete
		for(float timer = 0; timer <= duration; timer += Time.deltaTime)
			yield return null;

		//Debug.Log ("Character: " + character.name + " finished performing " + action.name);

		// If the current action the character is performing is still the same as the action that was passed as an argument,
		// the action has been performed until the very end. Therefore, inform the character that he has completed an action
		if(character.CharacterControl.CurrentAction == action)
		{
			//Debug.Log ("ACTION COMPLETED 'TIL THE END");
			// Inform this 'CharacterAnimator' instance that the character has finished performing the given action.
			OnActionComplete (action);
		}
	}

	/// <summary>
	/// The component responsible for rendering and animating the character's Spine skeleton
	/// IMPORTANT: Only populated after CharacterAnimator.Start() function is called
	/// </summary>
	public SkeletonAnimation Skeleton
	{
		get { return skeleton; }
		set { this.skeleton = value; }
	}
}
