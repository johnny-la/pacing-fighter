using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Displays a foldout to edit an array of Brawler.Event instances.
/// For instance, this allows the action editor to easily edit the events 
/// which occur when an action is executed
/// </summary>
public class EventsFoldout
{
	/** The events edited by this foldout. */
	private Brawler.Event[] events;
	
	/** The name given to the foldout. */
	private string title = "Events";
	
	private bool showFoldout = false;
	private bool[] showStartTimeFoldouts; 
	private bool[] showDurationFoldouts; 
	
	public EventsFoldout(string title, Brawler.Event[] events)
	{
		this.title = title;
		this.events = events;
		
		showStartTimeFoldouts = new bool[events.Length];
		showDurationFoldouts = new bool[events.Length];
	}
	
	/** Displays a foldout of a list of events */
	public Brawler.Event[] Display()
	{
		showFoldout = EditorGUILayout.Foldout (showFoldout, title + " (" + events.Length + ")");
		
		if(showFoldout)
		{
			EditorGUILayout.BeginVertical ();
			{
				EditorGUI.indentLevel++;
				
				// Cycle through each event 
				for(int i = 0; i < events.Length; i++)
				{
					Brawler.Event e = events[i];
					
					EditorGUILayout.BeginHorizontal ();
					{
						e.type = (Brawler.EventType)EditorGUILayout.EnumPopup ("Type:", e.type);
						if(GUILayout.Button ("X", GUILayout.Width (40)))
						{
							this.events = ArrayUtils.RemoveAt<Brawler.Event> (this.events, i);
							showStartTimeFoldouts = ArrayUtils.RemoveAt<bool>(showStartTimeFoldouts, i);
							showDurationFoldouts = ArrayUtils.RemoveAt<bool>(showDurationFoldouts, i);
							
							continue;
						}
					}
					EditorGUILayout.EndHorizontal ();
					
					if(e.type == Brawler.EventType.PerformAction)
					{
						// Select an action
						e.actionToPerform = (ActionScriptableObject)EditorGUILayout.ObjectField ("Action:", e.actionToPerform, 
						                                                                         typeof(ActionScriptableObject),false);
					}
					else if(e.type == Brawler.EventType.PerformBasicAction)
					{
						// Select a basic action
						e.basicActionToPerform = (BasicActionType) EditorGUILayout.EnumPopup ("Basic action:", e.basicActionToPerform);
					}
					else if(e.type == Brawler.EventType.CameraMovement)
					{
						// Set the camera settings
						e.cameraMovement.target = (TargetPosition)EditorGUILayout.EnumPopup ("Target position:", e.cameraMovement.target);
						if(e.cameraMovement.target == TargetPosition.CustomPosition)
							e.cameraMovement.targetPosition = EditorGUILayout.Vector2Field ("Position to move to:", e.cameraMovement.targetPosition);
						e.cameraMovement.zoom = EditorGUILayout.FloatField ("Zoom:", e.cameraMovement.zoom);
						e.cameraMovement.cameraSpeed = EditorGUILayout.FloatField ("Camera speed:", e.cameraMovement.cameraSpeed);
						
					}
					else if(e.type == Brawler.EventType.SoundEffect)
					{
						// Select the sound effect to play when the event is triggered
						e.soundEffect = (AudioClip)EditorGUILayout.ObjectField ("Sound effect:", e.soundEffect, typeof(AudioClip), false);
					}
					else if(e.type == Brawler.EventType.SlowMotion)
					{				
						e.slowMotion.timeScale = EditorGUILayout.Slider ("Time scale:", e.slowMotion.timeScale, 0.001f, 1.0f);
					}
					else if(e.type == Brawler.EventType.ParticleEffect)
					{
						// Select a particle effect
						e.particleEvent.effect = (ParticleEffect)EditorGUILayout.EnumPopup ("Particle Effect:", e.particleEvent.effect);
						e.particleEvent.spawnPoint = (ParticleSpawnPoint)EditorGUILayout.EnumPopup ("Spawn Point:", e.particleEvent.spawnPoint);
						e.particleEvent.offset = EditorGUILayout.Vector3Field ("Offset:", e.particleEvent.offset);
					}
					else if(e.type == Brawler.EventType.Force)
					{
						Force force = e.forceEvent;
						
						// Select a force type
						force.forceType = (ForceType)EditorGUILayout.EnumPopup ("Force Type:", force.forceType);
						switch(force.forceType)
						{
						case ForceType.Velocity:
							force.velocity = EditorGUILayout.Vector2Field ("Velocity:", force.velocity);
							force.relativeToFacingDirection = EditorGUILayout.Toggle ("Relative to facing direction?", force.relativeToFacingDirection);
							break;
						case ForceType.Position:
							force.target = (TargetPosition)EditorGUILayout.EnumPopup ("Target Position:", force.target);
							if(force.target == TargetPosition.CustomPosition)
								force.customTargetPosition = EditorGUILayout.Vector2Field ("Custom Position:", force.customTargetPosition);
							force.faceTarget = EditorGUILayout.Toggle("Face target?", force.faceTarget);
							break;
						}
					}
					else if(e.type == Brawler.EventType.ColorFlash)
					{
						ColorFlash colorFlash = e.colorFlash;

						// Edit the color-flashing event
						colorFlash.color = EditorGUILayout.ColorField ("Color:", colorFlash.color);

						colorFlash.renderInFront = EditorGUILayout.Toggle ("Render in front", colorFlash.renderInFront);
					}
					else if(e.type == Brawler.EventType.ScreenShake)
					{
						ScreenShake screenShake = e.screenShake;

						// Modify the screen shake settings
						screenShake.speed = EditorGUILayout.FloatField ("Speed:", screenShake.speed);
						screenShake.magnitude = EditorGUILayout.FloatField ("Magnitude:", screenShake.magnitude);
					}
					else if(e.type == Brawler.EventType.Tween)
					{
						TweenEvent tweenEvent = e.tweenEvent;

						TweenEventEditor(tweenEvent);
					}
					
					// Stores true if the event being edited requires a starting time to be specified
					bool editStartTime = (e.type != Brawler.EventType.None);
					
					// If we require to edit the duration
					if(editStartTime)
					{
						// "Starting Time" foldout
						showStartTimeFoldouts[i] = EditorGUILayout.Foldout (showStartTimeFoldouts[i], "Starting Time");
						
						if(showStartTimeFoldouts[i])
						{
							ActionEditor.StartTimeFoldout(e.startTime);
						}
					}
					
					// Stores true if the event being edited requires a 'duration' to be specified
					bool editDuration = (e.type == Brawler.EventType.SlowMotion || e.type == Brawler.EventType.Force || e.type == Brawler.EventType.ColorFlash
					                     || e.type == Brawler.EventType.FreezeAnimation || e.type == Brawler.EventType.ScreenShake || e.type == Brawler.EventType.Tween);
					
					// If we require to edit the duration
					if(editDuration)
					{
						// "Duration" foldout
						showDurationFoldouts[i] = EditorGUILayout.Foldout (showDurationFoldouts[i], "Duration");
						
						if(showDurationFoldouts[i])
						{
							ActionEditor.DurationFoldout (e.duration);
						}
					}
					
					EditorGUILayout.Space ();
				}
				
				// Add event ("+") button
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField ("");
					// Add new event
					if(GUILayout.Button ("+", GUILayout.Width (40)))
					{
						this.events = ArrayUtils.Add<Brawler.Event>(this.events,new Brawler.Event());
						showStartTimeFoldouts = ArrayUtils.Add<bool>(showStartTimeFoldouts, false);
						showDurationFoldouts = ArrayUtils.Add<bool>(showDurationFoldouts, false);
					}
				}
				EditorGUILayout.EndHorizontal ();
				
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
		}
		
		// Return the modified array of events
		return events;
		
	}

	/// <summary>
	/// Displays an editor to modify a tweening event. 
	/// Note: This method mutates the tweenEvent.
	/// </summary>
	public static void TweenEventEditor(TweenEvent tweenEvent)
	{
		tweenEvent.type = (TweenType)EditorGUILayout.EnumMaskField ("Property:", tweenEvent.type);
		
		if((tweenEvent.type & TweenType.Position) == TweenType.Position)
		{
			tweenEvent.targetPosition = EditorGUILayout.Vector3Field ("Target Position", tweenEvent.targetPosition);
			tweenEvent.positionEasingType = (LeanTweenType)EditorGUILayout.EnumPopup ("Position easing Type:", tweenEvent.positionEasingType);
			tweenEvent.positionRelativeToFacingDirection = EditorGUILayout.Toggle ("Relative to facing direction", tweenEvent.positionRelativeToFacingDirection);
			
			EditorGUILayout.LabelField ("------------------------------------------------");
		}
		if((tweenEvent.type & TweenType.Scale) == TweenType.Scale)
		{
			tweenEvent.targetScale = EditorGUILayout.Vector3Field ("Target Scale", tweenEvent.targetScale);
			tweenEvent.scaleEasingType = (LeanTweenType)EditorGUILayout.EnumPopup ("Scale easing Type:", tweenEvent.scaleEasingType);
			
			EditorGUILayout.LabelField ("------------------------------------------------");
		}
		if((tweenEvent.type & TweenType.Rotation) == TweenType.Rotation)
		{
			tweenEvent.targetAngle = EditorGUILayout.Slider("Target Angle:", tweenEvent.targetAngle, 0.0f, 360.0f);
			tweenEvent.rotationEasingType = (LeanTweenType)EditorGUILayout.EnumPopup ("Rotation easing Type:", tweenEvent.rotationEasingType);
			tweenEvent.angleRelativeToFacingDirection = EditorGUILayout.Toggle ("Relative to facing direction", tweenEvent.angleRelativeToFacingDirection);
			
			if(tweenEvent.angleRelativeToFacingDirection)
				EditorGUILayout.HelpBox ("0 degrees = Upright. If Facing Right: positive angle turns clockwise. If Facing Left: positive angle turns counter-clockwise.",
				                         MessageType.Info);
			else
				EditorGUILayout.HelpBox ("0 degrees = Upright. Positive angle turns the GameObject clockwise.", MessageType.Info);
		}
	}
	
}
