using UnityEngine;
using System.Collections;

public class HurtBoxObject : MonoBehaviour 
{
	/** Stores the action which activated this hurt box. */
	private Action action;
	
	void OnTriggerEnter2D(Collider2D other)
	{
		// If this hurt box just hit a hit box, play an impact sound
		if(other.gameObject.layer == Brawler.Layer.HitBox)
		{
			// Play an impact sound, since this hurt box just hit an adversary
			action.character.Sound.PlayRandomSound (action.impactSounds);
		}
	}

	/// <summary>
	/// The Action which activated this hurt box.
	/// </summary>
	/// <value>The hurt box info.</value>
	public Action Action
	{
		get { return action; }
		set { this.action = value; }
	}
}
