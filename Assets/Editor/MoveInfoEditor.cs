using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MoveInfo))]
public class MoveInfoEditor : Editor 
{
	private bool showAnimationFoldout = true; // If true, animation foldout is rolled open
	private bool showInputFoldout = true;

	public override void OnInspectorGUI()
	{
		// The MoveInfo instance being edited by this inspector
		MoveInfo moveInfo = (MoveInfo) target;

		// Add undo support to the MoveInfo instance.
		Undo.RecordObject (target, "Move Info Undo");

		EditorGUILayout.BeginVertical ();
		{
			/*********************
			 * ANIMATION FOLDOUT *
			**********************/
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
			} // End animation foldout

			/*****************
			 * INPUT FOLDOUT *
			 *****************/
			EditorGUI.indentLevel = 0;

			// Display the animation foldout
			showInputFoldout = EditorGUILayout.Foldout (showInputFoldout, "Input");

			if(showInputFoldout)
			{
				EditorGUI.indentLevel = 1;
				// Input Foldout contents
				EditorGUILayout.BeginVertical ();
				{
					moveInfo.inputType = (InputType)EditorGUILayout.EnumPopup ("Input Type:", moveInfo.inputType);
					if(moveInfo.inputType == InputType.Swipe)
						moveInfo.swipeDirection = (SwipeDirection)EditorGUILayout.EnumPopup ("Swipe Direction:", 
						                                                                     moveInfo.swipeDirection);
					moveInfo.inputRegion = (InputRegion)EditorGUILayout.EnumPopup ("Input Region:", moveInfo.inputRegion);

				}
				EditorGUILayout.EndVertical ();
			}
		}
		EditorGUILayout.EndVertical ();	
	}
}
