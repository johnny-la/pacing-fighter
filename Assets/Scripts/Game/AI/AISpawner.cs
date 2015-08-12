using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// TODO: UPDATE THIS COMPLETELY:
// Do the following: To spawn an enemy, choose a random cell in which it. If a cell visible to the camera is chosen,
// make the enemy spawn in a location that makes visual sense. Otherwise, if the cell is invisible to the camera,
// spawn the enemy by just making him appear.

public class AISpawner : MonoBehaviour 
{
	/// <summary>
	/// The amount of time between each update. The AI logic should run at a lower framerate for performance reasons
	/// </summary>
	public const float aiTimeStep = 1 / 10.0f;

	/// <summary>
	/// Stores a list of enemies that this spawner can spawn.
	/// </summary>
	public Character[] enemyPrefabs;

	/** The level in which the AIs are spawned. */
	private Level level;

	/** The cells in which enemies can be spawned. These cells are at the edge of the camera's viewable region. That is, the cells
	    are only partially viewable by the camera. */
	private List<Cell> spawningCells = new List<Cell>();

	void Awake()
	{
		// Start an update loop for the AI
		StartCoroutine(AIUpdateLoop());
	}

	/// <summary>
	/// Updates the AI spawner's internal logic at a fixed rate
	/// </summary>
	private IEnumerator AIUpdateLoop()
	{
		// Loop at the AI's update rate
		while(true)
		{
			// Update the visible/invisible cells in the level by updating the 'visible/invisibleCells' lists
			UpdateVisibleCells ();

			// Wait before the AI logic is updated again
			yield return new WaitForSeconds(aiTimeStep);
		}
	}

	/// <summary>
	/// Spawns an enemy on the battlefield. The enemy is spawned inside the given mob
	/// </summary>
	/// <returns>The spawned enemy </returns>
	public Character SpawnEnemy(int difficultyLevel, int maxEnemyWorth, EnemyMob enemyMob)
	{
		// Select a random enemy to spawn from the list of spawnable enemies ('enemyPrefabs:Enemy[]').
		Character enemyPrefab = ChooseRandomEnemy(difficultyLevel, maxEnemyWorth);

		// Spawn an instance of the chosen enemy 
		Character enemy = Instantiate (enemyPrefab) as Character;

		// Choose a position in which to spawn the enemy
		Vector2 spawnPosition = GetRandomSpawnPoint();
		// Spawn the enemy in the random position computed above
		enemy.Transform.position = spawnPosition;
		
		// Inform the EnemyMob instance that an enemy was spawned inside it. Allows the mob to keep track of the enemies inside of it
		enemyMob.OnEnemySpawn (enemy);

		//enemies.Add (enemy.GetComponent<Character>());

		// Return the spawned enemy. 
		return enemy;
	}

	/// <summary>
	/// Returns a random position in which to spawn a new enemy. This position is invisible to the player.
	/// </summary>
	/// <returns>The random spawn point.</returns>
	private Vector2 GetRandomSpawnPoint()
	{
		// Get a spawn point ahead of the player, at most 4 cells from the player's position
		return GetRandomSpawnPoint (true, 4);
	}

	/// <summary>
	/// Returns a random position in which to spawn a new enemy. This position will be invisible to the player.
	/// </summary>
	/// <param name="ahead">If set to true, the enemy is spawned ahead of the player. If false, the enemy is spawned
	/// behind the player. If there is not enough room ahead of the player to spawn an entity, a position behind the
	/// player is chosen. </param>
	/// <param name="maxDistanceAhead">The max number of cells between the player and the spawn position.</param>
	private Vector2 GetRandomSpawnPoint(bool ahead, int maxDistanceFromPlayer)
	{
		// Choose a random cell in which to spawn an enemy from the list of spawning cells
		Cell cell = ArrayUtils.RandomElement (spawningCells);

		// Get the LevelCell in which the enemy will spawn
		LevelCell levelCell = level.GetLevelCell (cell);

		// Stores the position where the enemy will spawn
		Vector2 spawnPosition = Vector2.zero;

		// Stores the camera used to view the world. This will be used to choose a location invisible to the camera
		GameCamera camera = GameManager.Instance.GameCamera;

		// Retrieve the character controled by the player
		Character player = GameManager.Instance.Player;

		// Compute the cell coordinates at which the player resides on the level
		Cell playerCell = level.GetCellFromPosition (player.Transform.position);
		// Retrieve the LevelCell the player is standing on
		LevelCell playerLevelCell = level.GetLevelCell (playerCell);

		// Stores the distance (in cell units) between the player and the start of the level. This indicates how far the user is in the level.
		int distanceFromPlayerToStart = playerLevelCell.DistanceToStart;

		Debug.LogWarning("Distance from player to start: " + distanceFromPlayerToStart + " cells");

		// Iterates from '1' to 'maxDistanceFromPlayer'. This is the amount of cells ahead of the player the entity is spawned.
		for(int distanceAhead = 1; distanceAhead <= maxDistanceFromPlayer; distanceAhead++)
		{
			// Stores the distance from the beginning of the level to the position where the enemy may be spawned
			int distanceFromStart = distanceFromPlayerToStart;

			// If the entity should be spawned ahead of the player
			if(ahead)
			{
				// Choose a distance further down the level by adding the 'distanceAhead'.
				distanceFromStart += distanceAhead;
			}
			// Else, if the spawn position should be located in back of the player
			else
			{
				// Choose a distance closer to the start of the level by subtracting by 'distanceAhead'.
				distanceFromStart -= distanceAhead;
			}

			// Get a random cell which lies 'distanceFromStart' cells away from the start of the level.
			LevelCell aheadCell = level.GetCellAtDistance(distanceFromStart);

			// If the cell ahead of the player is null, the search for a spawn position has failed.
			if(aheadCell == null)
			{
				// Invert the 'ahead' boolean. The next search, the spawn position will be found in the opposite direction
				ahead = !ahead;

				// Reset the distanceAhead integer to restart the search in the opposite direction
				distanceAhead = 0;

				// Restart the loop and search for a spawn position in the opposite direction of the player
				continue;
			}

			// If the chosen cell's center-position is invisible to the camera, choose this cell as the spawn point.
			if(!camera.IsViewable (aheadCell.Transform.position))
			{
				// Spawn the enemy in the middle of the LevelCell which lies ahead of the player.
				spawnPosition = aheadCell.Transform.position;

				// A good spawn position has been found, invisible to the camera. Thus, break from this loop
				break;
			}
		}
		
		// Compute the position of each edge of the LevelCell. One point which is invisible to the camera will be chosen as a spawn point
		/*Vector2 leftEdge = new Vector2 (levelCell.Left, levelCell.Transform.position.y);
		Vector2 rightEdge = new Vector2 (levelCell.Right, levelCell.Transform.position.y);
		Vector2 topEdge = new Vector2 (levelCell.Transform.position.x, levelCell.Top);
		Vector2 bottomEdge = new Vector2 (levelCell.Transform.position.x, levelCell.Bottom);
		
		// If the left edge of the cell is invisible to the camera
		if(!camera.IsViewable(leftEdge))
		{
			// Spawn the enemy at the left edge of the cell
			spawnPosition = leftEdge;
		}
		// If the right edge of the cell is invisible to the camera
		else if(!camera.IsViewable(rightEdge))
		{
			// Spawn the enemy at the right-most edge of the cell
			spawnPosition = rightEdge;
		}
		// If the top edge of the cell is invisible to the camera
		else if(!camera.IsViewable(topEdge))
		{
			// Spawn the enemy at the top-most edge of the cell
			spawnPosition = topEdge;
		}
		// If the bottom edge of the cell is invisible to the camera
		else if(!camera.IsViewable(bottomEdge))
		{
			// Spawn the enemy at the bottom-most edge of the cell
			spawnPosition = bottomEdge;
		}*/
		
		// Return the spawning position chosen above. This is where the next enemy will be spawned.
		return spawnPosition;
	}

	/// <summary>
	/// Chooses a random enemy from the internal list of spawnable enemies.
	/// </summary>
	/// <returns>The random enemy.</returns>
	/// <param name="difficultyLevel">The current difficulty level. Only enemies whose 'Enemy.difficultyRequirement' is less than or equal to 
	/// this value can be chosen. </param>
	/// <param name="maxEnemyWorth">The max worth of the . This is used as follows: If an enemy's worth is too high, spawning this enemy may be
	/// the equivalent of spawning 5 normal enemies. Thus, for an enemy to be chosen, its 'Enemy.enemyWorth' value is equal to 5, and must
	/// be less than or equal to 'maxEnemyWorth' to be spawned. </param>
	private Character ChooseRandomEnemy(int difficultyLevel, int maxEnemyWorth)
	{
		// Choose a random index from the list of enemy prefabs
		int randomIndex = UnityEngine.Random.Range (0, enemyPrefabs.Length);

		// Perform a linear probe on the 'enemyPrefabs' array until an enemy is found that satisfies the given requirements
		for(int i = 0; i < enemyPrefabs.Length; i++)
		{
			// Choose the next random index in the 'enemyPrefabs' array, starting at index 'randomIndex' and ending at 
			// '(randomIndex + (enemyPrefabs.Length-1)) % enemyPrefabs.Length'. This essentially scans through the entire
			// array until a suitable enemy is found
			int index = (randomIndex + i) % enemyPrefabs.Length;

			// Retrieve the chosen enemy from the 'enemyPrefabs' array
			Character enemy = enemyPrefabs[index];
			// Cache the component which stores the enemy's stats. Note: since enemy is a prefab, it isn't yet instantiated. Thus,
			// its 'enemy.CharacterStats' property is null, and its CharacterStats component must be found manually.
			EnemyStats enemyStats = (EnemyStats) enemy.GetComponent<CharacterStats>();

			// If the chosen enemy's difficulty requirement is lower or equal to the desired difficulty level, the enemy is elligible to be chosen
			if(enemyStats.difficultyRequirement <= difficultyLevel)
			{
				// If the chosen enemy's worth is less than or equal than the maxEnemyWorth, this enemy is weak enough to be spawned.
				if(enemyStats.enemyWorth <= maxEnemyWorth)
				{
					// Return the chosen enemy, since he satisfies the requirements specified by the method's parameters
					return enemy;
				}
			}
		}

		// If this statement is reached, no enemy in this AISpawner's enemy list satisfies the given requirements. Thus, return null.
		return null;
	}

	/// <summary>
	/// Updates the 'invisibleCells/visibleCells' arrays to store the cells which are viewable and unviewable by the camera.
	/// </summary>
	public void UpdateVisibleCells()
	{
		// If the level member variable has not yet been set, there is no level to scan through for visible cells.
		if(level == null)
			// Thus, return to avoid NullReferenceExceptions
			return;

		// Retrieve the camera used to display the game world
		GameCamera camera = GameManager.Instance.GameCamera;

		// Store the position of the four edges of the camera
		Vector2 leftCenter = new Vector2(camera.Left, camera.Transform.position.y);
		Vector2 rightCenter = new Vector2(camera.Right, camera.Transform.position.y);
		Vector2 topCenter = new Vector2(camera.Transform.position.x, camera.Top);
		Vector2 bottomCenter = new Vector2(camera.Transform.position.x, camera.Bottom);

		// Retrieves the cells visible at the four edges of the camera.
		Cell leftCell = level.GetCellFromPosition (leftCenter);
		Cell rightCell = level.GetCellFromPosition (rightCenter);
		Cell topCell = level.GetCellFromPosition (topCenter);
		Cell bottomCell = level.GetCellFromPosition (bottomCenter);

		// Clear the list of cells in which enemies can spawn.
		spawningCells.Clear();

		// If the cell visible to the left edge of the camera is traversable
		if(level.IsTraversable (leftCell))
		{
			// Add the cell to the list of potential spawn points
			spawningCells.Add(leftCell);
		}
		// If the cell visible to the right edge of the camera is traversable
		if(level.IsTraversable (rightCell))
		{
			// Add the cell to the list of potential spawn points
			spawningCells.Add(rightCell);
		}
		// If the cell visible to the top edge of the camera is traversable
		if(level.IsTraversable (topCell))
		{
			// Add the cell to the list of potential spawn points
			spawningCells.Add(topCell);
		}
		// If the cell visible to the bottom edge of the camera is traversable
		if(level.IsTraversable (bottomCell))
		{
			// Add the cell to the list of potential spawn points
			spawningCells.Add(bottomCell);
		}
	}

	/// <summary>
	/// Returns true if the cell in the level is visible by the given camera.
	/// </summary>
	private bool IsCellVisible(Cell cell, GameCamera camera)
	{
		// Get the LevelCell corresponding to the given cell coordinates.
		LevelCell levelCell = level.GetLevelCell (cell);

		// If the camera's frustum does not overlap the level cell
		if(camera.Right < levelCell.Left || camera.Left >= levelCell.Right 
		   || camera.Top < levelCell.Bottom || camera.Bottom > levelCell.Top)
		{
			// Return false, since the camera cannot see the cell
			return false;
		}

		// Return true if this statement is reached, since the camera can see the cell in the level
		return true;
	}

	/// <summary>
	/// The level in which the AIs are spawned.
	/// </summary>
	public Level Level
	{
		get { return level; }
		set { level = value; }
	}
}
