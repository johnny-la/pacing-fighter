using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ActionSet))]
public class ActionSetEditor : Editor 
{
	private bool showBasicActionsFoldout = true;
	private bool showCombatActionsFoldout = true;

	private bool showHitSoundsFoldout = false;

	public override void OnInspectorGUI()
	{
		// The ActionSet instance being edited by this inspector
		ActionSet actionSet = (ActionSet) target;
		BasicActions basicActions = actionSet.basicActions;

		// Add undo support to the ActionSet instance.
		Undo.RecordObject (actionSet, "Action Set Undo");

		/*************************
		 * BASIC ACTIONS FOLDOUT *
		 * ***********************/
		EditorGUI.indentLevel = 0;

		showBasicActionsFoldout = EditorGUILayout.Foldout (showBasicActionsFoldout, "Basic Actions");

		if(showBasicActionsFoldout)
		{
			EditorGUILayout.BeginVertical ();
			{
				EditorGUI.indentLevel = 1;

				// Set the default animations for each basic move
				basicActions.IdleAnimation = EditorGUILayout.TextField ("Idle Animation:", basicActions.IdleAnimation);
				basicActions.WalkAnimation = EditorGUILayout.TextField ("Walk Animation:", basicActions.WalkAnimation);
				basicActions.HitAnimation = EditorGUILayout.TextField ("Hit Animation:", basicActions.HitAnimation);

				/** Hit sounds foldout */
				showHitSoundsFoldout = EditorGUILayout.Foldout (showHitSoundsFoldout, "SFX: On Hit (" + basicActions.hit.startSounds.Length + ")");
				
				if(showHitSoundsFoldout)
				{
					EditorGUI.indentLevel = 2;
					
					// Display each possible hit sound (one is chosen at random when move is performed)
					for(int i = 0; i < basicActions.hit.startSounds.Length; i++)
					{
						// Edit hit sound
						EditorGUILayout.BeginHorizontal ();
						{
							basicActions.hit.startSounds[i] = (AudioClip)EditorGUILayout.ObjectField(basicActions.hit.startSounds[i], typeof(AudioClip), false);
							
							// Delete hit sound
							if(GUILayout.Button ("X", GUILayout.Width (40)))
								basicActions.hit.startSounds = ArrayUtils.Remove<AudioClip>(basicActions.hit.startSounds, basicActions.hit.startSounds[i]);
						}
						EditorGUILayout.EndHorizontal ();
					}
					
					// Add start sound ("+") button
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField ("");
						// Add sound
						if(GUILayout.Button ("+", GUILayout.Width (40)))
							basicActions.hit.startSounds = ArrayUtils.Add<AudioClip>(basicActions.hit.startSounds,null);
					}
					EditorGUILayout.EndHorizontal ();
					
					// Display help box if multiple sounds provided
					if(basicActions.hit.startSounds.Length > 1)
						EditorGUILayout.HelpBox("One is chosen at random when the action is performed", MessageType.Info);
				}

			}
			EditorGUILayout.EndVertical ();
		}

		/**************************
		 * COMBAT ACTIONS FOLDOUT *
		 **************************/
		EditorGUI.indentLevel = 0;

		// "Attack Actions" foldout
		showCombatActionsFoldout = EditorGUILayout.Foldout(showCombatActionsFoldout,"Combat Actions (" + actionSet.combatActionScriptableObjects.Length + ")");

		if(showCombatActionsFoldout)
		{
			EditorGUILayout.BeginVertical ();
			{
				EditorGUI.indentLevel = 1;

				// Display each Action in the action set
				for(int i = 0; i < actionSet.combatActionScriptableObjects.Length; i++)
				{
					// Select an action 
					EditorGUILayout.BeginHorizontal ();
					{
						actionSet.combatActionScriptableObjects[i] = (ActionScriptableObject)EditorGUILayout.ObjectField (actionSet.combatActionScriptableObjects[i], 
						                                                                                  				  typeof(ActionScriptableObject), false);

						// Delete action from action set
						if(GUILayout.Button ("X", GUILayout.Width (40)))
							actionSet.combatActionScriptableObjects = ArrayUtils.Remove<ActionScriptableObject>(actionSet.combatActionScriptableObjects,
							                          	   					  									actionSet.combatActionScriptableObjects[i]);
					}
					EditorGUILayout.EndHorizontal ();
				}					

				// Add action ("+") button
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField ("");
					// Add animation string
					if(GUILayout.Button ("+", GUILayout.Width (40)))
						actionSet.combatActionScriptableObjects = ArrayUtils.Add<ActionScriptableObject>(actionSet.combatActionScriptableObjects, null);
				}
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndVertical ();	
		} // End "Attack Actions" foldout
	}

	public void OnDisable()
	{
		// Save the changes made to the action set
		ActionSet actionSet = (ActionSet) target;
		if(actionSet != null)
		{
			AssetDatabase.Refresh();
			EditorUtility.SetDirty(actionSet);
			AssetDatabase.SaveAssets();
		}
	}
}
