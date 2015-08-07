using UnityEngine;
using System;
using System.Collections;

public class EnemyStats : CharacterStats
{
	/// <summary>
	/// The minimum difficulty level required to spawn this enemy
	/// </summary>
	[Tooltip("The minimum difficulty level (as set by the AIDirector) required to spawn this enemy")]
	public int difficultyRequirement = 1;

	/// <summary>
	/// The worth of the enemy. If set to 10, this enemy is worth 10 regular enemies. As another example, suppose this value is set to 3.
	/// Then, each one of these enemies is worth 3 regular enemies. Thus, if 10 enemies are allowed on the battlefield, and 'enemyWorth = 3',
	/// then a maximum of three of these enemies can be on the battlefield at once (assuming there are no other enemies on the battlefield).
	/// </summary>
	[Tooltip("The worth of the enemy. If set to 10, this enemy is worth 10 regular enemies. As another example, suppose this value is set to 3. " +
	 "Then, each one of these enemies is worth 3 regular enemies. Thus, if 10 enemies are allowed on the battlefield, and 'enemyWorth = 3', " +
	 "then a maximum of three of these enemies can be on the battlefield at once (assuming there are no other enemies on the battlefield). ")]
	public int enemyWorth = 1;
}

