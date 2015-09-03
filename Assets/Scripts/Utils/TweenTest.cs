using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Tests multiple tweening events.
/// </summary>
[System.Serializable]
[RequireComponent(typeof(Tweener))]
[ExecuteInEditMode]
public class TweenTest : MonoBehaviour
{
	public Brawler.Event[] tweenEvents = new Brawler.Event[0];

	private Tweener tweener;

	private Vector3 defaultLocalPosition, defaultScale = new Vector3(1,1,1);
	private float defaultAngle;

	// Use this for initialization
	void Start ()
	{
		tweener = gameObject.GetComponent<Tweener>();
	}

	public void Play()
	{
		tweener = gameObject.GetComponent<Tweener>();

		Reset ();

		defaultLocalPosition = transform.localPosition;
		defaultScale = transform.localScale;
		defaultAngle = transform.rotation.eulerAngles.z;

		Coroutine updateSceneCoroutine = StartCoroutine (UpdateScene());

		for(int i = 0; i < tweenEvents.Length; i++)
		{
			StartCoroutine (PlayTween(tweenEvents[i]));
		}

		StopCoroutine (updateSceneCoroutine);

	}

	private IEnumerator PlayTween(Brawler.Event e)
	{
		float startTime = 0;
		if(e.startTime.type == DurationType.Frame)
		{
			startTime = e.startTime.nFrames / CharacterAnimator.FRAME_RATE;
		}
		else
		{
			startTime = e.startTime.seconds;
		}

		Debug.Log ("Play tween at " + startTime + " seconds");

		yield return new WaitForSeconds(startTime);

		float duration;

		if(e.duration.type == DurationType.Frame)
		{
			duration = e.duration.nFrames / CharacterAnimator.FRAME_RATE;
		}
		else
		{
			duration = e.duration.seconds;
		}

		Debug.Log ("Play tween for " + duration + " seconds");

		tweener.PerformEvent (e.tweenEvent, duration);

	}

	private IEnumerator UpdateScene()
	{
		while(true)
		{
			SceneView.RepaintAll ();
			yield return null;
		}
	}

	public void Reset()
	{
		transform.localPosition = defaultLocalPosition;
		transform.localScale = defaultScale;

		Vector3 temp = transform.rotation.eulerAngles;
		temp.z = defaultAngle;
		transform.eulerAngles = temp;
	}

	// Update is called once per frame
	void Update ()
	{
		
	}

	public Tweener Tweener
	{
		get { return tweener; }
	}
}

