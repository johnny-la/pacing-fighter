using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the hit boxes and colliders associated with the character 
/// </summary>
public class CharacterCollider : MonoBehaviour 
{
	/** Caches the Character component that this script is controlling. */
	private Character character;
	
	/** The action set containing the actions the character can perform */
	private ActionSet actionSet;

	/** The skeleton to which hit boxes are attached */
	private SkeletonAnimation skeleton;

	private const int EnableHitBoxEvent = 1,
					  DisableHitBoxEvent = 2;

	void Start()
	{
		// Cache the Character component to access the character's global functionality 
		character = GetComponent<Character>(); 

		// Cache the character's components for easy access
		actionSet = GetComponent<ActionSet>();

		// Cache the character's skeleton. Hit boxes are set up to follow this skeleton's bones
		skeleton = GetComponentInChildren<SkeletonAnimation>();

		// Cycle through every basic action that the character can perform
		for(int i = 0; i < actionSet.basicActions.actions.Length; i++)
		{
			// Create the HitBoxes for each basic action.
			CreateHitBoxes(actionSet.basicActions.actions[i]);
		}
		// Cycle through every combat action in the action set
		for(int i = 0; i < actionSet.combatActions.Length; i++)
		{
			// Create the HitBoxes for the combat actions.
			CreateHitBoxes(actionSet.combatActions[i]);
		}

		// Call the OnAnimationEvent function whenever a Spine event fires in a character animation
		skeleton.state.Event += OnAnimationEvent;
	}

	/** Triggered when the character's animation fires a Spine event */
	private void OnAnimationEvent(Spine.AnimationState animationState, int trackIndex, Spine.Event e)
	{
		// Determine which event was fired
		switch(e.Int)
		{
		case EnableHitBoxEvent: 
			// Enable the hit boxes for the action being currently performed
			EnableHitBoxes(character.CharacterControl.CurrentAction);
			break;
		case DisableHitBoxEvent: 
			// Disable the hit boxes for the character's current action
			DisableHitBoxes (character.CharacterControl.CurrentAction);
			break;
		}
	}

	/// <summary>
	/// Play the specified action by activating its hit boxes at the correct time. Only the hit boxes marked as
	/// 'ForceHit' will be activated by this method. The rest are activated by events in the timeline for the
	/// action's animation
	/// </summary>
	/// 
	/// The object targetted by this action. If this is an attacking action, the given target will receive damage
	/// from this action
	/// 
	/// The index of the animation sequence chosen for the specified action. If this index is '1', it means that
	/// the character chose to play the action's second animation sequence. This changes the frame at which the
	/// hit boxes must activate. In fact, for each animation sequence, the hit boxes are told to activate at a
	/// different time
	public void Play(Action action, GameObject targetObject, int animationSequenceIndex)
	{
		// Cycle through each HitBox belonging to the given action
		for(int i = 0; i < action.hitBoxes.Length; i++)
		{
			// Cache the HitBox being cycled through
			HitBox hitBox = action.hitBoxes[i];

			// If the hit box is set to 'ForceHit', the hit box will hit its target at a specified frame.
			// Thus, we need to generate the hit artificially
			if(hitBox.hitBoxType == HitBoxType.ForceHit)
			{
				// Computes the time at which the hit box must generate a collision. The start frame is
				float startTime = hitBox.hitFrames[animationSequenceIndex] / CharacterAnimator.FRAME_RATE;
				// Start a coroutine that will tell the target object in 'startTime' seconds that it was hit by this hit box 
				StartCoroutine (GenerateHit(targetObject, hitBox, startTime));
			}
		}
	}

	/// <summary>
	/// Enable the hit boxes for the given action
	/// </summary>
	public void EnableHitBoxes(Action action)
	{
		// If the given action is null, ignore this call
		if(action == null)
			return;

		// Cycle through each hit box belonging to the given action
		for(int i = 0; i < action.hitBoxes.Length; i++)
		{
			HitBox hitBoxInfo = action.hitBoxes[i];

			// Enable the hit boxes
			hitBoxInfo.Enable ();
		}
	}

	/// <summary>
	/// Disable the hit boxes for the given action. Allows the action to perform collisions
	/// </summary>
	public void DisableHitBoxes(Action action)
	{
		// Ignore this call if the given action is null
		if(action == null)
			return;

		// Cycle through each hit box belonging to the given action
		for(int i = 0; i < action.hitBoxes.Length; i++)
		{
			HitBox hitBoxInfo = action.hitBoxes[i];
			
			// Disable the hit boxes
			hitBoxInfo.Disable();
		}
	}

	/// <summary>
	/// Creates the hit box for the given action.
	/// </summary>
	private void CreateHitBoxes(Action action)
	{
		// Cycle through each HitBox assigned to the action
		for(int i = 0; i < action.hitBoxes.Length; i++)
		{
			// Cache the HitBox being cycled through
			HitBox hitBoxInfo = action.hitBoxes[i];
			
			// Create a new GameObject for the hit box
			GameObject gameObject = new GameObject("HitBox");
			gameObject.transform.parent = transform;	// TODO: Cache both Transforms
			gameObject.layer = Brawler.Layer.HitBox; // This hit box inflicts damage
			
			// Attach a collider to the hit box
			BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
			collider.offset = hitBoxInfo.offset;
			collider.size = hitBoxInfo.size;
			collider.isTrigger = true;
			
			// Creates a BoneFollower instance which allows the HitBox to follow a bone's actionment
			BoneFollower boneFollower = gameObject.AddComponent<BoneFollower>();
			boneFollower.SkeletonRenderer = skeleton;
			boneFollower.boneName = hitBoxInfo.boneName;
			boneFollower.followBoneRotation = true;
			boneFollower.followZPosition = true;

			// Creates a hitBoxObject component so that the hit box can keep a reference to the HitBox object that it represents
			HitBoxObject hitBox = gameObject.AddComponent<HitBoxObject>();
			hitBox.HitBoxInfo = hitBoxInfo;
			
			// Cache the HitBox's GameObject and components inside the data container object
			hitBoxInfo.GameObject = gameObject;
			hitBoxInfo.Character = character;
			hitBoxInfo.Action = action;
			hitBoxInfo.Collider = collider;
			
			// Disable the hit box until the attack action is performed.
			hitBoxInfo.Disable();
		}
		
	}

	/// <summary>
	/// Called when the given hit box comes into contact with one of this character's hurt boxes.
	/// This method inflicts damage to the character based on the hit box's stats
	/// </summary>
	public void OnCollision(Collider2D hitBox)
	{
		// Perform the 'Hit' action, making the character display his hit animation
		character.CharacterControl.PerformAction (actionSet.basicActions.hit);

		// Informs the character that was hit that his CharacterAI component must adapt to the fact that he was hit
		character.CharacterAI.OnHit ();

		// Cache the hit box's components to determine the properties of the hit box which hit this character
		HitBoxObject hitBoxObject = hitBox.GetComponent<HitBoxObject>();

		// Inform the hit box that it just hit this character. The hit box will deal damage to the character and play impact sounds
		hitBoxObject.OnHit(character);
	}

	/** Detect collisions from incoming hit boxes */
	void OnTriggerEnter2D(Collider2D collider)
	{
		// If the character was hit by a hit box, inflict damage to the character
		if(collider.gameObject.layer == Brawler.Layer.HitBox)
		{
			// Generate a collision between this character and the collider which hit this character
			OnCollision(collider);
		}
	}

	/// <summary>
	/// Generates a hit between the given hit box and the target object. This method simply calls the targetObject's
	/// OnCollision() method after 'startTime' seconds, using the hit box's collider.
	/// </summary>
	private IEnumerator GenerateHit(GameObject targetObject, HitBox hitBox, float startTime)
	{
		// Wait 'startTime' seconds before calling the targetObject's OnCollision() method
		yield return StartCoroutine (Wait (startTime));

		// Cache the target object's Character component to avoid an excessive number of lookups
		Character targetCharacter = targetObject.GetComponent<Character>();

		// Generate an artificial collision between the given hit box and its target. This inflicts damage on the target.
		targetCharacter.CharacterCollider.OnCollision (hitBox.Collider);
	}

	/** Coroutine which waits for a given amount of seconds before resuming execution of the statements which follow */
	private IEnumerator Wait(float duration)
	{
		// Yield every frame until the timer reached the designated number of seconds
		for(float timer = 0; timer < duration; timer += Time.deltaTime)
			yield return null;
	}
}
