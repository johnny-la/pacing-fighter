using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Action))]
public class ActionEditor : Editor 
{
	private bool showAnimationFoldout = true; // If true, animation foldout is rolled open
	private bool showInputFoldout = true;
	private bool showHitBoxFoldout = true;
	private bool showForcesFoldout = true;
	private bool showSoundFoldout = true;
	private bool showOptionsFoldout = true;

	// Sound foldouts
	private bool showStartSoundsFoldout = false;
	private bool showImpactSoundsFoldout = false;

	// Force foldouts
	private bool[] showStartTimeFoldouts;
	private bool[] showDurationFoldouts;

	
	private GameObject[] templateHitBoxes; // If provided, template GameObjects provide values for each hit box

	public void OnEnable()
	{
		// The actionInfo instance being edited by this inspector
		Action actionInfo = (Action) target;

		// The GameObjects which serve as templates for the hit boxes. 
		// One is supplied for each hit box in this action. If a Gameobject
		// is dragged and dropped, its values are copactionInfoEditoried to the hit box
		templateHitBoxes = new GameObject[actionInfo.hitBoxes.Length];

		// "Forces" foldouts
		showStartTimeFoldouts = new bool[actionInfo.forces.Length];
		showDurationFoldouts = new bool[actionInfo.forces.Length];
	}

	public override void OnInspectorGUI()
	{
		// The actionInfo instance being edited by this inspector
		Action actionInfo = (Action) target;

		// Add undo support to the actionInfo instance.
		Undo.RecordObject (target, "Action Info Undo");

		EditorGUILayout.BeginVertical ();
		{
			actionInfo.name = EditorGUILayout.TextField ("Name:", actionInfo.name);

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
					for(int i = 0; i < actionInfo.animationSequences.Length; i++)
					{
						AnimationSequence animationSequence = actionInfo.animationSequences[i];
						EditorGUI.indentLevel = 1;

						// "Animation Sequence" header
						EditorGUILayout.BeginHorizontal ();
						{
							EditorGUILayout.LabelField ("Animation Sequence " + i);
							
							// Delete animation sequence
							if(GUILayout.Button ("Delete", GUILayout.Width (60)))
								actionInfo.animationSequences = ArrayUtils.Remove<AnimationSequence>(actionInfo.animationSequences,
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
						actionInfo.animationSequences = ArrayUtils.Add<AnimationSequence>(actionInfo.animationSequences,
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
					actionInfo.inputType = (InputType)EditorGUILayout.EnumPopup ("Input Type:", actionInfo.inputType);
					if(actionInfo.inputType == InputType.Swipe)
						actionInfo.swipeDirection = (SwipeDirection)EditorGUILayout.EnumPopup ("Swipe Direction:", 
						                                                                     actionInfo.swipeDirection);
					actionInfo.inputRegion = (InputRegion)EditorGUILayout.EnumPopup ("Input Region:", actionInfo.inputRegion);

				}
				EditorGUILayout.EndVertical ();
			}

			/*******************
			 * HIT BOX FOLDOUT *
			 *******************/

			EditorGUI.indentLevel = 0;

			showHitBoxFoldout = EditorGUILayout.Foldout (showHitBoxFoldout, "Hit Boxes (" + actionInfo.hitBoxes.Length + ")");

			if(showHitBoxFoldout)
			{
				EditorGUILayout.BeginVertical ();
				{
					EditorGUI.indentLevel = 1;

					// Cycle through each hit box associated with this action
					for(int i = 0; i < actionInfo.hitBoxes.Length; i++)
					{
						HitBox hitBox = actionInfo.hitBoxes[i];

						// If a template GameObject is provided, copy its values to the hit box
						templateHitBoxes[i] = (GameObject)EditorGUILayout.ObjectField ("Copy Values From:", templateHitBoxes[i], typeof(GameObject), true);

						// If a template object is given, copy its values to the hit box data instance
						if(templateHitBoxes[i] != null)
						{
							// Copy the values from the template object
							BoneFollower boneFollowerTemplate = templateHitBoxes[i].GetComponent<BoneFollower>();
							hitBox.boneName = boneFollowerTemplate.boneName;

							BoxCollider2D colliderTemplate = templateHitBoxes[i].GetComponent<BoxCollider2D>();
							hitBox.offset = colliderTemplate.offset;
							hitBox.size = colliderTemplate.size;
						}

						// Edit HitBox properties
						hitBox.boneName = EditorGUILayout.TextField ("Bone Name:", hitBox.boneName);
						hitBox.offset = EditorGUILayout.Vector2Field("Offset:", hitBox.offset);
						hitBox.size.x = EditorGUILayout.FloatField ("Width:", hitBox.size.x);
						hitBox.size.y = EditorGUILayout.FloatField ("Height:", hitBox.size.y);

						// Delete button
						EditorGUILayout.BeginHorizontal ();
						{
							// Left padding
							EditorGUILayout.LabelField ("");

							// Delete hit box
							if(GUILayout.Button ("Delete", GUILayout.Width (60)))
							{
								actionInfo.hitBoxes = ArrayUtils.Remove<HitBox>(actionInfo.hitBoxes, hitBox);
								templateHitBoxes = ArrayUtils.Remove<GameObject>(templateHitBoxes, templateHitBoxes[i]);
							}
						}
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.Space ();
					
					}

					// "New Hit Box" Button
					if(GUILayout.Button ("New Hit Box"))
					{
					   actionInfo.hitBoxes = ArrayUtils.Add<HitBox>(actionInfo.hitBoxes, new HitBox());
					   templateHitBoxes = ArrayUtils.Add<GameObject>(templateHitBoxes, null);
					}

				}
				EditorGUILayout.EndVertical ();
			} // End "Hit Box" foldout

			/******************
			 * FORCES FOLDOUT *
			 ******************/
			
			EditorGUI.indentLevel = 0;
			
			showForcesFoldout = EditorGUILayout.Foldout (showForcesFoldout, "Forces (" + actionInfo.forces.Length + ")");
			
			if(showForcesFoldout)
			{
				EditorGUILayout.BeginVertical ();
				{
					EditorGUI.indentLevel = 1;

					// Display each force for the action
					for(int i = 0; i < actionInfo.forces.Length; i++)
					{
						Force force = actionInfo.forces[i];

						// Select a force type
						force.forceType = (ForceType)EditorGUILayout.EnumPopup ("Force Type:", force.forceType);
						switch(force.forceType)
						{
						case ForceType.Velocity:
							force.velocity = EditorGUILayout.Vector2Field ("Velocity:", force.velocity);
							break;
						case ForceType.Position:
							force.target = (TargetPosition)EditorGUILayout.EnumPopup ("Target Position:", force.target);
							break;
						}

						EditorGUI.indentLevel = 1;

						// "Starting Time" foldout
						showStartTimeFoldouts[i] = EditorGUILayout.Foldout (showStartTimeFoldouts[i], "Starting Time");

						if(showStartTimeFoldouts[i])
						{
							EditorGUI.indentLevel = 2;

							// Specify the starting time of this force
							EditorGUILayout.BeginHorizontal ();
							{
								force.startTime.type = (DurationType)EditorGUILayout.EnumPopup ("Start at:", force.startTime.type);
								switch(force.startTime.type)
								{
								case DurationType.WaitForAnimationComplete:
									force.startTime.animationToWaitFor = EditorGUILayout.IntField ("", force.startTime.animationToWaitFor, GUILayout.Width (80));
									break;
								case DurationType.Frame:
									force.startTime.nFrames = EditorGUILayout.IntField ("", force.startTime.nFrames, GUILayout.Width (80));
									break;
								}
							}
							EditorGUILayout.EndHorizontal();
						}

						EditorGUI.indentLevel = 1;

						// "Duration" foldout
						showDurationFoldouts[i] = EditorGUILayout.Foldout (showDurationFoldouts[i], "Duration");
						
						if(showDurationFoldouts[i])
						{
							EditorGUI.indentLevel = 2;

							force.duration.type = (DurationType)EditorGUILayout.EnumPopup ("Duration type:", force.duration.type);

							switch(force.duration.type)
							{
							case DurationType.WaitForAnimationComplete:
								force.duration.animationToWaitFor = EditorGUILayout.IntField ("Stop at animation:", force.duration.animationToWaitFor);
								break;
							case DurationType.Frame:
								force.duration.nFrames = EditorGUILayout.IntField ("Number of frames:", force.duration.nFrames);
								break;
							}
						}

						// Delete a force
						EditorGUILayout.BeginHorizontal ();
						{
							EditorGUILayout.LabelField ("");
							// Delete a force
							if(GUILayout.Button ("Delete", GUILayout.Width (60)))
							{
								actionInfo.forces = ArrayUtils.Remove<Force>(actionInfo.forces, force);
								
								// Update boolean arrays for starting time/duration foldouts
								showStartTimeFoldouts = ArrayUtils.Remove<bool>(showStartTimeFoldouts, showStartTimeFoldouts[i]);
								showDurationFoldouts = ArrayUtils.Remove<bool>(showDurationFoldouts, showDurationFoldouts[i]);
							}
						}
						EditorGUILayout.EndVertical ();

						EditorGUILayout.Space ();
					}	

					// "New force" button
					if(GUILayout.Button ("New Force"))
					{
						actionInfo.forces =  ArrayUtils.Add<Force>(actionInfo.forces, new Force());
						
						// Update boolean arrays for starting time/duration foldouts
						showStartTimeFoldouts = ArrayUtils.Add<bool>(showStartTimeFoldouts, false);
						showDurationFoldouts = ArrayUtils.Add<bool>(showDurationFoldouts, false);
					}
				}
				EditorGUILayout.EndVertical ();
			} // End "Forces" foldout

			/*****************
			 * SOUND FOLDOUT *
			 *****************/
			
			EditorGUI.indentLevel = 0;
			
			showHitBoxFoldout = EditorGUILayout.Foldout (showHitBoxFoldout, "Sound");
			
			if(showHitBoxFoldout)
			{
				EditorGUILayout.BeginVertical ();
				{
					/** Start sounds foldout */
					EditorGUI.indentLevel = 1;

					showStartSoundsFoldout = EditorGUILayout.Foldout (showStartSoundsFoldout, "On Start");

					if(showStartSoundsFoldout)
					{
						EditorGUI.indentLevel = 2;

						// Display each possible start sound (one is chosen at random when move is performed)
						for(int i = 0; i < actionInfo.startSounds.Length; i++)
						{
							// Animation text field
							EditorGUILayout.BeginHorizontal ();
							{
								actionInfo.startSounds[i] = (AudioClip)EditorGUILayout.ObjectField(actionInfo.startSounds[i], typeof(AudioClip), false);
								
								// Delete animation string from animation sequence
								if(GUILayout.Button ("X", GUILayout.Width (40)))
									actionInfo.startSounds = ArrayUtils.Remove<AudioClip>(actionInfo.startSounds, actionInfo.startSounds[i]);
							}
							EditorGUILayout.EndHorizontal ();
						}
						
						// Add start sound ("+") button
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField ("");
							// Add sound
							if(GUILayout.Button ("+", GUILayout.Width (40)))
								actionInfo.startSounds = ArrayUtils.Add<AudioClip>(actionInfo.startSounds,null);
						}
						EditorGUILayout.EndHorizontal ();

						// Display help box if multiple sounds provided
						if(actionInfo.startSounds.Length > 1)
							EditorGUILayout.HelpBox("One is chosen at random when the action is performed", MessageType.Info);
					}

					/** Impact sounds foldout */
					EditorGUI.indentLevel = 1;
					
					showImpactSoundsFoldout = EditorGUILayout.Foldout (showImpactSoundsFoldout, "On Impact");
					
					if(showImpactSoundsFoldout)
					{
						EditorGUI.indentLevel = 2;

						// Display each possible start sound (one is chosen at random when move is performed)
						for(int i = 0; i < actionInfo.impactSounds.Length; i++)
						{
							// Impact sound
							EditorGUILayout.BeginHorizontal ();
							{
								actionInfo.impactSounds[i] = (AudioClip)EditorGUILayout.ObjectField(actionInfo.impactSounds[i], typeof(AudioClip), false);
								
								// Delete animation string from animation sequence
								if(GUILayout.Button ("X", GUILayout.Width (40)))
									actionInfo.impactSounds = ArrayUtils.Remove<AudioClip>(actionInfo.impactSounds, actionInfo.impactSounds[i]);
							}
							EditorGUILayout.EndHorizontal ();
						}
						
						// Add impact sound ("+") button
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField ("");
							// Add new impact sound
							if(GUILayout.Button ("+", GUILayout.Width (40)))
								actionInfo.impactSounds = ArrayUtils.Add<AudioClip>(actionInfo.impactSounds,null);
						}
						EditorGUILayout.EndHorizontal ();

						// Display help box if multiple sounds provided
						if(actionInfo.impactSounds.Length > 1)
							EditorGUILayout.HelpBox("One is chosen at random when the action is performed", MessageType.Info);
					}
					
				}
				EditorGUILayout.EndVertical ();
			} // End "Hit Box" foldout

			/*******************
			 * OPTIONS FOLDOUT *
			 *******************/
			
			EditorGUI.indentLevel = 0;
			
			showHitBoxFoldout = EditorGUILayout.Foldout (showHitBoxFoldout, "Options");
			
			if(showOptionsFoldout)
			{
				EditorGUILayout.BeginVertical ();
				{
					EditorGUI.indentLevel = 1;
					
					// "Cancelable?" checkbox
					actionInfo.cancelable = EditorGUILayout.Toggle ("Cancelable?", actionInfo.cancelable);
					// "Can cancel any move?" checkbox
					actionInfo.overrideCancelable = EditorGUILayout.Toggle ("Can cancel current move?", actionInfo.overrideCancelable);
					
				}
				EditorGUILayout.EndVertical ();
			} // End "Options" foldout

		}
		EditorGUILayout.EndVertical ();	
	}
}
