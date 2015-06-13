using UnityEngine;
using System.Collections;

public class PlayerMelee : MonoBehaviour
{
	private PlayerState playerState;

	public Transform axe;
	
	private void Awake()
	{
		this.playerState = GetComponent<PlayerState>();
	}

	public void Melee()
	{
		SkeletonAnimation componentInChildren = base.GetComponentInChildren<SkeletonAnimation>();
		componentInChildren.AnimationName = "Melee";
		componentInChildren.state.SetAnimation(0, "Melee", false);
		componentInChildren.state.AddAnimation(0, "Idle", false, 0f);
		base.StartCoroutine(this.AxeCollider());
	}
	
	public IEnumerator AxeCollider()
	{
		BoxCollider2D axeCollider = axe.GetComponent<BoxCollider2D>();
		
		yield return new WaitForSeconds(0.2f);
		axeCollider.enabled = true;
		yield return new WaitForSeconds(0.3f);
		axeCollider.enabled = false;
	}

	public PlayerState PlayerState
	{
		get { return this.playerState; }
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		if(collider.name == "ArmCollider")
		{
			SkeletonAnimation skeleton = base.GetComponentInChildren<SkeletonAnimation>();
			skeleton.AnimationName = "Hit";
			skeleton.state.SetAnimation(0, "Hit", false);
			skeleton.state.AddAnimation(0, "Idle", false, 0f);
		}
	}
}
