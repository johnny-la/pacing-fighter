using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MoveSet))]
public class MoveSetEditor : Editor 
{
	private bool showAttackMoveFoldout = true;

	public override void OnInspectorGUI()
	{
		// The MoveSet instance being edited by this inspector
		MoveSet moveSet = (MoveSet) target;

		// Add undo support to the MoveSet instance.
		Undo.RecordObject (moveSet, "Move Set Undo");

		// "Attack Moves" foldout
		showAttackMoveFoldout = EditorGUILayout.Foldout(showAttackMoveFoldout,"Attack Moves (" + moveSet.attackMoves.Length + ")");

		if(showAttackMoveFoldout)
		{
			EditorGUILayout.BeginVertical ();
			{
				EditorGUI.indentLevel = 1;

				// Display each Move in the move set
				for(int i = 0; i < moveSet.attackMoves.Length; i++)
				{
					// Select a move 
					EditorGUILayout.BeginHorizontal ();
					{
						moveSet.attackMoves[i] = (MoveInfo)EditorGUILayout.ObjectField (moveSet.attackMoves[i], typeof(MoveInfo), false);

						// Delete move from move set
						if(GUILayout.Button ("X", GUILayout.Width (40)))
							moveSet.attackMoves = ArrayUtils.Remove<MoveInfo>(moveSet.attackMoves,
							                          	   					  moveSet.attackMoves[i]);
					}
					EditorGUILayout.EndHorizontal ();
				}					

				// Add move ("+") button
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField ("");
					// Add animation string
					if(GUILayout.Button ("+", GUILayout.Width (40)))
						moveSet.attackMoves = ArrayUtils.Add<MoveInfo>(moveSet.attackMoves, null);
				}
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndVertical ();	
		} // End "Attack Moves" foldout
	}
}
