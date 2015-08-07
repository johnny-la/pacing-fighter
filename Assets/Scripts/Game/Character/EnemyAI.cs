
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Component which manages general enemy AI
/// </summary>
public class EnemyAI : CharacterAI
{
	/** The EnemyMob to which this enemy belongs. This instance contains all the enemies in the same mob as this one.
	    It allows the enemy to determine its behavior in a group. (i.e., where should this enemy be placed relative to the others?) */
	private EnemyMob enemyMob;

	/// <summary>
	/// The EnemyMob to which this enemy belongs. This instance contains all the enemies in the same mob as this one.
	/// It allows the enemy to determine its behavior in a group. (i.e., where should this enemy be placed relative to the others?)
	/// </summary>
	public EnemyMob EnemyMob
	{
		get { return enemyMob; }
		set { enemyMob = value; }
	}
}

