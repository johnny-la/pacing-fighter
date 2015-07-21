using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AISettings : MonoBehaviour 
{
	/** Stores the max number of enemies spawned at once. */
	public const int MAX_ENEMIES = 15;

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

	/** The max number of characters that can attack the player at the same time. */
	private int simultaneousAttackers = 1;

	/** Stores the last time when the player was attacked. */
	private float lastAttackTime;

	/** Stores the player's Character component */
	private Character player;

	/** The enemies currently on the battlefield. */
	private List<Character> enemies = new List<Character>(MAX_ENEMIES);

	/** Helper Vector2s used to avoid instantiation and destruction of several Vector2s. Used in GetAvailablePosition() */
	private Vector2 meanPosition = Vector2.zero;
	private Vector2 offset = Vector2.zero;

	void Start()
	{
		// Find the player gameObject to control his AI settings
		GameObject playerObject = GameObject.FindGameObjectWithTag ("Player");

		// Cache the player's Character component to modify his AI settings
		player = playerObject.GetComponent<Character>();

		// Tell the player the max number of characters that can attack him at the same time.
		player.CharacterAI.SimultaneousAttackers = simultaneousAttackers;

		GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag ("Enemy");
		for(int i = 0; i < enemyObjects.Length; i++)
			// Store all the existing enemies into the 'enemies' array
			enemies.Add (enemyObjects[i].GetComponent<Character>());
	}

	public void Update()
	{
		// If 'attackRate' seconds have passed since the last attack on the player
		if((Time.time - lastAttackTime) >= attackRate)
		{
			// Select an enemy to attack the player.
			Character attacker = SelectEnemyToAttack ();

			// If an attacker was chosen
			if(attacker != null)
			{
				// Tell the attacker to target the player for an attack. This allows the attacker to know which 
				// character he is attacking. It also signals his behavior tree to start attacking his target
				attacker.CharacterAI.SetAttackTarget (player);
				
				// Add this attacker to the player's list, so that he knows which enemies are attacking him
				player.CharacterAI.AddAttacker (attacker);
				
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
		if(!player.CharacterAI.CanBeAttacked ())
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
		GameObject enemy = Instantiate (zombiePrefab);

		// Store the enemy's Character component in a list
		enemies.Add (enemy.GetComponent<Character>());
	}

	/// <summary>
	/// Returns an available position an enemy should move to form a circle around the player.
	/// </summary>
	public Vector2 GetAvailablePosition()
	{
		// Reset the mean position to zero. This helper Vector2 stores the average location of the enemies
		meanPosition.Set (0,0);

		// Cycle through each enemy 
		for(int i = 0; i < enemies.Count; i++)
		{
			// Add the enemy's position to the mean position of the enemies.
			meanPosition += (Vector2)enemies[i].Transform.position;
		}

		// Divide the Vector2 by the number of enemies attacking the player. This generates the average position where the enemies reside
		meanPosition /= enemies.Count;

		// Calculate the distance vector from the enemies' mean position to the player's position.
		offset = (Vector2)player.Transform.position - meanPosition;
		// Rescale the offset vector to have a length of 'battleCircleRadius'. This way, the enemies will stay 'battleCircleRadius' units
		// away from the player.
		offset = offset.SetMagnitude(battleCircleRadius);

		// Return the position of the player, plus the calculated offset. This denotes an empty space the 
		return (Vector2)player.Transform.position + offset;
	}

	/// <summary>
	/// The max number of attackers that can attack the player at the same time.
	/// </summary>
	public int SimultaneousAttackers
	{
		get { return simultaneousAttackers; }

		set
		{
			// Update the 'simultaneousAttackers' variable
			simultaneousAttackers = value;
			// Set the max number of attackers that can attack the player at the same time
			player.CharacterAI.SimultaneousAttackers = value;
		}

	}
}
