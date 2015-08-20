using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// =============================================================================
public class Sphere : MonoBehaviour {
	
	public AudioClip answerMissedClip;
	public Transform spawnRoot;
	public float flingStrength = 1000.0f;

	public new Transform transform;
	public new Rigidbody rigidbody;
	
	bool isBeingThrown = false;
	bool isHeld = false;
	bool shouldLerpToOrigin = false;
	bool shouldPlayToneASAP = false;
	
	float radius = 1.0f;
	
	Vector3 spawnPosition;
	Transform thisTransform;
	
	
#region Public Methods	
	
	// -------------------------------------------------------------------------
	public float GetDistanceFromBoxCenter() {
		
		Vector3 center = Camera.main.transform.position;
		center.z = thisTransform.position.z;
		
		return (thisTransform.position - center).magnitude;
	}
	
#endregion

#region Monobehaviour methods
	// -------------------------------------------------------------------------
	void Start () {
		
		thisTransform = transform;		
		spawnPosition = spawnRoot.position;

		transform = GetComponent<Transform>();
		rigidbody = GetComponent<Rigidbody>();
		
		StartCoroutine("ManualUpdate");
	}
	
	// -------------------------------------------------------------------------
	void OnDestroy() {
		StopCoroutine("ManualUpdate");
	}
	
	// -------------------------------------------------------------------------
	IEnumerator ManualUpdate() {
		
		yield return null;	
		
		while (true) {
			
			if (shouldLerpToOrigin) {
				rigidbody.isKinematic = true;
				yield return StartCoroutine(LerpToStartPosition());
				shouldLerpToOrigin = false;
				rigidbody.isKinematic = false;
			}
			
			UpdateGrabSphere();
			
			if (isHeld) {
				
				float depth = thisTransform.position.z - Camera.main.transform.position.z;
				
				// update position
				Vector3 newWorldPoint = Camera.main.ScreenToWorldPoint(
					new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth));
				
				Debug.Log (newWorldPoint);
				
				Vector2 newWorldPoint2D = new Vector2(newWorldPoint.x, newWorldPoint.y);
				Vector3 direction = newWorldPoint2D.normalized;
				float distance2D = newWorldPoint2D.magnitude;
				
				// match position with mouse/touch or clamp if we're too far
				if (distance2D > radius) {					
					
					thisTransform.localPosition = direction * radius;					
					distance2D = radius;
				} else {
				
					thisTransform.position = newWorldPoint;							
				}
				
				float percentDistance = distance2D / radius;
				
				// If we let go of the button or we are out of bounds, release the ball
				if (Input.GetMouseButtonUp(0)) {
					
					isHeld = false;
						
					if (distance2D > 0.1) {
						yield return StartCoroutine(Sling());									
						isBeingThrown = true;
					} else {
						thisTransform.position = spawnPosition;
					}
					
				} 
			}	
			
			yield return null;
		}
	}
	
	// -------------------------------------------------------------------------
	void UpdateGrabSphere() {
		
		bool canGrabSphere = GetDistanceFromBoxCenter() < radius;
			
		if (canGrabSphere) {
			
			if (Input.GetMouseButtonDown(0)) {
		
				Ray screenRay =
					Camera.main.ScreenPointToRay(Input.mousePosition);
				
				RaycastHit hit = new RaycastHit();
				if (Physics.Raycast(screenRay, out hit)) {
					
					if (hit.transform == thisTransform) {
						
						isHeld = true;							
						rigidbody.isKinematic = true;								
					}				
				}
			}				
		}
	}
	
	// -------------------------------------------------------------------------
	IEnumerator LerpToStartPosition() {
		
		Vector3 startPosition = thisTransform.position;
		
		float time = 0.0f;
		float duration = 0.4f;
		
		while (time < duration) {
			
			time += Time.deltaTime;
			
			float adjustedTime = Util.Easing.EaseIn(time / duration,Util.EasingType.Sine);
			thisTransform.position = Vector3.Lerp(startPosition, spawnPosition, adjustedTime);
			
		
			yield return null;
		}
		
		thisTransform.position = spawnPosition;
	}
	
	// -------------------------------------------------------------------------
	 void OnCollisionEnter(Collision collisionInfo) {
		
		if (!isBeingThrown) {
			return;
		}
		
        isBeingThrown = false;		
		shouldLerpToOrigin = true;
		
		Camera.main.GetComponent<PerlinShake>().PlayShake();
		//Camera.main.GetComponent<RandomShake>().PlayShake();
    }
	
#endregion

	
#region Private Methods
	
	// -------------------------------------------------------------------------
	IEnumerator Sling() {
		
		rigidbody.isKinematic = false;
		
		float distanceFromStart = 0.0f;
		float distanceToCenter;
		
		Vector2 spawnPos = new Vector2(spawnPosition.x, spawnPosition.y);
		Vector2 startPos = new Vector2(thisTransform.position.x, thisTransform.position.y);
		Vector2 direction = (spawnPos - startPos);
		
		distanceToCenter = direction.magnitude;
		direction.Normalize();
		
		//Debug.Log(direction);
		
		do {
			yield return new WaitForFixedUpdate();
			
			Vector2 currentPos = new Vector2(thisTransform.position.x, thisTransform.position.y);		
			distanceFromStart = (startPos - currentPos).magnitude;			
			
			float springDisplacement = 1.0f + distanceToCenter - distanceFromStart;
			
			float xForce = direction.x * springDisplacement * Time.fixedDeltaTime * flingStrength;
			float yForce = direction.y * springDisplacement * Time.fixedDeltaTime * flingStrength;
			
			rigidbody.AddForce(new Vector3(xForce, yForce, 0.0f));
				
		} while (distanceFromStart <= distanceToCenter);		
	}
	
#endregion
	
}
