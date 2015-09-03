using UnityEngine;
using System.Collections;

/// <summary>
/// The type of force applied onto a character. Either he can move at a specified velocity,
/// or move towards a specified location when the force is applied
/// </summary>
public enum ForceType
{
	Velocity,
	Position // Move towards a specified target position
}

/// <summary>
/// A force applied on the character whilst performing a move 
/// </summary>
[System.Serializable]
public class Force
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
	/// The event to perform once the force is done being applied. For instance, if the event requires an action
	/// to be performed, the action is performed by the same entity that was affected by this force
	/// </summary>
	public Brawler.Event onCompleteEvent;
	
	/// <summary>
	/// If true, the entity performing this action will face towards his TargetPosition when this force is applied
	/// Note: Only applies when 'target != TargetPosition.None'
	/// </summary>
	public bool faceTarget = false;
	
	/// <summary>
	/// The time at which the force activates
	/// </summary>
	public CastingTime startTime = new CastingTime();
	
	/// <summary>
	/// The amount of time the force is active.
	/// </summary>
	public CastingTime duration = new CastingTime();
	
	public Force() 
	{
		// Create a new event which will be triggered when the force is done being applied
		onCompleteEvent = new Brawler.Event();
	}
	
	/// <summary>
	/// Creates a deep copy of the given force. Creates an OnComplete event inside the Force instance
	/// </summary>
	public Force(Force template) : this(template, true)
	{
	}
	
	/// <summary>
	/// Creates a new force instance.	
	/// </summary>
	/// <param name="createOnCompleteEvent">If set to <c>true</c> create an onComplete event. This is set to true
	/// by default. This parameter is set to false when the Force is created inside a ForceEvent instance. In this
	/// case, creating an 'onComplete' Event would cause infinite recursion. This is because each ForceEvent holds
	/// an instance of a Force, which creates a new 'Brawler.Event'. Then, the Brawler.Event creates a new ForceEvent,
	/// which then creates a new Force, and the recursion never stops. Therefore, if this Force is a member variable
	/// inside a ForceEvent instance, the onComplete event should not be created.
	/// </param>
	public Force(bool createOnCompleteEvent) 
	{
		if(createOnCompleteEvent)
			// Create a new event which will be triggered when the force is done being applied
			onCompleteEvent = new Brawler.Event();
		
	}
	
	/// <summary>
	/// Creates a new force instance given a template.
	/// </summary>
	/// <param name="createOnCompleteEvent">If set to <c>true</c> create an onComplete event. This is set to true
	/// by default. This parameter is set to false when the Force is created inside a ForceEvent instance. In this
	/// case, creating an 'onComplete' Event would cause infinite recursion. This is because each ForceEvent holds
	/// an instance of a Force, which creates a new 'Brawler.Event'. Then, the Brawler.Event creates a new ForceEvent,
	/// which then creates a new Force, and the recursion never stops. Therefore, if this Force is a member variable
	/// inside a ForceEvent instance, the onComplete event should not be created.
	/// </param>
	public Force(Force template, bool createOnCompleteEvent) 
	{
		if(createOnCompleteEvent)
			// Create a deep copy of the event
			onCompleteEvent = new Brawler.Event(template.onCompleteEvent);
		
		// Copy the templates values into this new instance
		forceType = template.forceType;
		velocity = template.velocity;
		relativeToFacingDirection = template.relativeToFacingDirection;
		target = template.target;
		customTargetPosition = template.customTargetPosition;
		faceTarget = template.faceTarget;
		startTime = template.startTime;
		duration = template.duration;
		
	}
	
	/// <summary>
	/// Copy the values from the given event. This allows this Force instance to emulate the force specified in the
	/// ForceEvent. This is necessary because ForceEvents are not applied on entities directly. It takes a Force instance
	/// to directly apply a Force on an entity. Therefore, to apply a ForceEvent on an entity, its parameters must first be
	/// copied into a Force instance, which is then applied on the entity.
	/// </summary>
	public void CopyValues(ForceEvent forceEvent)
	{
		// Copy the values from the ForceEvent. Note that the ForceEvents do not directly specify a start time or duration.
		// Rather, it is the Brawler.Event instance which wraps this ForceEvent that specifies it. Further, a ForceEvent
		// cannot have an OnCompleteEvent. Otherwise, since ForceEvents are wrapped by a Brawler.Event instance, this would
		// cause a circular recursion which would infinitely create new Brawler.Event and ForceEvent instances
		forceType = forceEvent.forceType;
		velocity = forceEvent.velocity;
		relativeToFacingDirection = forceEvent.relativeToFacingDirection;
		target = forceEvent.target;
		customTargetPosition = forceEvent.customTargetPosition;
		faceTarget = forceEvent.faceTarget;
	}
}