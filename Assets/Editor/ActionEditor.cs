using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ActionScriptableObject))]
public class ActionEditor : Editor 
{
	private bool showAnimationFoldout = true; // If true, animation foldout is rolled open
	private bool showInputFoldout = true;
	private bool showHitBoxFoldout = true;
	private bool showForcesFoldout = true;
	private bool showLinkableActionsFoldout = false;
	private bool showSoundsFoldout = true;
	private bool showOptionsFoldout = true;

	// Events foldout
	private EventsFoldout onStartEventsFoldout;

	// Sound foldouts
	private bool showStartSoundsFoldout = false;
	private bool showImpactSoundsFoldout = false;

	// Force foldouts
	private bool[] showStartTimeFoldouts;
	private bool[] showDurationFoldouts;
	private bool[] showOnCompleteEventFoldouts;

	private bool[] showHitOptionsFoldouts;
	private GameObject[] templateHitBoxes; // If provided, template GameObjects provide properties for each hit box
	private EventsFoldout[] hitSelfEventsFoldouts;
	private EventsFoldout[] hitAdversaryEventsFoldouts;


	public void OnEnable()
	{
		// The actionScriptableObject instance being edited by this inspector
		ActionScriptableObject actionScriptableObject = (ActionScriptableObject) target;
		// Retrieves the action instance from the scriptable object being edited. This is a data container for the action's properties
		Action actionInfo = actionScriptableObject.action;

		// The foldout for the events performed at the same time this action is performed
		onStartEventsFoldout = new EventsFoldout("Events On Start", actionInfo.onStartEvents);

		showHitOptionsFoldouts = new bool[actionInfo.hitBoxes.Length];
		// The GameObjects which serve as templates for the hit boxes. 
		// One is supplied for each hit box in this action. If a Gameobject
		// is dragged and dropped, its values are copactionInfoEditoried to the hit box
		templateHitBoxes = new GameObject[actionInfo.hitBoxes.Length];
		// The foldouts which edit the events which happen on collision for each hit box.
		hitSelfEventsFoldouts = new EventsFoldout[actionInfo.hitBoxes.Length];
		hitAdversaryEventsFoldouts = new EventsFoldout[actionInfo.hitBoxes.Length];
		for(int i = 0; i < actionInfo.hitBoxes.Length; i++)
		{
			hitSelfEventsFoldouts[i] = new EventsFoldout("Events [Self]", actionInfo.hitBoxes[i].hitInfo.selfEvents);
			hitAdversaryEventsFoldouts[i] = new EventsFoldout("Events [Adversary]", actionInfo.hitBoxes[i].hitInfo.adversaryEvents);
		}


		// "Forces" foldouts
		showStartTimeFoldouts = new bool[actionInfo.forces.Length];
		showDurationFoldouts = new bool[actionInfo.forces.Length];
		showOnCompleteEventFoldouts = new bool[actionInfo.forces.Length];
	}

	public void OnDisable()
	{
		// Save the changes made to this action
		ActionScriptableObject actionScriptableObject = (ActionScriptableObject) target;
		AssetDatabase.Refresh();
		EditorUtility.SetDirty(actionScriptableObject);
		AssetDatabase.SaveAssets();
	}

	public override void OnInspectorGUI()
	{
		// The actionScriptableObject instance being edited by this inspector
		ActionScriptableObject actionScriptableObject = (ActionScriptableObject) target;
		// Retrieves the action instance from the scriptable object being edited. This is a data container for the action's properties
		Action actionInfo = actionScriptableObject.action;

		// Add undo support to the actionInfo instance.
		Undo.RecordObject (actionScriptableObject, "Action Info Undo");

		EditorGUILayout.BeginVertical ();
		{
			actionInfo.name = EditorGUILayout.TextField ("Name:", actionInfo.name);

			/*********************
			 * ANIMATION FOLDOUT *
			**********************/
			EditorGUI.indentLevel = 0;
			showAnimationFoldout = AnimationFoldout(actionInfo, showAnimationFoldout);

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
					// Can the action be activated by user input?
					EditorGUILayout.BeginHorizontal ();
					{
						EditorGUILayout.LabelField ("Activate using input?", GUILayout.Width(150));
						actionInfo.listensToInput = EditorGUILayout.Toggle ("", actionInfo.listensToInput);
					}
					EditorGUILayout.EndHorizontal ();


					// Only show input options if the action requires user input
					if(actionInfo.listensToInput)
					{
						actionInfo.inputType = (InputType)EditorGUILayout.EnumPopup ("Input Type:", actionInfo.inputType);
						if(actionInfo.inputType == InputType.Swipe)
							actionInfo.swipeDirection = (SwipeDirection)EditorGUILayout.EnumPopup ("Swipe Direction:", 
							                                                                     actionInfo.swipeDirection);
						actionInfo.inputRegion = (InputRegion)EditorGUILayout.EnumPopup ("Input Region:", actionInfo.inputRegion);
					}

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

						// Select a hit box type
						hitBox.hitBoxType = (HitBoxType)EditorGUILayout.EnumPopup ("Hit box type:", hitBox.hitBoxType);

						// If the hit box is a standard box collider attached to a bone
						if(hitBox.hitBoxType == HitBoxType.Standard)
						{
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
						}
						// Else, if the hit box automatically hits its target at a specified time
						else if(hitBox.hitBoxType == HitBoxType.ForceHit)
						{
							EditorGUILayout.LabelField ("Frame Select:");

							EditorGUI.indentLevel = 2;

							// Cycle through each hit frame, where the j-th hit frame corresponds to the j-th animation sequence
							for(int j = 0; j < hitBox.hitFrames.Length; j++)
							{
								EditorGUILayout.BeginHorizontal ();
								{
									// Choose the frame where the hit box hits its target for the j-th animation sequence
									EditorGUILayout.LabelField ("Animation sequence " + j + ", frame:");
									hitBox.hitFrames[j] = EditorGUILayout.IntField ("", hitBox.hitFrames[j], GUILayout.Width (100));
								}
								EditorGUILayout.EndHorizontal ();
							}
						}

						EditorGUI.indentLevel = 1;

						// "Hit Options" foldout
						showHitOptionsFoldouts[i] = EditorGUILayout.Foldout (showHitOptionsFoldouts[i], "Hit Options");

						if(showHitOptionsFoldouts[i])
						{
							// Change what happens when a hit box hits an opposing hit box
							hitBox.hitInfo.baseDamage = EditorGUILayout.FloatField ("Base Damage:", hitBox.hitInfo.baseDamage);
							// hitBox.hitInfo.hitStrength = (HitStrength)EditorGUILayout.EnumPopup ("Strength:", hitBox.hitInfo.hitStrength);
							hitBox.hitInfo.knockbackVelocity = EditorGUILayout.Vector2Field ("Knockback velocity:", hitBox.hitInfo.knockbackVelocity);
							hitBox.hitInfo.knockbackTime = EditorGUILayout.FloatField ("Knockback time:", hitBox.hitInfo.knockbackTime);

							hitBox.hitInfo.selfEvents = hitSelfEventsFoldouts[i].Display ();
							hitBox.hitInfo.adversaryEvents = hitAdversaryEventsFoldouts[i].Display ();
						}

						// Delete button
						EditorGUILayout.BeginHorizontal ();
						{
							// Left padding
							EditorGUILayout.LabelField ("");

							// Delete hit box
							if(GUILayout.Button ("Delete", GUILayout.Width (60)))
							{
								actionInfo.hitBoxes = ArrayUtils.Remove<HitBox>(actionInfo.hitBoxes, hitBox);
								showHitOptionsFoldouts = ArrayUtils.RemoveAt(showHitOptionsFoldouts, i);
								templateHitBoxes = ArrayUtils.Remove<GameObject>(templateHitBoxes, templateHitBoxes[i]);
								hitSelfEventsFoldouts = ArrayUtils.RemoveAt (hitSelfEventsFoldouts, i);
								hitAdversaryEventsFoldouts = ArrayUtils.RemoveAt (hitAdversaryEventsFoldouts, i);

							}
						}
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.Space ();
					
					}

					// "New Hit Box" Button
					if(GUILayout.Button ("New Hit Box"))
					{
						HitBox newHitBox = new HitBox();
						actionInfo.hitBoxes = ArrayUtils.Add<HitBox>(actionInfo.hitBoxes, newHitBox);
						// Create the 'hitFrames' array to match the number of animation sequences for the action
						actionInfo.hitBoxes[actionInfo.hitBoxes.Length-1].hitFrames = new int[actionInfo.animationSequences.Length];
						showHitOptionsFoldouts = ArrayUtils.Add<bool>(showHitOptionsFoldouts, false);
						templateHitBoxes = ArrayUtils.Add<GameObject>(templateHitBoxes, null);
						hitSelfEventsFoldouts = ArrayUtils.Add<EventsFoldout> (hitSelfEventsFoldouts, new EventsFoldout("Events [Self]", newHitBox.hitInfo.selfEvents));
						hitAdversaryEventsFoldouts = ArrayUtils.Add<EventsFoldout> (hitAdversaryEventsFoldouts, new EventsFoldout("Events [Adversary]", newHitBox.hitInfo.adversaryEvents));
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
							if(force.target == TargetPosition.CustomPosition)
								force.customTargetPosition = EditorGUILayout.Vector2Field ("Custom Position:", force.customTargetPosition);
							force.faceTarget = EditorGUILayout.Toggle("Face target?", force.faceTarget);
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

							//Define the duration of the force
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

						EditorGUI.indentLevel = 1;

						// "On Complete" foldout
						showOnCompleteEventFoldouts[i] = EditorGUILayout.Foldout (showOnCompleteEventFoldouts[i],"On Complete");

						if(showOnCompleteEventFoldouts[i])
						{
							// Select what to perform when the force is done being applied
							force.onCompleteEvent.type = (Brawler.EventType)EditorGUILayout.EnumPopup ("Event type:", force.onCompleteEvent.type);

							if(force.onCompleteEvent.type == Brawler.EventType.PerformAction)
							{
								// Select an action
								force.onCompleteEvent.actionToPerform = (ActionScriptableObject)EditorGUILayout.ObjectField ("Action:",
								                                                                                             force.onCompleteEvent.actionToPerform, 
								                                                                                             typeof(ActionScriptableObject),false);
							}
							else if(force.onCompleteEvent.type == Brawler.EventType.PerformBasicAction)
							{
								// Select a basic action
								force.onCompleteEvent.basicActionToPerform = (BasicActionType) EditorGUILayout.EnumPopup ("Basic action:",
								                                                                                      force.onCompleteEvent.basicActionToPerform);
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
								showStartTimeFoldouts = ArrayUtils.RemoveAt(showStartTimeFoldouts, i);
								showDurationFoldouts = ArrayUtils.RemoveAt(showDurationFoldouts, i);
								showOnCompleteEventFoldouts = ArrayUtils.RemoveAt(showOnCompleteEventFoldouts, i);
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
						showOnCompleteEventFoldouts = ArrayUtils.Add<bool>(showOnCompleteEventFoldouts, false);
					}
				}
				EditorGUILayout.EndVertical ();
			} // End "Forces" foldout

			/*********************************
			 * LINKABLE COMBAT MOVES FOLDOUT *
			 *********************************/

			EditorGUI.indentLevel = 0;

			showLinkableActionsFoldout = EditorGUILayout.Foldout(showLinkableActionsFoldout, "Linkable Combat Actions (" + 
			                                                     actionInfo.linkableCombatActionScriptableObjects.Length + ")");

			if(showLinkableActionsFoldout)
			{
				EditorGUI.indentLevel = 1;

				for(int i = 0; i < actionInfo.linkableCombatActionScriptableObjects.Length; i++)
				{
					EditorGUILayout.BeginHorizontal();
					{
						// Choose a linkable combat action
						actionInfo.linkableCombatActionScriptableObjects[i] = (ActionScriptableObject)EditorGUILayout.ObjectField(
							actionInfo.linkableCombatActionScriptableObjects[i],
							typeof(ActionScriptableObject),false);

						// Delete a linkable combat action
						if(GUILayout.Button("X", GUILayout.Width(40)))
						{
							actionInfo.linkableCombatActionScriptableObjects = ArrayUtils.RemoveAt(
								actionInfo.linkableCombatActionScriptableObjects,i);
						}
					}
					EditorGUILayout.EndHorizontal();
				}

				// New linkable combat action ("+") button
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("");

					if(GUILayout.Button("+", GUILayout.Width(40)))
					{
						actionInfo.linkableCombatActionScriptableObjects = ArrayUtils.Add<ActionScriptableObject>(
							actionInfo.linkableCombatActionScriptableObjects,null);
					}
				}
				EditorGUILayout.EndHorizontal();


				EditorGUILayout.HelpBox("The combat actions which can cancel this action and be performed instead. Useful for creating combos. " +
										"(Note that any combat action can be performed after this action. However, if a move is in this list, " +
				                        "it has higher priority, and will be chosen first over another one which satisfies the same input.", 
				                        MessageType.Info);
			}

			/***************************
			 * EVENTS ON START FOLDOUT *
			 **************************/
			EditorGUI.indentLevel = 0;

			actionInfo.onStartEvents = onStartEventsFoldout.Display ();

			/******************
			 * SOUNDS FOLDOUT *
			 ******************/
			
			EditorGUI.indentLevel = 0;
			
			showSoundsFoldout = EditorGUILayout.Foldout (showSoundsFoldout, "Sounds");
			
			if(showSoundsFoldout)
			{
				EditorGUILayout.BeginVertical ();
				{
					/** Start sounds foldout */
					EditorGUI.indentLevel = 1;

					showStartSoundsFoldout = EditorGUILayout.Foldout (showStartSoundsFoldout, "On Start (" + actionInfo.startSounds.Length + ")");

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
							EditorGUILayout.HelpBox("One sound is chosen at random when the action is performed", MessageType.Info);
					}

					/** Impact sounds foldout */
					EditorGUI.indentLevel = 1;
					
					showImpactSoundsFoldout = EditorGUILayout.Foldout (showImpactSoundsFoldout, "On Impact (" + actionInfo.impactSounds.Length + ")");
					
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
							EditorGUILayout.HelpBox("One sound is chosen at random when the action is performed", MessageType.Info);
					}
					
				}
				EditorGUILayout.EndVertical ();
			} // End "Hit Box" foldout

			/*******************
			 * OPTIONS FOLDOUT *
			 *******************/
			
			EditorGUI.indentLevel = 0;
			
			showOptionsFoldout = EditorGUILayout.Foldout (showHitBoxFoldout, "Options");
			
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

	/// <summary>
	/// Displays a simple animation foldout for the action. The action is only allowed to have one animation sequence
	/// </summary>
	/// <returns> true if the animation foldout is rolled open. False if it is closed </returns>
	public static bool SimpleAnimationFoldout(Action action, bool showFoldout)
	{
		// Display an "Animation" foldout
		showFoldout = EditorGUILayout.Foldout (showFoldout, "Animation");
		
		if(showFoldout)
		{
			EditorGUILayout.BeginVertical ();
			{
				EditorGUI.indentLevel++;

				AnimationSequence animationSequence = action.animationSequences[0];
				
				// Edit individual animations from animation sequence
				for(int i = 0; i < animationSequence.animations.Length; i++)
				{
					// Animation text field
					EditorGUILayout.BeginHorizontal ();
					{
						// Choose animation
						EditorGUILayout.LabelField (i + ":", GUILayout.Width (30));
						animationSequence.animations[i] = EditorGUILayout.TextField (animationSequence.animations[i]);

						// If there is more than one animation in the sequence, place a delete button for the current animation.
						// Ensures that the user can't delete an animation if there is only one in the sequence.
						if(animationSequence.animations.Length > 1)
						{
							// Delete animation string from animation sequence
							if(GUILayout.Button ("X", GUILayout.Width (40)))
							{
								animationSequence.animations = ArrayUtils.Remove<string>(animationSequence.animations,
								                                                         animationSequence.animations[i]);;
							}
						}
					}
					EditorGUILayout.EndHorizontal ();
				}
				
				// Add animation ("+") button
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField ("");
					// Add animation string
					if(GUILayout.Button ("+", GUILayout.Width (40)))
					{
						animationSequence.animations = ArrayUtils.Add<string>(animationSequence.animations,"");
					}
				}
				EditorGUILayout.EndHorizontal ();
				
				// Loop last animation?
				/*EditorGUILayout.BeginHorizontal ();
				{
					EditorGUILayout.LabelField ("Loop last animation?", GUILayout.Width (150));
					animationSequence.loopLastAnimation = EditorGUILayout.Toggle (animationSequence.loopLastAnimation);
				}
				EditorGUILayout.EndHorizontal ();*/
				
				EditorGUILayout.Space ();

				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical (); // End animation foldout
		} // End animation foldout
		
		return showFoldout;
	}

	/// <summary>
	/// Diplays an animation sequence foldout for the given action
	/// </summary>
	/// <returns> true if the animation foldout is rolled open. False if it is closed </returns>
	public static bool AnimationFoldout(Action action, bool showFoldout)
	{
		// Display an "Animation" foldout
		showFoldout = EditorGUILayout.Foldout (showFoldout, "Animation");
		
		if(showFoldout)
		{
			EditorGUILayout.BeginVertical ();
			{
				// Display each AnimationSequence
				for(int i = 0; i < action.animationSequences.Length; i++)
				{
					AnimationSequence animationSequence = action.animationSequences[i];
					EditorGUI.indentLevel = 1;
					
					// "Animation Sequence" header
					EditorGUILayout.BeginHorizontal ();
					{
						EditorGUILayout.LabelField ("Animation Sequence " + i);
						
						// Delete animation sequence
						if(GUILayout.Button ("Delete", GUILayout.Width (60)))
						{
							action.animationSequences = ArrayUtils.Remove<AnimationSequence>(action.animationSequences,
							                                                                     animationSequence);
							
							// Delete a hit frame from each hit box
							for(int j = 0; j < action.hitBoxes.Length; j++)
								action.hitBoxes[j].hitFrames = ArrayUtils.RemoveAt<int>(action.hitBoxes[j].hitFrames, j);
						}
					}
					EditorGUILayout.EndHorizontal ();
					
					// Edit individual animations from animation sequence
					for(int j = 0; j < animationSequence.animations.Length; j++)
					{
						// Animation text field
						EditorGUILayout.BeginHorizontal ();
						{
							// Choose animation
							EditorGUILayout.LabelField (j + ":", GUILayout.Width (30));
							animationSequence.animations[j] = EditorGUILayout.TextField (animationSequence.animations[j]);
							
							// Delete animation string from animation sequence
							if(GUILayout.Button ("X", GUILayout.Width (40)))
							{
								animationSequence.animations = ArrayUtils.Remove<string>(animationSequence.animations,
								                                                         animationSequence.animations[j]);;
							}
						}
						EditorGUILayout.EndHorizontal ();
					}
					
					// Add animation ("+") button
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField ("");
						// Add animation string
						if(GUILayout.Button ("+", GUILayout.Width (40)))
						{
							animationSequence.animations = ArrayUtils.Add<string>(animationSequence.animations,"");
						}
					}
					EditorGUILayout.EndHorizontal ();
					
					// Loop last animation?
					EditorGUILayout.BeginHorizontal ();
					{
						EditorGUILayout.LabelField ("Loop last animation?", GUILayout.Width (150));
						animationSequence.loopLastAnimation = EditorGUILayout.Toggle (animationSequence.loopLastAnimation);
					}
					EditorGUILayout.EndHorizontal ();
					
					EditorGUILayout.Space ();
				}
				
				// "New Animation Sequence" button
				if(GUILayout.Button ("New Animation Sequence"))
				{
					action.animationSequences = ArrayUtils.Add<AnimationSequence>(action.animationSequences,
					                                                                  new AnimationSequence());
					
					// Add a hit frame for each hit box, corresponding to the time the hit box activates for the new animation sequence
					for(int j = 0; j < action.hitBoxes.Length; j++)
					{
						action.hitBoxes[j].hitFrames = ArrayUtils.Add<int>(action.hitBoxes[j].hitFrames, 0);
					}
				}
			}
			EditorGUILayout.EndVertical (); // End animation foldout
		} // End animation foldout

		return showFoldout;
	}

	/** Displays the CONTENTS of a 'startTime' foldout. A starting time is used to specify when a certain force or effect
	 * begins. It is an instance of CastingTime */
	public static void StartTimeFoldout(CastingTime startTime)
	{
		EditorGUI.indentLevel++;
		
		// Specify the starting time of this force
		EditorGUILayout.BeginHorizontal ();
		{
			startTime.type = (DurationType)EditorGUILayout.EnumPopup ("Start at:", startTime.type);
			switch(startTime.type)
			{
			case DurationType.WaitForAnimationComplete:
				startTime.animationToWaitFor = EditorGUILayout.IntField ("", startTime.animationToWaitFor, GUILayout.Width (80));
				break;
			case DurationType.Frame:
				startTime.nFrames = EditorGUILayout.IntField ("", startTime.nFrames, GUILayout.Width (80));
				break;
			}
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUI.indentLevel--;
	}

	/** Displays the CONTENTS of a 'duration' foldout. A starting time is used to specify how long a certain force or effect
	 * lasts. It is an instance of CastingTime */
	public static void DurationFoldout(CastingTime duration)
	{
		EditorGUI.indentLevel++;
		
		//Define the duration of the force
		duration.type = (DurationType)EditorGUILayout.EnumPopup ("Duration type:", duration.type);
		
		switch(duration.type)
		{
		case DurationType.WaitForAnimationComplete:
			duration.animationToWaitFor = EditorGUILayout.IntField ("Stop at animation:", duration.animationToWaitFor);
			break;
		case DurationType.Frame:
			duration.nFrames = EditorGUILayout.IntField ("Number of frames:", duration.nFrames);
			break;
		}
		
		EditorGUI.indentLevel--;
	}

}