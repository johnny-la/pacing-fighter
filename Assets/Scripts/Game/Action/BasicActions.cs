using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// An enumeration used to refer to the basic actions each character can perform
/// </summary>
public enum BasicActionType
{
	Idle,
	Walk,
	Hit,
	Knockback,
	KnockbackRise,
	Death,
	DeathKnockback,
	NullAction	// Perform no animation and stay still.
}

/// <summary>
/// Denotes a basic action (one that any character can perform)
/// </summary>
[System.Serializable]
public class BasicAction : Action
{
	/// <summary>
	/// The type used to refer to this basic action (Idle, Walk, etc.).
	/// </summary>
	public BasicActionType type;

	/// <summary>
	/// Creates a new action of the given type (Walk, Idle, etc.). </summary>
	public BasicAction(BasicActionType type)
	{
		this.type = type;

		// Create an animation sequence for the basic action
		AnimationSequence animationSequence = new AnimationSequence();

		// Add an animation to the animation sequence. By default, make this the same as the type of the basic action
		animationSequence.animations = new string[1]{type.ToString ()};

		// Store the animation sequence inside this action's 'animationSequences' array
		base.animationSequences = new AnimationSequence[1]{new AnimationSequence()};
	}
}

// <summary>
/// A set of actions every character can perform by default. 
/// </summary>
[System.Serializable]
public class BasicActions
{	
	/** A container for all of the character's basic actions 
	    IMPORTANT: This array is populated in the 'ActionSetEditor.OnEnabled()' function to ensure that the array is up-to-date 
	    whenever the inspector needs to edit the basic actions. */
	public BasicAction[] actions;

	/** The basic actions any character can perform.  */
	public BasicAction idle = new BasicAction(BasicActionType.Idle);
	public BasicAction walk = new BasicAction(BasicActionType.Walk);
	public BasicAction hit = new BasicAction(BasicActionType.Hit);
	public BasicAction knockback = new BasicAction(BasicActionType.Knockback);
	public BasicAction knockbackRise = new BasicAction(BasicActionType.KnockbackRise);
	public BasicAction death = new BasicAction(BasicActionType.Death);
	public BasicAction deathKnockback = new BasicAction(BasicActionType.DeathKnockback);
	public BasicAction nullAction = new BasicAction(BasicActionType.NullAction);

	/** A dictionary which maps a basic action type to its corresponding action. Used to
	 *  easily access the data container (Action instance) for each basic action. */
	private Dictionary<BasicActionType,Action> basicActionsDictionary = new Dictionary<BasicActionType,Action>();

	public BasicActions()
	{
		// Creates a list containing all of the basic actions
		// EDIT: This is done inside 'ActionSetEditor.OnEnabled()'
		/*actions = new BasicAction[]{
			idle,
			walk,
			hit,
			knockback,
			knockbackRise,
			death,
			deathKnockback
		};*/

		// Inialize the default properties for each basic action
		// EDIT: This is called from 'ActionSetEditor.OnEnabled()'
		Init ();
	}
	
	/// <summary>
	/// Initialializes the basic moves' default properties. Accepts the Character which is performing these basic actions.
	/// This method is called in the 'ActionSetEditor.OnEnabled()' method every time the basic actions need to be edited.
	/// Ensures that, if ever a new basic action is created or edited, it is initialized with the correct properties
	/// </summary>
	public void Init()
	{
		// Sets the default properties for the idle action
		idle.name = "Idle";
		idle.cancelable = true;
		idle.animationSequences[0].loopLastAnimation = true;
		basicActionsDictionary.Insert (BasicActionType.Idle, idle);
		
		// Sets the default properties for the walking action
		walk.name = "Walk";
		// Create the force which will make the player walk
		Force walkForce = new Force();
		// Start walking immediately
		walkForce.startTime.type = DurationType.Frame;
		walkForce.startTime.nFrames = 0;
		walkForce.duration.type = DurationType.UsePhysicsData;
		// Move to the user's cursor
		walkForce.forceType = ForceType.Position;
		walkForce.target = TargetPosition.TouchedPosition;
		// Return to idle once character has reached his move target
		walkForce.onCompleteEvent.type = Brawler.EventType.PerformBasicAction;
		walkForce.onCompleteEvent.basicActionToPerform = BasicActionType.Idle;
		walkForce.faceTarget = true;
		walk.forces = new Force[]{walkForce};
		// Extra properties
		walk.cancelable = true;
		walk.listensToInput = true;
		walk.inputType = InputType.Click;
		walk.inputRegion = InputRegion.Any;
		walk.animationSequences[0].loopLastAnimation = true;
		basicActionsDictionary.Insert(BasicActionType.Walk, walk);
		
		// Sets the default properties for the idle action
		hit.name = "Hit";
		hit.cancelable = true;
		hit.overrideCancelable = true;
		basicActionsDictionary.Insert (BasicActionType.Hit, hit);

		// Set the default properties for the 'knockback' action 
		knockback.name = "Knockback";
		// Create the force which will make the character be knocked back 
		/*Force knockbackForce = new Force();
		// Start the knockback force immediately
		knockbackForce.startTime.type = DurationType.Frame;
		knockbackForce.startTime.nFrames = 0;
		knockbackForce.duration.type = DurationType.UsePhysicsData;
		knockbackForce.forceType = ForceType.Velocity;
		knockbackForce.target = TargetPosition.TouchedPosition;
		// Return to idle once character has reached his move target
		walkForce.onCompleteEvent.type = Brawler.EventType.PerformBasicAction;
		walkForce.onCompleteEvent.basicActionToPerform = BasicAction.Idle;
		walkForce.faceTarget = true;
		walk.forces = new Force[]{walkForce};*/
		knockback.cancelable = false;
		knockback.overrideCancelable = true;
		basicActionsDictionary.Insert(BasicActionType.Knockback, knockback);

		// Set the default properties for the 'rising after knockback' action 
		knockbackRise.name = "Knockback_Rise";
		knockbackRise.cancelable = false;
		basicActionsDictionary.Insert(BasicActionType.KnockbackRise, knockbackRise);

		// Set the default properties for the 'die' action 
		death.name = "Death";
		// The event which makes this character die after the death animation finishes playing.
		Brawler.Event deathEvent = new Brawler.Event();
		deathEvent.type = Brawler.EventType.Die;
		// Kill the character once the death animation is complete
		deathEvent.startTime.type = DurationType.WaitForAnimationComplete;
		deathEvent.startTime.animationToWaitFor = 0;
		death.onStartEvents = new Brawler.Event[1]{deathEvent};
		death.cancelable = false;
		basicActionsDictionary.Insert(BasicActionType.Death, death);

		// Set the default properties for the 'DeathKnockback' action
		deathKnockback.name = "DeathKnockback";
		deathKnockback.cancelable = false;
		deathKnockback.overrideCancelable = true;
		basicActionsDictionary.Insert (BasicActionType.DeathKnockback, deathKnockback);

		// Set the default properties for the 'Null' action
		nullAction.name = "NullAction";
		// Loop the 'Null' animation, freezing the character's animation until he performs a new action
		nullAction.animationSequences[0].loopLastAnimation = true;
		basicActionsDictionary.Insert (BasicActionType.NullAction, nullAction);
	}

	/// <summary>
	/// Sets the character which is performing the basic actions. 
	/// IMPORTANT: Must be called on game start to ensure that each basic action knows which character they are going to be performed by
	/// </summary>
	public void SetCharacter(Character character)
	{
		// Cycle through each basic action
		for(int i = 0; i < actions.Length; i++)
		{
			// Tell the basic action which character will be performing this action
			actions[i].character = character;
		}
	}

	/// <summary>
	/// Returns the Action instance corresponding to the given action type. Allows external classes
	/// to easily access the Action instances for each basic action
	/// </summary>
	public Action GetBasicAction(BasicActionType basicAction)
	{
		// Return the action instance corresponding to the given basic action. The dictionary
		// maps enumeration constants to action instances.
		return basicActionsDictionary[basicAction];
	}
	
	/// <summary>
	/// Sets the name of the character's idle animation.
	/// </summary>
	public string IdleAnimation
	{
		get { return idle.animationSequences[0].animations[0]; }
		set { idle.animationSequences[0].animations[0] = value; }
	}
	
	/// <summary>
	/// The default walking animation for the character
	/// </summary>
	public string WalkAnimation
	{
		get { return walk.animationSequences[0].animations[0]; }
		set { walk.animationSequences[0].animations[0] = value; }
	}
	
	/// <summary>
	/// The default hit animation for the character
	/// </summary>
	public string HitAnimation
	{
		get { return hit.animationSequences[0].animations[0]; }
		set { hit.animationSequences[0].animations[0] = value; }
	}

	/// <summary>
	/// The default knockback animation for the character
	/// </summary>
	public string KnockbackAnimation
	{
		get { return knockback.animationSequences[0].animations[0]; }
		set { knockback.animationSequences[0].animations[0] = value; }
	}

	/// <summary>
	/// The default 'rise after knockback' animation for the character
	/// </summary>
	public string KnockbackRiseAnimation
	{
		get { return knockbackRise.animationSequences[0].animations[0]; }
		set { knockbackRise.animationSequences[0].animations[0] = value; }
	}

	/// <summary>
	/// The default death animation for the character
	/// </summary>
	public string DeathAnimation
	{
		get { return death.animationSequences[0].animations[0]; }
		set { death.animationSequences[0].animations[0] = value; }
	}

}