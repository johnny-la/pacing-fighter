using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Defines a container for a mob of enemies. Also controls overall enemy behavior.
/// </summary>
public class EnemyMob : MonoBehaviour 
{
	/** Stores the max number of enemies spawned at once. */
	public const int MAX_ENEMIES = 15;

	/** The settings for the enemy AI. */
	private AISettings aiSettings;

	/** The enemies currently on the battlefield. */
	private List<Character> enemies = new List<Character>(MAX_ENEMIES);
	
	/** Stores the last time when the player was attacked. */
	private float lastAttackTime;
	
	/** Stores the target Character this mob is trying to attack. */
	private Character attackTarget;
	

	void Start()
	{
		GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag ("Enemy");
		for(int i = 0; i < enemyObjects.Length; i++)
			// Store all the existing enemies into the 'enemies' array
			enemies.Add (enemyObjects[i].GetComponent<Character>());
	}

	/// <summary>
	/// Creates an enemy mob of the given difficulty level
	/// </summary>
	public EnemyMob(int difficultyLevel)
	{
		// Set the mob's difficulty level. This changes the parameters of the mob to make it harder or easier.
		SetDifficultyLevel(difficultyLevel);
	}

	/// <summary>
	/// Sets the difficulty level of the enemy mob. Changes its parameters of combat to make it more or less difficult.
	/// </summary>
	public void SetDifficultyLevel(int difficultyLevel)
	{
		// Set the enemies' AI settings to default.
		aiSettings.Set(AIDirector.Instance.defaultAISettings);
	}
	
	public void Update()
	{
		// If 'attackRate' seconds have passed since the last attack on the player
		if((Time.time - lastAttackTime) >= aiSettings.attackRate)
		{
			// Select an enemy to attack the player.
			Character attacker = SelectEnemyToAttack ();
			
			// If an attacker was chosen
			if(attacker != null)
			{
				// Tell the attacker to target the player for an attack. This allows the attacker to know which 
				// character he is attacking. It also signals his behavior tree to start attacking his target
				attacker.CharacterAI.SetAttackTarget (attackTarget);
				
				// Add this attacker to the player's list, so that he knows which enemies are attacking him
				attackTarget.CharacterAI.AddAttacker (attacker);
				
				// Update the last time an enemy attacked the player.
				lastAttackTime = Time.time;
			}
		}
	}
	
	/// <summary>
	/// Returns a character that will attack the player
	/// </summary>
	public Character SelectEnemyToAttack()
	{
		// If the player cannot be attacked, return null, since no enemy can be selected to attack the player
		if(!attackTarget.CharacterAI.CanBeAttacked ())
			return null;
		
		// Return a random enemy from the list of all enemies.
		return ArrayUtils.RandomElement(enemies);
	}
	
	/// <summary>
	/// Spawns an enemy on the battlefield.
	/// </summary>
	public void SpawnEnemy()
	{
		// Spawn an enemy gameObject
		GameObject enemy = Instantiate (aiSettings.zombiePrefab);
		
		// Store the enemy's Character component in a list
		enemies.Add (enemy.GetComponent<Character>());
	}
	
	/// <summary>
	/// Called when the given enemy is killed.
	/// </summary>
	public void OnEnemyDeath(Character enemy)
	{
		// Remove the enemy from the list of currently-active enemies.
		enemies.Remove (enemy);
	}
	
	/// <summary>
	/// Returns an available position an enemy should move to form a circle around the player.
	/// </summary>
	public Vector2 GetAvailablePosition()
	{
		// Reset the mean position to zero. This helper Vector2 stores the average location of the enemies
		Vector2 meanPosition = Vector2.zero;
		
		// Cycle through each enemy 
		for(int i = 0; i < enemies.Count; i++)
		{
			// Add the enemy's position to the mean position of the enemies.
			meanPosition += (Vector2)enemies[i].Transform.position;
		}
		
		// Divide the Vector2 by the number of enemies attacking the player. This generates the average position where the enemies reside
		meanPosition /= enemies.Count;
		
		// Calculate the distance vector from the enemies' mean position to the player's position.
		Vector2 offset = (Vector2)attackTarget.Transform.position - meanPosition;
		// Rescale the offset vector to have a length of 'battleCircleRadius'. This way, the enemies will stay 'battleCircleRadius' units
		// away from the player.
		offset = offset.SetMagnitude(aiSettings.battleCircleRadius);
		
		// Return the position of the player, plus the calculated offset. This denotes an empty space the 
		return (Vector2)attackTarget.Transform.position + offset;
	}

	/// <summary>
	/// Stores the target Character this mob is trying to attack.
	/// </summary>
	public Character AttackTarget
	{
		get { return attackTarget; }
		set { attackTarget = value; }
	}

	/// <summary>
	/// The max number of attackers that can attack the player at the same time.
	/// </summary>
	public int SimultaneousAttackers
	{
		get { return aiSettings.simultaneousAttackers; }
		
		set
		{
			// Update the 'simultaneousAttackers' variable
			aiSettings.simultaneousAttackers = value;
			// Set the max number of attackers that can attack the player at the same time
			attackTarget.CharacterAI.SimultaneousAttackers = value;
		}
	}
}
