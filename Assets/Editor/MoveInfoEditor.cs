using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MoveInfo))]
public class MoveInfoEditor : Editor 
{
	private bool showAnimationFoldout = true;

	public override void OnInspectorGUI()
	{
		// The MoveInfo instance being edited by this inspector
		MoveInfo moveInfo = (MoveInfo) target;

		// Add undo support to the MoveInfo instance.
		Undo.RecordObject (target, "Move Info Undo");

		EditorGUILayout.BeginVertical ();
		{
			EditorGUI.indentLevel = 0;

			// Display an "Animation" foldout
			showAnimationFoldout = EditorGUILayout.Foldout (showAnimationFoldout, "Animation");

			if(showAnimationFoldout)
			{
				EditorGUILayout.BeginVertical ();
				{
					// Display each AnimationSequence
					for(int i = 0; i < moveInfo.animationSequences.Length; i++)
					{
						AnimationSequence animationSequence = moveInfo.animationSequences[i];
						EditorGUI.indentLevel = 1;

						// "Animation Sequence" header
						EditorGUILayout.BeginHorizontal ();
						{
							EditorGUILayout.LabelField ("Animation Sequence " + i);
							
							// Delete animation sequence
							if(GUILayout.Button ("Delete", GUILayout.Width (50)))
								moveInfo.animationSequences = ArrayUtils.Remove<AnimationSequence>(moveInfo.animationSequences,
								                                     animationSequence);
						}
						EditorGUILayout.EndHorizontal ();
						
						// Edit individual animations from animation sequence
						for(int j = 0; j < animationSequence.animations.Length; j++)
						{
							// Animation text field
							EditorGUILayout.BeginHorizontal ();
							{
								EditorGUILayout.PrefixLabel (j + ":");
								animationSequence.animations[j] = EditorGUILayout.TextField (animationSequence.animations[j]);

								// Delete animation string from animation sequence
								if(GUILayout.Button ("X", GUILayout.Width (40)))
									animationSequence.animations = ArrayUtils.Remove<string>(animationSequence.animations,
									                          	   							 animationSequence.animations[j]);
							}
							EditorGUILayout.EndHorizontal ();
						}

						// Add animation ("+") button
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField ("");
							// Add animation string
							if(GUILayout.Button ("+", GUILayout.Width (40)))
								animationSequence.animations = ArrayUtils.Add<string>(animationSequence.animations,"");
						}
						EditorGUILayout.EndHorizontal ();
					}

					// "New Animation Sequence" button
					if(GUILayout.Button ("New Animation Sequence"))
						moveInfo.animationSequences = ArrayUtils.Add<AnimationSequence>(moveInfo.animationSequences,
						                                                                new AnimationSequence());
				}
				EditorGUILayout.EndVertical (); // End animation foldout
			}
		}
		EditorGUILayout.EndVertical ();
	}
}
