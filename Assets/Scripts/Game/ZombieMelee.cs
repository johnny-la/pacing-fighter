using System.Collections;
using UnityEngine;

public class ZombieMelee : MonoBehaviour
{
	public void Melee()
	{
		SkeletonAnimation skeleton = base.GetComponentInChildren<SkeletonAnimation>();
		skeleton.AnimationName = "Melee_Front_Swing";
		skeleton.state.SetAnimation(0, "Melee_Front_Swing", false);
		skeleton.state.AddAnimation(0, "Idle", false, 0f);
		base.StartCoroutine(this.AttackCollider());
	}
	
	public IEnumerator AttackCollider()
	{
		BoxCollider2D attackCollider = GameObject.Find ("ArmCollider").GetComponent<BoxCollider2D>();
		
		yield return new WaitForSeconds(1.0f);
		attackCollider.enabled = true;
		yield return new WaitForSeconds(0.2f);
		attackCollider.enabled = false;
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider.name == "AxeCollider")
		{
			Debug.Log("Zombie hit");
			SkeletonAnimation componentInChildren = base.GetComponentInChildren<SkeletonAnimation>();

			if(componentInChildren.AnimationName == null || 
			   !componentInChildren.AnimationName.Equals("Hit_Head"))
			{
				componentInChildren.state.SetAnimation(0, "Hit_Head", false);
				componentInChildren.state.AddAnimation(0, "Idle", false, 0f);
			}
		}
	}
}
