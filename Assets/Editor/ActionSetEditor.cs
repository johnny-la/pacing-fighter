using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ActionSet))]
public class ActionSetEditor : Editor 
{
	private bool showBasicActionsFoldout = true;
	private bool showCombatActionsFoldout = true;

	private bool showHitSoundsFoldout = false;
	private bool showKnockbackSoundsFoldout = false;

	private BasicActionEditor[] basicActionEditors;

	public void OnEnable()
	{
		// Cache the ActionSet instance being edited by this inspector
		ActionSet actionSet = (ActionSet) target;

		// Creates a list containing all of the action set's basic actions
		actionSet.basicActions.actions = new BasicAction[] {
			actionSet.basicActions.idle,
			actionSet.basicActions.walk,
			actionSet.basicActions.hit,
			actionSet.basicActions.knockback,
			actionSet.basicActions.knockbackRise,
			actionSet.basicActions.death,
			actionSet.basicActions.deathKnockback,
			actionSet.basicActions.nullAction
		};
		
		// Inialize the default properties for each basic action
		actionSet.basicActions.Init ();

		// Create the editors for the basic actions
		basicActionEditors = new BasicActionEditor[actionSet.basicActions.actions.Length];

		for(int i = 0; i < actionSet.basicActions.actions.Length; i++)
		{
			basicActionEditors[i] = new BasicActionEditor(actionSet.basicActions.actions[i]);
		}
	}

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

				// Cycle through each basic action in the ActionSet
				for(int i = 0; i < basicActionEditors.Length; i++)
				{
					BasicActionEditor basicActionEditor = basicActionEditors[i];

					basicActionEditor.Display ();
				}
			}
			EditorGUILayout.EndHorizontal ();
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

				EditorGUILayout.HelpBox ("The combat actions at the top of the list have priority over the lower ones. "+
				                         "That is, if two actions are activated using the same input, the one higher on the " +
				                         "list will be chosen", MessageType.Info);
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

public class BasicActionEditor
{
	// The BasicAction instance being edited
	private BasicAction basicAction;

	private bool showFoldout = false;

	private bool showAnimationFoldout;
	private bool showSoundEffectsFoldout;

	private EventsFoldout onStartEventsFoldout;

	/// <summary>
	/// Creates a new editor for the given basic action
	/// </summary>
	public BasicActionEditor(BasicAction basicAction)
	{
		this.basicAction = basicAction;

		onStartEventsFoldout = new EventsFoldout("Events", basicAction.onStartEvents);
	}

	public void Display()
	{
		showFoldout = EditorGUILayout.Foldout (showFoldout, basicAction.type.ToString());

		if(showFoldout)
		{
			EditorGUI.indentLevel++;

			/***********************
			/*  Animation foldout  */
			/**********************/
			showAnimationFoldout = ActionEditor.SimpleAnimationFoldout (basicAction, showAnimationFoldout);

			/******************************
			/*  Events on start foldout  */
			/*****************************/
			basicAction.onStartEvents = onStartEventsFoldout.Display ();

			/**************************
			/*  Sound effects foldout */
			/**************************/
			showSoundEffectsFoldout = EditorGUILayout.Foldout (showSoundEffectsFoldout, "Sound Effects (" + basicAction.startSounds.Length + ")");
			
			if(showSoundEffectsFoldout)
			{
				EditorGUI.indentLevel++;
				
				// Display each possible hit sound (one is chosen at random when move is performed)
				for(int i = 0; i < basicAction.startSounds.Length; i++)
				{
					// Edit hit sound
					EditorGUILayout.BeginHorizontal ();
					{
						basicAction.startSounds[i] = (AudioClip)EditorGUILayout.ObjectField(basicAction.startSounds[i], typeof(AudioClip), false);
						
						// Delete hit sound
						if(GUILayout.Button ("X", GUILayout.Width (40)))
							basicAction.startSounds = ArrayUtils.Remove<AudioClip>(basicAction.startSounds, basicAction.startSounds[i]);
					}
					EditorGUILayout.EndHorizontal ();
				}
				
				// Add start sound ("+") button
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField ("");
					// Add sound
					if(GUILayout.Button ("+", GUILayout.Width (40)))
						basicAction.startSounds = ArrayUtils.Add<AudioClip>(basicAction.startSounds,null);
				}
				EditorGUILayout.EndHorizontal ();
				
				// Display help box if multiple sounds provided
				if(basicAction.startSounds.Length > 1)
					EditorGUILayout.HelpBox("One is chosen at random when the action is performed", MessageType.Info);

				EditorGUI.indentLevel--;
			}
		
			EditorGUI.indentLevel--;
		}
	}
}
