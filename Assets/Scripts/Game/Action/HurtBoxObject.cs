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
		// Inform the CharacterStats component that he was hit by a hurt box. Inflicts damage to the character hit by this hurt box
		adversary.CharacterStats.OnHit(hitBoxInfo.hitInfo, hitBoxInfo.Character);

		// Increment the combo for the character belonging to this hurt box. This character just hit an opponent and must thus increase his combo
		hitBoxInfo.Character.CharacterStats.IncrementCombo();
		
		// Play one of the impact sounds associated to this hurt box. This plays a sound right when the hurt box hits a target
		hitBoxInfo.Character.Sound.PlayRandomSound (hitBoxInfo.Action.impactSounds);
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
