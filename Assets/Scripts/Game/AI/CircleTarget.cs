using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// Make an enemy move towards an empty position that will form a circle around the player.
/// </summary>

public class CircleTarget : BehaviorDesigner.Runtime.Tasks.Action 
{
	/** The Character component attached to the GameObject performing this action. */
	private Character character;

	/** The EnemyMob this enemy is a part of. Computes the position the character should move to to circle his target */
	private EnemyMob enemyMob;

	/** The position the enemy should move to to circle the player */
	private Vector2 moveTarget;

	public override void OnAwake()
	{
		// Cache the character component for the character performing this action
		character = transform.GetComponent<Character>();
	}

	public override void OnStart()
	{
		// Cache the EnemyMob to which this enemy belongs. This tells the character where he should move to circle his current attacking target.
		enemyMob = ((EnemyAI)character.CharacterAI).EnemyMob;

		// Compute an available position for the enemy to move to. This forms a better circle around the player.
		moveTarget = enemyMob.GetAvailablePosition ();

		// Move the character to his movement target. This target is an empty space that will help form a circle around the player.
		character.CharacterMovement.MoveTo (moveTarget);
	}

	public override TaskStatus OnUpdate()
	{
		// When the character reaches his target position, return success. The character has reached his target when 'HasMoveTarget' is false.
		if(!character.CharacterMovement.MoveToTargetScript.HasMoveTarget)
		{
			return TaskStatus.Success;
		}

		// If this statement is reached, the character is still moving to his target location. Thus, return a status of 'Runnning'
		return TaskStatus.Running;
	}

}
