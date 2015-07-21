using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// An enumeration used to refer to the basic actions each character can perform
/// </summary>
public enum BasicAction
{
	Idle,
	Walk,
	Hit,
	Knockback,
	KnockbackRise,
	Death
}

// <summary>
/// A set of actions every character can perform by default. 
/// </summary>
[System.Serializable]
public class BasicActions
{
	/** The basic actions any character can perform */
	public Action idle = new Action();
	public Action walk = new Action();
	public Action hit = new Action();
	public Action knockback = new Action();
	public Action knockbackRise = new Action();
	public Action death = new Action();
	
	/** A container for all of the character's basic actions */
	public Action[] actions;

	/** A dictionary which maps a basic action type to its corresponding action. Used to
	 *  easily access the data container (Action instance) for each basic action. */
	private Dictionary<BasicAction,Action> basicActionsDictionary = new Dictionary<BasicAction,Action>();
	
	/// <summary>
	/// Initialializes the basic moves' default properties. Accepts the Character which is performing these basic actions
	/// </summary>
	public void Init(Character character)
	{
		// Sets the default properties for the idle action
		idle.name = "Idle";
		idle.character = character;
		idle.cancelable = true;
		idle.animationSequences[0].loopLastAnimation = true;
		basicActionsDictionary.Add (BasicAction.Idle, idle);
		
		// Sets the default properties for the walking action
		walk.name = "Walk";
		walk.character = character;
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
		walkForce.onCompleteEvent.basicActionToPerform = BasicAction.Idle;
		walkForce.faceTarget = true;
		walk.forces = new Force[]{walkForce};
		// Extra properties
		walk.cancelable = true;
		walk.listensToInput = true;
		walk.inputType = InputType.Click;
		walk.inputRegion = InputRegion.Any;
		walk.animationSequences[0].loopLastAnimation = true;
		basicActionsDictionary.Add (BasicAction.Walk, walk);
		
		// Sets the default properties for the idle action
		hit.name = "Hit";
		hit.character = character;
		hit.cancelable = true;
		hit.overrideCancelable = true;
		basicActionsDictionary.Add (BasicAction.Hit, hit);

		// Set the default properties for the 'knockback' action 
		knockback.name = "Knockback";
		knockback.character = character;
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
		basicActionsDictionary.Add (BasicAction.Knockback, knockback);

		// Set the default properties for the 'rising after knockback' action 
		knockbackRise.name = "Knockback_Rise";
		knockbackRise.character = character;
		knockbackRise.cancelable = false;
		basicActionsDictionary.Add(BasicAction.KnockbackRise, knockbackRise);

		// Set the default properties for the 'die' action 
		knockbackRise.name = "Death";
		knockbackRise.character = character;
		knockbackRise.cancelable = false;
		basicActionsDictionary.Add(BasicAction.Death, death);
		
		// Create the array which contains all of the character's basic actions
		actions = new Action[]{idle,walk,hit};
	}

	/// <summary>
	/// Returns the Action instance corresponding to the given action type. Allows external classes
	/// to easily access the Action instances for each basic action
	/// </summary>
	public Action GetBasicAction(BasicAction basicAction)
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