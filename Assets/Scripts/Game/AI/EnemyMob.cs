﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Defines a container for a mob of enemies. Also controls overall enemy behavior.
/// </summary>
public class EnemyMob : MonoBehaviour 
{
	/** Stores the max number of enemies in the mob at once. */
	public const int MAX_ENEMIES = 15;

	/** The settings for the enemies' combat AI. */
	private EnemyAISettings aiSettings;

	/** The enemies currently on the battlefield. */
	private List<Character> enemies;
	
	/** Stores the last time when the player was attacked. */
	private float lastAttackTime;
	
	/** Stores the target Character this mob is trying to attack. */
	private Character attackTarget;

	void Awake()
	{
		// If the EnemyAISettings instance for this enemy mob has not yet been created
		if(aiSettings == null)
			// Create a new EnemyAISettings instance to control the enemies' behaviour in combat
			aiSettings = new EnemyAISettings();
	}

	public void Init()
	{
		// Creates a new list which will hold all the enemies in the mob.
		enemies = new List<Character>(MAX_ENEMIES);

		GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag ("Enemy");
		for(int i = 0; i < enemyObjects.Length; i++)
		{
			Character enemy = enemyObjects[i].GetComponent<Character>();

			// Perform initialization procedures, since the enemy was just added to this EnemyMob
			OnEnemySpawn (enemy);
		}
	}

	/// <summary>
	/// Creates an enemy mob
	/// </summary>
	public EnemyMob()
	{
		// Set the mob's difficulty level. This changes the parameters of the mob to make it harder or easier.
		//SetDifficultyLevel(difficultyLevel);
	}

	/// <summary>
	/// Sets the difficulty level of the enemy mob. Changes its parameters of combat to make it more or less difficult.
	/// </summary>
	/*public void SetDifficultyLevel(int difficultyLevel)
	{
		// Set the enemies' AI settings to default.
		aiSettings.Set(AIDirector.Instance.defaultEnemyAISettings);
	}*/
	
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

		// Choose a random index from the 'enemies' list
		int randomIndex = Random.Range (0, enemies.Count-1);

		// Search through the list of enemies, starting at a random index, until an enemy which can attack has been found
		for(int i = 0; i < enemies.Count; i++)
		{
			// Choose the next enemy in the array using a linear probing technique
			int index = (randomIndex + i) % enemies.Count;

			// Select the random enemy from the 'enemies' list
			Character enemy = enemies[index];

			// If the chosen enemy is already attacking a target, do not choose this enemy again
			if(enemy.CharacterAI.IsAttacking(attackTarget))
			{
				continue;
			}

			// If this statement is reached, an enemy that can attack has been found. Thus, return this enemy
			return enemy;
		}

		// Return null, since no enemy in the mob can attack right now
		return null;
	}

	/// <summary>
	/// Despawns an enemy from the mob that is invisible to the camera. Used to control game difficulty.
	/// </summary>
	public void DespawnEnemy()
	{
		// Stores the camera which displays the world.
		GameCamera camera = GameManager.Instance.GameCamera;

		// Cycle through the list of enemies and find one that is invisible to the camera 
		for(int i = enemies.Count-1; i >= 0; i--)
		{
			Character enemy = enemies[i];

			// If the enemy is invisible to the camera
			if(!camera.IsViewable (enemy.Transform.position))
			{
				Debug.Log ("Kill the enemy " + enemy);
				// Tell the EnemyMob that the enemy has died so that it is removed from the enemy mob.
				OnEnemyDeath (enemy);

				// Kill the enemy.
				enemy.Die ();
				enemy.gameObject.SetActive (false); 
			}
			else
			{
				Debug.Log ("CANNOT kill the enemy " + enemy);
			}
		}
	}
	
	/// <summary>
	/// Spawns an enemy on the battlefield.
	/// </summary>
	public void OnEnemySpawn(Character enemy)
	{
		// Tell the enemy's behavior tree to target the same attackTarget as this EnemyMob. The enemy will now know which
		// character to follow around in order to prepare to attack.
		enemy.CharacterAI.BehaviorTreeAttackTarget = attackTarget.Transform;
		// Tell the enemy's behavior tree that he should be following his target at a distance of 'aiSettings.battleCircleRadius'
		enemy.CharacterAI.BehaviorTreeFollowDistance = aiSettings.battleCircleRadius;

		// Infor the enemy that it has been spawned in this EnemyMob. This 
		((EnemyAI)enemy.CharacterAI).EnemyMob = this;

		// Subscribe to the enemy's death event. The instant the enemy is on the ground and his death animation is complete, he will be removed from this mob.
		enemy.OnDeath += OnEnemyDeath;

		// Store the enemy inside the list of enemies in the mob
		enemies.Add (enemy);
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
	/// Sets the attack stat for each enemy in the mob. Their attack is set to a percentage of their original value.
	/// </summary>
	public void SetEnemyStrength(float percent)
	{
		// Cycle through each enemy and set their attack stat
		for(int i = 0; i < enemies.Count; i++)
		{
			Character enemy = enemies[i];

			// Set the enemy's strength to a percentage of its default value.
			enemy.CharacterStats.Strength = enemy.CharacterStats.defaultStrength * percent;

			//Debug.Log ("Set enemy strength to: " + enemy.CharacterStats.Strength);
		}
	}

	/// <summary>
	/// Sets the defense stat for each enemy in the mob. Their defense is set to a percentage of their original value.
	/// </summary>
	public void SetEnemyDefense(float percent)
	{
		// Cycle through each enemy and set their defense stat
		for(int i = 0; i < enemies.Count; i++)
		{
			Character enemy = enemies[i];
			
			// Set the enemy's strength to a percentage of its default value.
			enemy.CharacterStats.Defense = enemy.CharacterStats.defaultDefense * percent;

			//Debug.Log ("Set enemy defense to: " + enemy.CharacterStats.Defense);
		}
	}

	/// <summary>
	/// Sets the speed stat for each enemy in the mob. Their stat is set to a percentage of their original value.
	/// </summary>
	public void SetEnemySpeed(float percent)
	{
		// Cycle through each enemy and set their speed stat
		for(int i = 0; i < enemies.Count; i++)
		{
			Character enemy = enemies[i];
			
			// Set the enemy's speed to a percentage of its default value.
			enemy.CharacterMovement.PhysicsData.MinWalkSpeed = enemy.CharacterMovement.DefaultPhysicsData.MinWalkSpeed * percent;

			//Debug.Log ("Set enemy speed to: " + enemy.CharacterMovement.PhysicsData.MinWalkSpeed);
		}
	}

	/// <summary>
	/// Updates the battle radius at which the enemies follow their target
	/// </summary>
	public void SetBattleCircleRadius(float radius)
	{
		// Cycle through each enemy
		for(int i = 0; i < enemies.Count; i++)
		{
			Character enemy = enemies[i];
			
			// Set the enemy's battle circle radius to the given value
			enemy.CharacterAI.BehaviorTreeFollowDistance = radius;

			Debug.Log ("Set battle radius to: " + enemy.CharacterAI.BehaviorTreeFollowDistance);
		}

		// Set the battle circle radius in the enemy mob settings
		aiSettings.battleCircleRadius = radius;
	}

	/// <summary>
	/// Stores the target Character this mob is trying to attack.
	/// </summary>
	public Character AttackTarget
	{
		get { return attackTarget; }
		set 
		{ 
			attackTarget = value; 

			// Set the max number of attackers that can attack this mob's target at the same time. 
			attackTarget.CharacterAI.SimultaneousAttackers = SimultaneousAttackers;
		}
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
			// Set the max number of attackers that can attack the target at the same time. 
			attackTarget.CharacterAI.SimultaneousAttackers = value;
		}
	}

	/// <summary>
	/// Returns the number of enemies currently active in the EnemyMob. Dead enemies do not contribute to this count
	/// </summary>
	public int EnemyCount
	{
		get 
		{ 
			// If the 'enemies' List has not yet been defined, this mob contains zero enemies.
			if(enemies == null)
				return 0;

			return enemies.Count; 
		}
	}

	/// <summary>
	/// Sets the AI settings for the enemy mob. Determines how aggressive the enemies in the mob are by specifying
	/// the attack rate, the number of simultaneous attackers, etc.
	/// </summary>
	public EnemyAISettings Settings
	{
		get 
		{ 
			// If the EnemyAISettings instance for this enemy mob has not yet been created
			if(aiSettings == null)
				// Create a new EnemyAISettings instance to control the enemies' behaviour in combat
				aiSettings = new EnemyAISettings();

			return aiSettings; 
		}
		set { aiSettings = value; }
	}
}
