using UnityEngine;
using System.Collections;

public class PlayerAnimator : CharacterAnimator
{
	void Start()
	{
		// Initalizes base class variables 
		base.Start();

		skeleton.Skeleton.FindSlot("Rifle").Attachment = null;
		skeleton.Skeleton.FindSlot("Teleporter").Attachment = null;
	}
}

