using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The AI Settings for enemies in the game.
/// </summary>

[System.Serializable]
public class EnemyAISettings
{
	/// <summary>
	/// The number of seconds it takes before an enemy decides to attack the player
	/// </summary>
	[Tooltip("The number of seconds it takes before an enemy decides to attack the player")]
	public float attackRate = 1.0f;

	/// <summary>
	/// A fluctuation to introduce randomness in the attack rate.
	/// </summary>
	[Tooltip("A fluctuation (in seconds) to introduce randomness in the attack rate.")]
	public float attackRateFluctuation = 1.0f;

	/// <summary>
	/// The distance the enemies should stay from the player to circle around him
	/// </summary>
	[Tooltip("The distance the enemies should stay from the player to circle around him")]
	public float battleCircleRadius = 2.0f;
	
	/// <summary>
	/// The max number of characters that can attack the player at the same time.
	/// </summary>
	[Tooltip("The max number of characters that can attack the player at the same time.")]
	public int simultaneousAttackers = 1;

	/// <summary>
	/// Copies the settings from the given AISettings instance into this instance
	/// </summary>
	public void Set(EnemyAISettings other)
	{
		// Copy the settings of the given instance
		attackRate = other.attackRate;
		attackRateFluctuation = other.attackRateFluctuation;
		battleCircleRadius = other.battleCircleRadius;
		simultaneousAttackers = other.simultaneousAttackers;
	}
}
