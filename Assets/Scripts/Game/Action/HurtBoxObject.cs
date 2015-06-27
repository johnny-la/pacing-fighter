using UnityEngine;
using System.Collections;

public class HurtBoxObject : MonoBehaviour 
{
	/** Stores the hit box script  which activated this hurt box. */
	private HitBox hitBoxInfo;
	
	void OnTriggerEnter2D(Collider2D other)
	{
		// If this hurt box just hit a hit box, play an impact sound
		if(other.gameObject.layer == Brawler.Layer.HitBox)
		{
			// Play an impact sound, since this hurt box just hit an adversary
			hitBoxInfo.Action.character.Sound.PlayRandomSound (hitBoxInfo.Action.impactSounds);
		}
	}

	/// <summary>
	/// The Action which activated this hurt box.
	/// </summary>
	/// <value>The hurt box info.</value>
	public HitBox HitBox
	{
		get { return hitBoxInfo; }
		set { this.hitBoxInfo = value; }
	}
}
