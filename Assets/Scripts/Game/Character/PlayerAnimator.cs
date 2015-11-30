using UnityEngine;
using System.Collections;
//using Xft; // Weapon trail asset

public class PlayerAnimator : CharacterAnimator
{
	//private XWeaponTrail weaponTrail;

	void Start()
	{
		// Initalizes base class variables 
		base.Start();

		skeleton.Skeleton.FindSlot("Rifle").Attachment = null;
		skeleton.Skeleton.FindSlot("Teleporter").Attachment = null;

		//weaponTrail = transform.FindChild ("Graphics").FindChild ("Anchors").FindChild ("Weapon_Slot").FindChild ("X-WeaponTrail").GetComponent<XWeaponTrail>();
	}

	/*void Update()
	{
		base.Update ();

		// If the character is performing a combat move
		if(!(character.CharacterControl.CurrentAction is BasicAction))
		{
			// Activate the weapon trail
			//weaponTrail.Activate ();
		}
		else
		{
			//weaponTrail.Deactivate ();
		}
	}*/
}

