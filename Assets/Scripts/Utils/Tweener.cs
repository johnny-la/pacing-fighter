using UnityEngine;
using System.Collections;

/// <summary>
/// Denotes the property to tween 
/// </summary>
[System.Flags]
public enum TweenType
{
	Position = 1,
	Scale = 2,
	Rotation = 4
}

/// <summary>
/// Adds tweening functionality to the GameObject this script is attached to.
/// </summary>
public class Tweener : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		
	}

	/// <summary>
	/// Performs the given tweeing event on this GameObject
	/// </summary>
	public void PerformEvent(TweenEvent tweenEvent, float duration)
	{
		// If the tween modifies position
		if((tweenEvent.type & TweenType.Position) == TweenType.Position)
		{
			// Perform a tween on the GameObject's localPosition, lasting 'duration' seconds 
			LeanTween.moveLocal(gameObject, tweenEvent.targetPosition, duration).setEase (tweenEvent.positionEasingType);
		}

		// If the tween modifies scale
		if((tweenEvent.type & TweenType.Scale) == TweenType.Scale)
		{
			// Perform a tween on the GameObject's scale, lasting 'duration' seconds 
			LeanTween.scaleX(gameObject, tweenEvent.targetScale.x, duration).setEase (tweenEvent.scaleEasingType);
			LeanTween.scaleY(gameObject, tweenEvent.targetScale.y, duration).setEase (tweenEvent.scaleEasingType);
			LeanTween.scaleZ(gameObject, tweenEvent.targetScale.z, duration).setEase (tweenEvent.scaleEasingType);
		}

		// If the tween modifies rotation
		if((tweenEvent.type & TweenType.Rotation) == TweenType.Rotation)
		{
			// Perform a tween on the GameObject's scale, lasting 'duration' seconds 
			LeanTween.rotateZ(gameObject, tweenEvent.targetAngle, duration).setEase (tweenEvent.rotationEasingType);
		}

		Debug.Log ("PERFORM THE TWEEN: " + tweenEvent.ToString ());
		Debug.Log ("Duration: " + duration + " seconds");
	}
}

