using UnityEditor;
using UnityEngine;
using System;
using System.Collections;

[CustomEditor(typeof(TweenTest))]
public class TweenTestEditor : Editor
{
	private EventsFoldout tweenEventsFoldout;

	public void OnEnable()
	{
		TweenTest tweenTest = (TweenTest) target;

		tweenEventsFoldout = new EventsFoldout("Tween Events", tweenTest.tweenEvents);
	}

	public override void OnInspectorGUI()
	{
		TweenTest tweenTest = (TweenTest) target;

		if(tweenEventsFoldout == null)
			tweenEventsFoldout = new EventsFoldout("Tween Events", tweenTest.tweenEvents);

		tweenTest.tweenEvents = tweenEventsFoldout.Display ();

		if(GUILayout.Button ("Play"))
		{
			tweenTest.Play ();	
			SceneView.RepaintAll();
		}

		if(GUILayout.Button ("Reset"))
		{
			tweenTest.Reset();
			SceneView.RepaintAll();
		}

		// Add undo support
		Undo.RecordObject (tweenTest, "Tween Test Undo");
	}
}

