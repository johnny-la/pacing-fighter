using UnityEngine;
using System.Collections;

public class ShakeRotation : MonoBehaviour 
{
	// The default pivot angle of the GameObject
	public float defaultAngle;


	public float shakeTime;

	[Tooltip("The max z-angle of the GameObject in degrees once it shakes.")]
	public float maxShakeAngle = 45;

	void Start () 
	{
		StartCoroutine(ShakeCoroutine());
	}
	
	// Update is called once per frame
	void Update () 
	{

	}

	public IEnumerator ShakeCoroutine()
	{
		while(true)
		{
			yield return new WaitForSeconds(2.0f);
			
			Shake ();
		}
	}

	/// <summary>
	/// Shake this GameObject around its target angle.
	/// </summary>
	public void Shake()
	{
		LTDescr tweenDescription = LeanTween.rotateZ (gameObject, maxShakeAngle, shakeTime/5).setEase (LeanTweenType.easeOutQuad);
		LeanTween.rotateZ (gameObject, defaultAngle, shakeTime).setEase (LeanTweenType.easeOutElastic).setDelay (shakeTime/5);
	}
}
