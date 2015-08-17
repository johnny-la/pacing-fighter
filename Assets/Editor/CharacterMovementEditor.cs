using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CharacterMovement))]
public class CharacterMovementEditor : Editor 
{
	private bool showPhysicsFoldout = true; // If true, show the physics foldout
	private bool showWalkSpeedFoldout = true;

	public override void OnInspectorGUI()
	{
		// Caches the scripts this inspector modifies
		CharacterMovement characterMovement = (CharacterMovement) target;
		PhysicsData defaultPhysicsData = characterMovement.DefaultPhysicsData;

		/*******************
		 * PHYSICS FOLDOUT *
		 *******************/
		EditorGUI.indentLevel = 0;

		// "Physics" foldout
		showPhysicsFoldout = EditorGUILayout.Foldout (showPhysicsFoldout, "Physics");

		if(showPhysicsFoldout)
		{
			EditorGUI.indentLevel = 1;

			// "Default Walk Speed" foldout
			showWalkSpeedFoldout = EditorGUILayout.Foldout (showWalkSpeedFoldout, "Default Walk Speed");
		
			if(showWalkSpeedFoldout)
			{
				// Physics foldout 
				EditorGUILayout.BeginVertical ();
				{
					// Floating-point fields for the character's walking speed
					defaultPhysicsData.MinWalkSpeed = EditorGUILayout.FloatField ("Min:",
					                                                              defaultPhysicsData.MinWalkSpeed);
					defaultPhysicsData.MaxWalkSpeed = EditorGUILayout.FloatField ("Max:",
					                                                              defaultPhysicsData.MaxWalkSpeed);
				}
				EditorGUILayout.EndVertical ();
			} // End "Default Walk Speed" foldout
		}

		Undo.RecordObject (characterMovement, "Character Movement Undo");
	}

}
