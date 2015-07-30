using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
	/// <summary>
	/// The currently-active level in the game
	/// </summary>
	private Level currentLevel;

	/// <summary>
	/// The camera which is currently displaying the game world.
	/// </summary>
	private GameCamera gameCamera;

	void Awake()
	{
		// If the 'Instance' singleton is already created, but this instance is not the singleton
		if(Instance != null && Instance != this)
			// Destroy this GameCamera instance to avoid having more than one GameManager in the Hierarchy at the same time
			Destroy (gameObject);
		
		// Set the GameObject singleton to this instance, since no singleton has yet to be set.
		Instance = this;
		
		// Don't destroy the GameManager when a new scene is loaded. Ensures that the same GameManager is kept from scene to scene
		DontDestroyOnLoad (gameObject);

		// Retrieve the GameCamera component from the main camera in the scene. This will act as the main game camera
		gameCamera = Camera.main.GetComponent<GameCamera>();

		// Set the current level to be the Level script attached to the GameManager object
		currentLevel = GetComponent<Level>();

		// Generate a level starting at (0,0) going from the left to the right
		currentLevel.GenerateLevel(new Vector2(0,0), CellAnchor.Left, CellAnchor.Right);
	}

	/// <summary>
	/// The static GameManager instance every script has access to.
	/// </summary>
	public static GameManager Instance
	{
		get; private set;
	}

	/// <summary>
	/// The camera which is currently displaying the game world
	/// </summary>
	public GameCamera GameCamera
	{
		get { return gameCamera; }
		set { gameCamera = value; }
	}
}
