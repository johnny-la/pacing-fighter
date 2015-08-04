using UnityEngine;
using System.Collections;

namespace Brawler
{
	/// <summary>
	/// An event to perform once a behaviour is performed.
	/// </summary>
	[System.Serializable]
	public class Event
	{
		/// <summary>
		/// Specifies what will type of behaviour will be performed when the event is fired
		/// </summary>
		public EventType type;

		/// <summary>
		/// The action to perform once this event is fired. Only pertinent if 'type == EventType.PerformAction'
		/// </summary>
		public ActionScriptableObject actionToPerform;

		/// <summary>
		/// The basic action to perform once this event is fired. Only used if 'type == EventType.PerformBasicAction'
		/// </summary>
		public BasicActionType basicActionToPerform;

		/// <summary>
		/// The sound effect to play when the event is triggered (assuming the event type is set to 'EventType.SoundEffect').
		/// </summary>
		public AudioClip soundEffect;

		/// <summary>
		/// A camera movement activated when this event is triggered.
		/// </summary>
		public CameraMovement cameraMovement = new CameraMovement();

		/// <summary>
		/// The slow motion to activate if this event's type is set to EventType.SlowMotion		
		/// </summary>
		public SlowMotion slowMotion = new SlowMotion();

		/// <summary>
		/// The particle effect to play when the event is triggered.
		/// </summary>
		public ParticleEvent particleEvent = new ParticleEvent();

		/// <summary>
		/// The time at which the event activates.
		/// </summary>
		public CastingTime startTime = new CastingTime();
		
		/// <summary>
		/// The amount of time for which the event is activated (may be irrelevant for events such as 'PerformAction')
		/// </summary>
		public CastingTime duration = new CastingTime();

		public Event()
		{
		}

		// Creates an Event with the same values as the given 
		public Event(Event other)
		{
			type = other.type;
			actionToPerform = other.actionToPerform;
			basicActionToPerform = other.basicActionToPerform;
			soundEffect = other.soundEffect;

			cameraMovement = new CameraMovement(other.cameraMovement);
			slowMotion = other.slowMotion;
			particleEvent = other.particleEvent;

			startTime = other.startTime;
			duration = other.duration;
		}
	}

	/// <summary>
	/// Specifies what will happen when the event is performed
	/// </summary>
	public enum EventType
	{
		None,
		PerformAction,
		PerformBasicAction,
		SoundEffect,
		SlowMotion,
		ParticleEffect,
		CameraMovement,
		Die
	}
}

/// <summary>
/// A camera movement triggered by an action's event
/// </summary>
[System.Serializable]
public class CameraMovement
{
	/// <summary>
	/// The target position the camera will move towards.
	/// </summary>
	public TargetPosition target;
	
	/// <summary>
	/// The Transform the camera will try to follow. Used if 'targetPosition == Self'
	/// </summary>
	[HideInInspector]
	public Transform targetTransform;
	
	/// <summary>
	/// The position the camera will move towards. Used if the camera must follow a static, non-moving position.
	/// </summary>
	public Vector2 targetPosition;
	
	/// <summary>
	/// The target zoom of the camera.
	/// </summary>
	public float zoom = 1.0f;
	
	/// <summary>
	/// The speed at which the camera moves to its target position and zoom
	/// </summary>
	public float cameraSpeed = 1.0f;

	public CameraMovement() {}

	/// <summary>
	/// Create a deep copy of the given template
	/// </summary>
	public CameraMovement(CameraMovement template)
	{
		// Copies the values from the given template
		target = template.target;
		targetTransform = template.targetTransform;
		targetPosition = template.targetPosition;
		zoom = template.zoom;
	}
}

/// <summary>
/// A slow motion event that can be triggered from an action
/// </summary>
[System.Serializable]
public class SlowMotion
{
	/// <summary>
	/// The time scale to set the game at when slow motion is active. The lower the number, the slower the speed
	/// </summary>
	public float timeScale = 0.5f;
}

[System.Serializable]
public class ParticleEvent
{
	/// <summary>
	/// The particle effect that is played when the event is triggered.
	/// </summary>
	public ParticleEffect effect;
	
	/// <summary>
	/// The point at which the particles spawn
	/// </summary>
	public ParticleSpawnPoint spawnPoint;
	
	/// <summary>
	/// A position offset for the particles, relative to the spawning point. If the spawn
	/// point is set to 'Self', the offset is relative to the entity's facing direction.
	/// That is, if offset = (1,2,0), and the entity is facing left, the x-component is 
	/// flipped directions, and the offset is set to (-1,2,0)
	/// </summary>
	public Vector3 offset;
	
}

/// <summary>
/// The location where a particle effect is spawned 
/// </summary>
public enum ParticleSpawnPoint
{
	Self
}