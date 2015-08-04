using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The AI Settings for enemies in the game.
/// </summary>

[System.Serializable]
public class AISettings
{
	/// <summary>
	/// The number of seconds it takes before an enemy decides to attack the player
	/// </summary>
	public float attackRate = 1.0f;

	/// <summary>
	/// A fluctuation to introduce randomness in the attack rate.
	/// </summary>
	public float attackRateFluctuation = 1.0f;

	/// <summary>
	/// The distance the enemies should stay from the player to circle around him
	/// </summary>
	public float battleCircleRadius = 2.0f;

	/** A template zombie GameObject to instantiate. */
	public GameObject zombiePrefab;
	
	/// <summary>
	/// The max number of characters that can attack the player at the same time.
	/// </summary>
	public int simultaneousAttackers = 1;

	/// <summary>
	/// Copies the settings from the given AISettings instance into this instance
	/// </summary>
	public void Set(AISettings other)
	{
		// Copy the settings of the given instance
		attackRate = other.attackRate;
		attackRateFluctuation = other.attackRateFluctuation;
		battleCircleRadius = other.battleCircleRadius;
		simultaneousAttackers = other.simultaneousAttackers;
	}
}
