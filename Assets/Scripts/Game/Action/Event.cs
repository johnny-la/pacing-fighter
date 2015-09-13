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
		/// The force to apply on the character which activated  this event.
		/// </summary>
		public Force forceEvent = new Force(false);

		/// <summary>
		/// Flashes the character which activated this event a certain color.
		/// </summary>
		public ColorFlash colorFlash = new ColorFlash();
		/// <summary>
		/// Creates a ghosting effect behind the character which activated this event.
		/// </summary>
		public GhostEffect ghostEffect = new GhostEffect();

		/// <summary>
		/// Shakes the screen when this event is triggered.
		/// </summary>
		public ScreenShake screenShake = new ScreenShake();

		/// <summary>
		/// A tweening event performed on the GameObject which triggered this event.
		/// </summary>
		public TweenEvent tweenEvent = new TweenEvent();

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

		// Creates an Event with the same properties as the given event
		public Event(Event other)
		{
			type = other.type;
			actionToPerform = other.actionToPerform;
			basicActionToPerform = other.basicActionToPerform;
			soundEffect = other.soundEffect;

			cameraMovement = new CameraMovement(other.cameraMovement);
			slowMotion = other.slowMotion;
			particleEvent = other.particleEvent;
			forceEvent = new Force(other.forceEvent, false);
			colorFlash = other.colorFlash;
			ghostEffect = other.ghostEffect;
			screenShake = other.screenShake;
			tweenEvent = other.tweenEvent;

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
		Force,
		ColorFlash,
		FreezeAnimation,
		ScreenShake,
		Tween,
		Ghosting,
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

/// <summary>
/// An event which applies a force on the entity which triggered this event.
/// </summary>
[System.Serializable]
public class ForceEvent
{
	/// <summary>
	/// Determines whether the force is specified by a velocity or a target position
	/// </summary>
	public ForceType forceType;
	
	/// <summary>
	/// The velocity at which the character moves. Only used if this force is specified by a velocity.
	/// </summary>
	public Vector2 velocity;

	/// <summary>
	/// If true, the velocity is applied in the direction the character is facing. That is a positive
	/// horizontal velocity will make the character move in the direction he is facing. 
	/// </summary>
	public bool relativeToFacingDirection;
	
	/// <summary>
	/// Specifies the type of target the character is trying to move towards. Used if 'forceType=Position'
	/// </summary>
	public TargetPosition target = TargetPosition.None;
	
	/// <summary>
	/// The target position that the force will move an entity to. Only used if 'target==TargetPosition.CustomPosition'
	/// </summary>
	public Vector2 customTargetPosition;
	
	/// <summary>
	/// If true, the entity performing this action will face towards his TargetPosition when this force is applied
	/// Note: Only applies when 'target != TargetPosition.None'
	/// </summary>
	public bool faceTarget = false;

	/// <summary>
	/// The knockback force applied on the character that triggered this event. Note that this is a helper instance, used
	/// to avoid instantiating a new 'Force' object every time this ForceEvent is applied. The constructor is passed a value
	/// of false, to avoid creating a 'Brawler.Event' instance inside the Force. If such an event was created, infinite recursion
	/// would occur. In fact, since Forces hold a reference to an Event, and Events hold a reference to a Force, creating an 
	/// Event in the Force instance would cause an infinite loop of object instantiation and would crash Unity.
	/// </summary>
	private Force appliedForce = new Force(false);

	/// <summary>
	/// A helper force which is applied on the entity that triggered this event. Note that this instance is used to avoid 
	/// instantiating a new force every time this force event is applied
	/// </summary>
	public Force AppliedForce
	{
		get { return appliedForce; }
	}
}

/// <summary>
/// Creates a ghosting effect on the character which activated this event
/// </summary>
[System.Serializable]
public class GhostEffect
{
	/// <summary>
	/// The color of the ghosts.
	/// </summary>
	public Color color;
	
	/// <summary>
	/// If true, the ghosts are rendered in front of the character. Otherwise, they are rendered in back.
	/// </summary>
	public bool renderInFront;
}

/// <summary>
/// Flashes the character which activated this event by the desired color
/// </summary>
[System.Serializable]
public class ColorFlash
{
	/// <summary>
	/// The color that the character flashes.
	/// </summary>
	public Color color;

	/// <summary>
	/// If true, the flash is rendered in front of the character. Otherwise, it is rendered in back.
	/// </summary>
	public bool renderInFront;
}

/// <summary>
/// Stores the properties for a screen shake
/// </summary>
[System.Serializable]
public class ScreenShake
{
	/// <summary>
	/// The speed at which the screen shakes.
	/// </summary>
	public float speed;

	/// <summary>
	/// The max distance travelled by the camera relative to its original position.
	/// </summary>
	public float magnitude;
}

/// <summary>
/// Performs a tween on the GameObject which activated this event.
/// </summary>
[System.Serializable]
public class TweenEvent
{
	/// <summary>
	/// Denotes the property modified by the tweening
	/// </summary>
	public TweenType type;

	/// <summary>
	/// The easing type used for each property.
	/// </summary>
	public LeanTweenType positionEasingType;
	public LeanTweenType scaleEasingType;
	public LeanTweenType rotationEasingType;

	/// <summary>
	/// The target position for the tween, if 'type == TweenType.Position'
	/// </summary>
	public Vector3 targetPosition;

	/// <summary>
	/// The target scale for the tween, if 'type == TweenType.Scale'
	/// </summary>
	public Vector3 targetScale = new Vector3(1,1,1);

	/// <summary>
	/// The target z-rotation for the tween (in degrees)
	/// </summary>
	[Range(0,360)]
	public float targetAngle;

	/// <summary>
	/// If true, the target position is relative to the character's facing direction. That is, if the character is facing right,
	/// +x will move the character to the right. If the character is facing left, a positive x-position will move him left.
	/// </summary>
	public bool positionRelativeToFacingDirection;
	/// <summary>
	/// If true, the target angle is relative to the character's facing direction. That is, if the character is facing right,
	/// positive will move the character to the clockwise. If the character is facing left, a positive angle will rotate him counter-clockwise.
	/// </summary>
	public bool angleRelativeToFacingDirection;

	public string ToString()
	{
		return "Type: " + type.ToString () + ", Target position: " + targetPosition + ", Target scale: " + targetScale + " Target angle: " + targetAngle;
	}
}