using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// TODO: UPDATE THIS COMPLETELY:
// Do the following: Choose a random cell in which to spawn an enemy. If a cell visible to the camera is chosen,
// make the enemy spawn in a location that makes visual sense. Otherwise, if the cell is invisible to the camera,
// spawn the enemy by just making him appear.

public class AISpawner : MonoBehaviour 
{
	/// <summary>
	/// The amount of time between each update. The AI logic should run at a lower framerate for performance reasons
	/// </summary>
	public const float aiTimeStep = 1 / 10.0f;

	/** The level in which the AIs are spawned. */
	private Level level;

	/** The cells that are viewable by the game camera. No AIs will be spawned here to avoid them being seen whilst spawned. */
	private List<Cell> visibleCells = new List<Cell>();

	/** The cells which are not viewable by the game camera. AIs can be spawned in these cells. */
	private List<Cell> invisibleCells = new List<Cell>();

	void Awake()
	{
		// Start an update loop for the AI
		StartCoroutine(AIUpdateLoop());
	}

	/// <summary>
	/// Updates the AI spawner's internal logic at a fixed rate
	/// </summary>
	public IEnumerator AIUpdateLoop()
	{
		// Loop infinitely
		while(true)
		{
			// Update the visible/invisible cells in the level by updating the 'visible/invisibleCells' lists
			UpdateVisibleCells ();

			// Wait before the AI logic is updated again
			yield return new WaitForSeconds(aiTimeStep);
		}
	}

	/// <summary>
	/// Spawns an enemy on the battlefield. Spawns the enemy inside the given mob
	/// </summary>
	public void SpawnEnemy(GameObject enemyPrefab, EnemyMob enemyMob)
	{
		// Spawn an enemy gameObject
		GameObject enemy = Instantiate (enemyPrefab);
		
		// Inform the EnemyMob that an enemy was spawned inside it. Store the enemy's Character component inside an internal list
		enemyMob.OnEnemySpawn (enemy.GetComponent<Character>());

		//enemies.Add (enemy.GetComponent<Character>());
	}

	/// <summary>
	/// Updates the 'invisibleCells/visibleCells' arrays to store the cells which are viewable and unviewable by the camera.
	/// </summary>
	public void UpdateVisibleCells()
	{
		// Clear the list of visible/invisible cells to repopulate them.
		visibleCells.Clear ();
		invisibleCells.Clear ();

		// Store the cell coordinates for each traversable cell in the level
		List<Cell> traversableCells = level.TraversableCells;

		// Cycle through each traversable cell in the level
		for(int i = 0; i < traversableCells.Count; i++)
		{
			// If the cell is visible to the camera
			if(IsCellVisible(traversableCells[i], GameManager.Instance.GameCamera))
			{
				// Add the cell to the list of viewable cells
				visibleCells.Add (traversableCells[i]);
			}
			// Else, if the cell is invisible to the camera
			else
			{
				// Add the cell to the list of invisible cells
				invisibleCells.Add (traversableCells[i]);
			}
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
