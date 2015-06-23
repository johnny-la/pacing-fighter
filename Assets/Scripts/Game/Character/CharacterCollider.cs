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
		
		// Cycle through every attack action in the action set
		for(int i = 0; i < actionSet.combatActions.Length; i++)
		{
			// Create the HitBoxes for the attack actions.
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
			EnableHitBoxes(character.CharacterControl.Currentaction);
			break;
		case DisableHitBoxEvent: 
			// Disable the hit boxes for the character's current action
			DisableHitBoxes (character.CharacterControl.Currentaction);
			break;
		}
	}

	/// <summary>
	/// Enable the hit boxes for the given action
	/// </summary>
	public void EnableHitBoxes(Action action)
	{
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
	private void CreateHitBoxes(Action combatAction)
	{
		// Cycle through each HitBox assigned to the action
		for(int i = 0; i < combatAction.hitBoxes.Length; i++)
		{
			// Cache the HitBox being cycled through
			HitBox hitBoxInfo = combatAction.hitBoxes[i];
			
			// Create a new GameObject for the hit box
			GameObject gameObject = new GameObject("HitBox");
			gameObject.transform.parent = transform;	// TODO: Cache both Transforms
			gameObject.layer = Brawler.Layer.HurtBox; // This hit box inflicts damage
			
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

			// Creates a HurtBoxObject component so that the hit box can keep track of the action that triggered this hit box
			HurtBoxObject hurtBox = gameObject.AddComponent<HurtBoxObject>();
			hurtBox.Action = combatAction;
			
			// Cache the HitBox's GameObject and components inside the data container object
			hitBoxInfo.GameObject = gameObject;
			hitBoxInfo.Character = character;
			hitBoxInfo.Collider = collider;
			
			// Disable the hit box until the attack action is performed.
			hitBoxInfo.Disable();
		}
		
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		// If the character was hit by a hurt box, inflict damage to the character
		if(collider.gameObject.layer == Brawler.Layer.HurtBox)
		{
			// Perform the 'Hit' action, making the character display his hit animation
			character.CharacterControl.PerformAction (actionSet.basicActions.hit);

			Debug.Log ("Disable " + character.CharacterControl.Currentaction + " hit boxes from " + gameObject.name);
		}
	}
}
