using UnityEngine;
using System.Collections;

public class HurtBoxObject : MonoBehaviour 
{
	/** Stores the hit box script  which activated this hurt box. */
	private HitBox hitBoxInfo;

	/// <summary>
	/// Called when this hurt box hits another character. This method is called from the CharacterCollider.OnCollision()
	/// method when a hurt box enters a character's collider. This method performs necessary actions, such as inflicting
	/// damage on the character and playing an impact sound.
	/// </summary>
	public void OnHit(Character adversary)
	{
		// Stores the 'HitInfo' instance which dictates how much damage to deal when this HitBox hurts an adversary
		HitInfo hitInfo = hitBoxInfo.hitInfo;

		// Inform the CharacterStats component that he was hit by a hurt box. Inflicts damage to the character hit by this hurt box
		adversary.CharacterStats.OnHit(hitInfo, hitBoxInfo.Character);

		// Generate a knockback force on the adversary
		GenerateKnockback(adversary);

		// Increment the combo for the character belonging to this hurt box. This character just hit an opponent and must thus increase his combo
		hitBoxInfo.Character.CharacterStats.IncrementCombo();
		
		// Play one of the impact sounds associated to this hurt box. This plays a sound right when the hurt box hits a target
		hitBoxInfo.Character.Sound.PlayRandomSound (hitBoxInfo.Action.impactSounds);
	}

	/// <summary>
	/// Called when the given character was hit by this HurtBox. The adversary is sent flying back according to the 
	/// hurt box's HitInfo instance, which dictates the speed of the knockback. 
	/// </summary>
	private void GenerateKnockback(Character adversary)
	{
		// Stores the 'HitInfo' instance which dictates the speed and time of the knockback
		HitInfo hitInfo = hitBoxInfo.hitInfo;

		// Stores the direction in which the adversary should be knocked back
		Direction knockbackDirection = Direction.Right;
		
		// If the character which hit the adversary is to the right of his adversary, the knockback should send the adversary flying left
		if(hitBoxInfo.Character.Transform.position.x > adversary.Transform.position.x)
			knockbackDirection = Direction.Left;
		
		// Apply a knockback force to the adversary which was hit by this hurt box. The 'HitInfo' instance stores the speed and time of the knockback.
		adversary.CharacterForces.Knockback (hitInfo, knockbackDirection);

		//hitBoxInfo.Character.CharacterForces.Knockback (hitInfo, knockbackDirection);
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		// If this hurt box just hit a hit box, do something
		if(other.gameObject.layer == Brawler.Layer.HitBox)
		{
			// 
			// Play an impact sound, since this hurt box just hit an adversary
			hitBoxInfo.Action.character.Sound.PlayRandomSound (hitBoxInfo.Action.impactSounds);
		}
	}

	/// <summary>
	/// The Action which activated this hurt box.
	/// </summary>
	/// <value>The hurt box info.</value>
	public HitBox HitBoxInfo
	{
		get { return hitBoxInfo; }
		set { this.hitBoxInfo = value; }
	}
}
