using UnityEngine;
using System.Collections;

public class DebugTimeScale : MonoBehaviour {

	void Awake()
	{
		StartCoroutine (PrintTimeScale());
	}

	public IEnumerator PrintTimeScale()
	{
		while(true)
		{
			Debug.Log ("Time scale: " + Time.timeScale);
			
			yield return null;
		}
	}
}
