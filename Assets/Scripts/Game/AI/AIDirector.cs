using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The phase of intensity. Controls the ups and downs of game intensity in order to give the player adequate feelings
/// of highs and lows of difficulty.
/// </summary>
public enum IntensityMode
{
	BuildUp,
	SustainPeak,
	PeakFade,
	Relax
}

/// <summary>
/// The AI Director which oversees the battlefield and the AI of all entities.
/// </summary>
public class AIDirector : MonoBehaviour 
{
	/// <summary>
	/// The default AI settings, as set in the inspector.
	/// </summary>
	public AISettings defaultAISettings;

	/** The player that the enemies are attacking. His state is tracked to introduce dynamic difficulty adjustment. */
	private Character player;
	
	/** The EnemyMob instance which controls the AI of enemies on-screen. */
	private EnemyMob enemyMob;

	/// <summary>
	/// The duration of the build up.
	/// </summary>
	public float buildUpDuration;
	
	/// <summary>
	/// The amount of time the AI director sustains the peak game intensity for (Left4Dead = 3-5 seconds).
	/// </summary>
	public float sustainPeakDuration;
	
	/// <summary>
	/// The duration of the relaxing phase. This is the amount of time for which enemies become sparse and weak. 
	/// </summary>
	public float relaxDuration;

	/// <summary>
	/// The minimum game intensity required for the game to be at its peak difficulty. The peak difficulty is sustained for 'sustainPeakDuration'
	/// seconds and  then dies down.
	/// </summary>
	public float peakThreshold;
	
	/// <summary>
	/// The current game intensity. Used to gauge the amount of difficulty/anxiety the user is facing. 
	/// </summary>
	private float gameIntensity;


	/** Note: Using the Start() method so that this function is invoked after GameManager.Awake(). */
	void Start()
	{
		// If the singleton instance already exists, and this instance is not the singleton
		if(Instance != null && Instance != this)
		{
			// Destroy this director to avoid having more than one in the scene at any time
			Destroy (gameObject);
		}

		// If this statement is reached, the AIDirector singleton has not yet been defined. Thus, set the singleton to be this instance
		Instance = this;

		// Don't destroy this AIDirector when the scene changes. Ensures that the same director persists throughout the whole game.
		DontDestroyOnLoad (gameObject);

		// Find the player gameObject to control his AI settings
		GameObject playerObject = GameObject.FindGameObjectWithTag ("Player");
		
		// Cache the player's Character component to modify his AI settings
		player = playerObject.GetComponent<Character>();

		// Populate the current level with enemies
		PopulateLevel(GameManager.Instance.CurrentLevel);
	}

	void Update()
	{
		if(gameIntensity > peakThreshold)
		{
			// Sustain the 
			StartCoroutine (SustainPeak());
		}
	}

	/// <summary>
	/// Populates the given level with enemies. Call this the first time the level is loaded.
	/// </summary>
	public void PopulateLevel(Level level)
	{
		//EnemyMob enemyMob = new EnemyMob(1);

		//GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag ("Enemy");
		//for(int i = 0; i < enemyObjects.Length; i++)
			// Store all the existing enemies into the 'enemies' array
			//enemies.Add (enemyObjects[i].GetComponent<Character>());

		// Retrieve the EnemyMob component attached to this GameObject. This component controls the enemies' behavior
		enemyMob = GetComponent<EnemyMob>();
		
		// Make the enemies attack the player.
		enemyMob.AttackTarget = player;

	}

	/// <summary>
	/// Set the difficulty for the AI director. This determines how difficult the next epoch of enemies will be 
	/// </summary>
	public void SetDifficulty(int difficultyLevel)
	{

	}

	/// <summary>
	/// Called whenever a Character deals damage to another character.
	/// </summary>
	/// <param name="adversary">The adversary which hit another character.</param>
	/// <param name="damagedCharacter">The character damaged by the adversary.</param>
	/// <param name="damage">The damage dealt on the character.</param>
	public void OnDamageDealt(Character adversary, Character damagedCharacter, float damage)
	{
		// Compute the squared distance between the adversary and the damaged character
		Vector2 squareDistance = (adversary.Transform.position - damagedCharacter.Transform.position).sqrMagnitude;

		// If the Player inflicted damage to an enemy
		if(adversary.gameObject.layer == Brawler.Layer.Player)
		{

		}

		// If the player was damaged, the game intensity should increase accordingly
		if(damagedCharacter.gameObject.layer == Brawler.Layer.Player)
		{

		}
	}


	/// <summary>
	/// The Singleton instance for the AIDirector. Oversees all enemy behavior.
	/// </summary>
	public static AIDirector Instance
	{
		get; private set;
	}
}
