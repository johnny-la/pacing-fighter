using UnityEngine;
using System.Collections;

public class HitBoxObject : MonoBehaviour 
{
	/** Stores the hit box script which controls this collider. */
	private HitBox hitBoxInfo;

	/// <summary>
	/// Called when this hit box hits another character. This method is called from the CharacterCollider.OnCollision()
	/// method when a hit box enters a character's collider. This method performs necessary actions, such as inflicting
	/// damage on the character and playing an impact sound.
	/// </summary>
	public void OnHit(Character adversary)
	{
		// Stores the 'HitInfo' instance which dictates how much damage to deal when this HitBox hits an adversary
		HitInfo hitInfo = hitBoxInfo.hitInfo;

		// Inform the CharacterStats component that he was hit by a hit box. Inflicts damage to the character hit by this hit box
		adversary.CharacterStats.OnHit(hitInfo, hitBoxInfo.Character);

		// Generate a knockback force on the adversary
		Knockback(adversary);

		// Freeze the characters' animations for a small amount of time to add impact to the hit
		FreezeAnimations(hitBoxInfo.Character, adversary);

		// Cycle through the events in the 'hitInfo.selfEvents' array. These events are supposed to be executed by the character which
		// generated this hit
		for(int i = 0; i < hitInfo.selfEvents.Length; i++)
		{
			// Cache the event being cycled through
			Brawler.Event e  = hitInfo.selfEvents[i];

			// Make the character that hit the adversary perform the events specified in the 'hitInfo.events' array. These events
			// are meant to be triggered when the hit is registered.
			hitBoxInfo.Character.CharacterControl.PerformEvent(hitInfo.selfEvents[i]);
		}

		// Cycle through the events in the 'hitInfo.adversaryEvents' array. These events are supposed to be executed by the character which
		// received the hit
		for(int i = 0; i < hitInfo.adversaryEvents.Length; i++)
		{
			// Make the character that got hit (the adversary) perform the events specified in the 'hitInfo.adversaryEvents' array. These events
			// are meant to be triggered when the hit is registered.
			adversary.CharacterControl.PerformEvent(hitInfo.adversaryEvents[i]);
		}

		// Increment the combo for the character belonging to this hit box. This character just hit an opponent and must thus increase his combo
		hitBoxInfo.Character.CharacterStats.IncrementCombo();
		
		// Play one of the impact sounds associated to this hit box. This plays a sound right when the hit box hits a target
		hitBoxInfo.Character.Sound.PlayRandomSound (hitBoxInfo.Action.impactSounds);
	}

	/// <summary>
	/// Called when the given character was hit by this hitBox. The adversary is sent flying back according to the 
	/// hit box's HitInfo instance, which dictates the speed of the knockback. 
	/// IMPORTANT: Must be called after damage has been dealt to the adversary to know if the adversary died on impact.
	/// If so, the adversary is sent flying back to his death (i.e., his 'DeathKnockback' action is performed).
	/// </summary>
	private void Knockback(Character adversary)
	{
		// Stores the 'HitInfo' instance which dictates the speed and time of the knockback
		HitInfo hitInfo = hitBoxInfo.hitInfo;

		// Stores the direction in which the adversary should be knocked back
		Direction knockbackDirection = Direction.Right;
		
		// If the character which hit the adversary is to the right of his adversary, the knockback should send the adversary flying left
		if(hitBoxInfo.Character.Transform.position.x > adversary.Transform.position.x)
			knockbackDirection = Direction.Left;

		// If the adversary did not die from the hit, perform a regular knockback
		if(!adversary.CharacterStats.IsDead ())
		{
			// Apply a knockback force to the adversary which was hit by this hit box. The 'HitInfo' instance stores the speed and time of the knockback.
			adversary.CharacterForces.Knockback (hitInfo, knockbackDirection);
		}
		// Else if the character died from the hit, make him perform his 'DeathKnockback' action
		else
		{
			// Make the character that was hit (the adversary) perform a 'death knockback'. This is a strong knockback which leaves him on the ground.
			adversary.CharacterControl.DeathKnockback (hitInfo, knockbackDirection);
		}		

		//hitBoxInfo.Character.CharacterForces.Knockback (hitInfo, knockbackDirection);
	}

	/// <summary>
	/// Freeze the characters' animations for a small amount of time to add impact to the hit
	/// </summary>
	private void FreezeAnimations(Character character, Character adversary)
	{
		// Determine the amount of seconds the characters should freeze for.
		float freezeDuration = hitBoxInfo.hitInfo.freezeFrames / CharacterAnimator.FRAME_RATE;

		// Freeze the characters' animations for the duration computed above
		character.CharacterAnimator.FreezeAnimation (freezeDuration);
		adversary.CharacterAnimator.FreezeAnimation (freezeDuration);
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		// If this hit box just hit a hurt box, inflict damage to it
		if(other.gameObject.layer == Brawler.Layer.HurtBox)
		{

		}
	}

	/// <summary>
	/// The Action which activated this hit box.
	/// </summary>
	/// <value>The hit box info.</value>
	public HitBox HitBoxInfo
	{
		get { return hitBoxInfo; }
		set { this.hitBoxInfo = value; }
	}
}
