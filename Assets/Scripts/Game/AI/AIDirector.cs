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
/// <remarks>
/// Epoch = A "wave" of enemies. During each epoch, the AI Director starts at the build up phase, transitions to the
/// sustain peak and peak fade phases, and ends at the 'relax' phase. Each epoch is a satisfying wave of combat
/// </remarks>
public class AIDirector : MonoBehaviour 
{
	/// <summary>
	/// The amount of time between each AI update. If this is set to 1/10.0f, the director is updated at a "framerate" of
	/// 10 "frames"/second.
	/// </summary>
	public const float aiTimeStep = 1/10.0f;

	/// <summary>
	/// The default AI settings, as set in the inspector.
	/// </summary>
	public EnemyAISettings defaultEnemyAISettings;
	/// <summary>
	/// The default settings for an epoch. During the first epoch, these settings will be used to determine the difficulty of the enemy wave.
	/// </summary>
	public EpochSettings defaultEpochSettings;

	/// <summary>
	/// The settings used to monitor the player's anxiety. Determines several factors, such as how fast the player's anxiety decreases 
	/// during periods of inactivity.
	/// </summary>
	public AnxietyMonitorSettings playerAnxietyMonitorSettings;
	
	/// <summary>
	/// If true, the AIDirector adjusts the enemy's stats to the user's skill level. That is, if the user is having a difficult time
	/// the enemys' stats will lower to help the user finish the current epoch.
	/// </summary>
	public bool adaptEnemyStats = false;
	
	/// <summary>
	/// If true, the AIDirector adjusts the enemy's behaviour to the user's skill level. That is, if the user is having a difficult time
	/// the enemys' behavior will become more naive to help the user finish the current epoch.
	/// </summary>
	public bool adaptEnemyBehavior = false;

	/// <summary>
	/// Monitors the amount of anxiety the player is feeling. Used to determine the game's current intensity.
	/// </summary>
	private AnxietyMonitor playerAnxietyMonitor = new AnxietyMonitor();

	/** The player that the enemies are attacking. His state is tracked to introduce dynamic difficulty adjustment. */
	private Character player;
	
	/** The EnemyMob instance which controls the AI of enemies on-screen. */
	private EnemyMob enemyMob;

	/** The helper class used to spawn enemies on the battlefield. */
	private AISpawner enemySpawner;

	/// <summary>
	/// The current intensity phase of the AIDirector. (Is it building up to the peak intensity, sustaining it, or relaxing?)
	/// </summary>
	private IntensityMode intensityMode = IntensityMode.BuildUp;
	/// <summary>
	/// The current game intensity. Used to gauge the amount of difficulty/anxiety the user is facing. 
	/// </summary>
	private float gameIntensity;

	/// <summary>
	/// The number enemies spawned in the current epoch.
	/// </summary>
	private int enemiesSpawned;
	/// <summary>
	/// The time at which the last enemy was spawned by the AIDirector. Used to keep track of when the next enemy should spawn.
	/// </summary>
	private float lastEnemySpawnTime;
	/// <summary>
	/// The last time at which an enemy was de-spawned. De-spawning enemies is used to dynamically adjust game difficulty.	
	/// </summary>
	private float lastEnemyDespawnTime;

	/// <summary>
	/// The settings used for the current epoch.
	/// </summary>
	private EpochSettings epochSettings;
	/// <summary>
	/// The current epoch of the AI director. Each epoch essentially represents a wave. It consists of a transition between each game intensity state
	/// (build up, sustain peak, peak fade and relax).
	/// </summary>
	private int epoch = 1;	// Start at epoch 1

	/// <summary>
	/// If true, the AI director is paused, and does not spawn enemies nor control enemy AI. 
	/// </summary>
	public bool paused = false;


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

		// Initialize the AIDirector.
		Init();

		Time.timeScale = 1;
	}

	/// <summary>
	/// Initialize the AIDirector. Called on start when the AIDirector is instantiated.
	/// </summary>
	public void Init()
	{
		// Set the epoch settings to default.
		epochSettings = new EpochSettings(defaultEpochSettings);

		// Retrieve the 'AISpawner' instance used to spawn enemies on the battlefield.
		enemySpawner = GetComponent<AISpawner>();
		// Set the enemy spawner to spawn enemies in the GameManager's current level
		enemySpawner.Level = GameManager.Instance.CurrentLevel;

		// Find the player gameObject to control his AI settings
		GameObject playerObject = GameObject.FindGameObjectWithTag ("Player");
		// Cache the player's Character component to modify his AI settings
		player = playerObject.GetComponent<Character>();

		// Retrieve the EnemyMob component attached to this GameObject. This component controls the enemies' behavior
		enemyMob = GetComponent<EnemyMob>();
		// Set the enemy mob's combat settings to default.
		enemyMob.Settings.Set (defaultEnemyAISettings);
		// Make the enemies attack the player.
		enemyMob.AttackTarget = player;
		// Initialize the enemy mob.
		enemyMob.Init();

		// Update the settings for the player's AnxietyMonitor. These settings determine the rate at which player anxiety changes.
		playerAnxietyMonitor.Settings = playerAnxietyMonitorSettings;
		// Sets the AnxietyMonitor to monitor the player's anxiety.
		playerAnxietyMonitor.CharacterToMonitor = player;
		
		// Populate the current level with enemies
		PopulateLevel(GameManager.Instance.CurrentLevel);

		//paused = true;

		// Start updating the AI in a coroutine loop, where the AI is updated every 'aiTimeStep' seconds.
		StartCoroutine(UpdateAI ());
		// Update the game's intensity value every 'aiTimeStep' seconds. 
		StartCoroutine (UpdateGameIntensity());
	}

	/// <summary>
	/// Update the AIDirector's logic
	/// </summary>
	private IEnumerator UpdateAI()
	{
		// Update the AI Director in a loop.
		while(true)
		{
			// If the AI Director is paused, skip this update and wait until the director is unpaused.
			if(paused)
			{
				yield return new WaitForSeconds(aiTimeStep);
				continue;
			}

			// Switch the intensity mode and decide what how to control the AI accordingly by calling the appropriate Update method
			switch(intensityMode)
			{
			case IntensityMode.BuildUp:
				UpdateBuildUp();
				break;
			case IntensityMode.SustainPeak:
				yield return StartCoroutine(UpdateSustainPeak());
				break;
			case IntensityMode.PeakFade:
				UpdatePeakFade();
				break;
			case IntensityMode.Relax:
				yield return StartCoroutine(UpdateRelax());
				break;
			}

			// Wait 'aiTimeStep' seconds before updating the AI Director again. Ensures that the AI runs at a lower "framerate" than everything else
			yield return new WaitForSeconds(aiTimeStep);
		}
	}

	/// <summary>
	/// Update the AI in 'BuildUp' mode. Enemies should start to spawn in order to reach the 'peakThreshold' intensity. 
	/// </summary>
	private void UpdateBuildUp()
	{
		// Debug.Log ("Currently in BUILD UP mode. Number of enemies spawned: " + enemiesSpawned + ". Enemies remaining: " + enemyMob.EnemyCount);

		// If all enemies have been spawned this epoch and all of them are dead, the user killed all the enemies before reaching the SustainPeak mode
		if(enemiesSpawned == epochSettings.maxEnemies && enemyMob.EnemyCount == 0)
		{
			// The current epoch is finished since all enemies are dead. Thus, skip directly to RELAX mode to get ready for the next epoch
			intensityMode = IntensityMode.Relax;

			Debug.LogWarning ("All enemies killed. Skip to RELAX mode");

			// Return from this function. This avoids spawning another enemy before entering RELAX mode.
			return;
		}

		// If there are less enemies spawned than the current epoch permits, keep spawning enemies until the max is reached.
		if(enemyMob.EnemyCount < epochSettings.maxEnemies)
		{
			// If at least '1/buildUpSpawnRate' seconds have passed since the last enemy was spawned, it is time to spawn another enemy. Note that 
			// 'epochSettings.buildUpSpawnRate' enemies should be spawned every second.
			if((Time.time - lastEnemySpawnTime) >= (1/epochSettings.buildUpSpawnRate))
			{
				// Spawn a new enemy in the level. The difficulty setting and 'maxEnemyWorth' is directly proportional to the current epoch number
				enemySpawner.SpawnEnemy (epoch, epoch, enemyMob);

				Debug.LogWarning ("Spawn an enemy");

				// Update the time at which the last enemy was spawned.
				lastEnemySpawnTime = Time.time;

				// Increment the number of enemies spawned this epoch.
				enemiesSpawned++;
			}
		}

		// If the game's intensity has reached the peak intensity threshold, sustain this peak before ramping down again
		if(gameIntensity >= epochSettings.peakIntensityThreshold)
		{
			// Sustain the peak intensity threshold for 'sustainPeakDuration' and then ramp down the intensity
			intensityMode = IntensityMode.SustainPeak;

			Debug.LogWarning("Enter SUSTAIN PEAK mode");
		}
	}

	/// <summary>
	/// Update the AI Director whilst it is in 'SustainPeak' mode. The enemy onslaught should be constant for 'sustainPeakDuration'
	/// seconds and then ramp down accordingly.
	/// </summary>
	private IEnumerator UpdateSustainPeak()
	{
		// Choose a random amount of time (between a min and a max) to sustain the peak intensity of the game
		float sustainPeakDuration = UnityEngine.Random.Range(epochSettings.sustainPeakDuration.min, 
		                                                     epochSettings.sustainPeakDuration.max);

		Debug.LogWarning ("Wait for " + sustainPeakDuration + " seconds");

		// Wait for 'sustainPeakDuration' seconds. This allows the game to maintain its intensity for sufficiently long before ramping back down
		for(float timer = sustainPeakDuration; timer >= 0.0f; timer -= Time.deltaTime)
		{
			yield return null;
		}

		Debug.LogWarning ("Enter PEAK FADE mode");

		// Switch to PeakFade mode. Since the peak intensity has been maintained for long enough, the difficulty should start ramping down
		intensityMode = IntensityMode.PeakFade;
	}

	/// <summary>
	/// Updates AI in the PeakFade phase. Enemies should ramp down in difficulty/number until the game intensity is below the 'relaxUpperBound'
	/// </summary>
	private void UpdatePeakFade()
	{
		// If the game intensity is lower than the 'relaxUpperBound', enter 'Relax' mode. Equivalently, if no more enemies remain,
		// enter relax mode and heal the player before starting the next epoch.
		bool enterRelaxMode = (gameIntensity <= epochSettings.relaxUpperBound || enemyMob.EnemyCount == 0);

		// If the AIDirector should transition to 'Relax' mode
		if(enterRelaxMode)
		{
			// Enter relax mode, maintaining the game intensity to a minimum for 'relaxDuration' seconds ramping up in difficulty again
			intensityMode = IntensityMode.Relax;

			Debug.LogWarning("Enter RELAX mode");
		}

		// Compute the difficulty the user is facing. The higher the game intensity, the higher the factor. The game will then adjust to reduce difficulty
		float difficultyFactor = gameIntensity / epochSettings.peakIntensityThreshold;

		// If the AI Director should adapt the enemies' stats to the user's skill level
		if(adaptEnemyStats)
		{
			// Update the enemies' stats to give the user an easier time if the difficulty is too high
			enemyMob.SetEnemyStrength(1/difficultyFactor);
			enemyMob.SetEnemyDefense(1/difficultyFactor);
		}
		
		// If the AI Director should adapt the enemies' behavior to the user's skill level
		if(adaptEnemyBehavior)
		{
			// Update the enemies' behavior to give the user an easier time if the difficulty is too high
			enemyMob.SetEnemySpeed(1/difficultyFactor);
			enemyMob.SimultaneousAttackers = (int)(Mathf.Max (1, defaultEnemyAISettings.simultaneousAttackers / difficultyFactor));
			enemyMob.Settings.attackRate = (defaultEnemyAISettings.attackRate * difficultyFactor * 1.7f);
			enemyMob.SetBattleCircleRadius(defaultEnemyAISettings.battleCircleRadius + (difficultyFactor/4));

			Debug.Log ("Set simultaneous attackers to: " + enemyMob.SimultaneousAttackers);
			Debug.Log ("Set attack rate to: " + enemyMob.Settings.attackRate);

			// If at least '1/peakFadeDespawnRate' seconds have passed since the last enemy was de-spawned
			if((Time.time - lastEnemyDespawnTime) >= (1.0f/epochSettings.peakFadeDespawnRate))
			{
				// De-spawn another enemy. This ensures that game intensity doesn't rise much higher than the threshold
				enemyMob.DespawnEnemy();

				Debug.Log ("DESPAWN ENEMY");
				
				// Update the last time an enemy was killed by the AIDirector.
				lastEnemyDespawnTime = Time.time;
			}
			else 
			{
				Debug.Log ("Only " + (Time.time - lastEnemyDespawnTime) + "s have passed since last enemy despawn.");
			}
		}
		
		Debug.LogWarning("Difficulty factor: " + difficultyFactor);
	}

	/// <summary>
	/// Update the AIDirector whilst in 'Relax' mode. Enemies should be sparse for the user to prepare for another build up.
	/// </summary>
	private IEnumerator UpdateRelax()
	{
		// Choose a random amount of time (between a min and a max) to sustain relax the intensity of the game
		float relaxDuration = UnityEngine.Random.Range(epochSettings.relaxDuration.min, epochSettings.relaxDuration.max);

		// Regenerate the player back to 100% health once the AIDirector is in Relax phase. Allows the player to prepare for the next epoch.
		player.GetComponent<RegeneratingHealth>().Regenerate(1.0f);

		// Wait for 'relaxDuration' seconds. This allows the game to relax its intensity for sufficiently long before building back up 
		for(float timer = relaxDuration; timer >= 0.0f; timer -= Time.deltaTime)
		{
			yield return null;
		}

		// Start the next epoch. Each epoch is essentially a wave of enemies controlled in difficulty by the AIDirector.
		StartNextEpoch();

		Debug.LogWarning("Enter BUILD UP mode");
	}

	/// <summary>
	/// Updates the game intensity value every 'aiTimeStep' seconds.
	/// </summary>
	/// <returns>The game intensity.</returns>
	private IEnumerator UpdateGameIntensity()
	{
		while(true)
		{
			// Update the player's anxiety level according to the player's health and the number of enemies surrounding him
			playerAnxietyMonitor.Update (aiTimeStep);
			// Set the game's intensity to the same value as the player's anxiety. The more difficulty the player is having, the higher the intensity
			gameIntensity = playerAnxietyMonitor.Anxiety;
			
			//Debug.Log ("Player anxiety: (rounded to nearest hundredth): " + playerAnxietyMonitor.Anxiety.ToString ("F2"));
			
			// Wait 'aiTimeStep' seconds before updating the game intensity again. Updating the intensity can involve expensive physics calls, and thus should update less often
			yield return new WaitForSeconds(aiTimeStep);
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

	}

	/// <summary>
	/// Sarts the next epoch. Each epoch is essentially a wave of enemies controlled in difficulty by the AIDirector.
	/// </summary>
	private void StartNextEpoch()
	{
		// Increment the epoch we are at. Each epoch represents a succession of build up, sustain peak, peak fade and relax modes.
		epoch++;

		// Reset the enemy's spawn counter to zero. This variable keeps track of the number of enemies spawned in the current epoch.
		enemiesSpawned = 0;

		// Reset the enemy settings to default
		enemyMob.SimultaneousAttackers = defaultEnemyAISettings.simultaneousAttackers;
		enemyMob.Settings.attackRate = defaultEnemyAISettings.attackRate;
		enemyMob.SetBattleCircleRadius(defaultEnemyAISettings.battleCircleRadius);
		
		// Update the settings for the next epoch.
		UpdateEpochSettings();

		// Switch to BuildUp mode. Each new epoch starts in BuildUp mode, which spawns enemies until the player's anxiety is high enough
		intensityMode = IntensityMode.BuildUp;

		Debug.LogWarning("START A NEW EPOCH: " + epoch);

	}

	/// <summary>
	/// Updates the settings for the next epoch and store them in the 'epochSettings' variable. Adjusts the difficulty for the current player.
	/// </summary>
	private void UpdateEpochSettings()
	{
		// Depending on the epoch the director is currently at, adjust the settings for the next epoch accordingly. In general, the higher the
		// epoch, the tougher the difficulty.
		switch(epoch)
		{
		case 1:
			epochSettings.maxEnemies = defaultEpochSettings.maxEnemies;
			epochSettings.peakIntensityThreshold = defaultEpochSettings.peakIntensityThreshold;
			break;
		case 2:
			epochSettings.maxEnemies = defaultEpochSettings.maxEnemies + 3;
			epochSettings.peakIntensityThreshold = defaultEpochSettings.peakIntensityThreshold + 3;
			break;
		case 3:
			epochSettings.maxEnemies = defaultEpochSettings.maxEnemies + 8;
			epochSettings.peakIntensityThreshold = defaultEpochSettings.peakIntensityThreshold + 8;
			break;
		case 4:
			epochSettings.maxEnemies = defaultEpochSettings.maxEnemies + 13;
			epochSettings.peakIntensityThreshold = defaultEpochSettings.peakIntensityThreshold + 15;
			break;
		}
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
	/// <param name="character">The adversary which hit another character.</param>
	/// <param name="damagedCharacter">The character damaged by the adversary.</param>
	/// <param name="damage">The damage dealt on the character.</param>
	public void OnDamageDealt(Character character, Character damagedCharacter, float damage)
	{
		// Compute the squared distance between the adversary and the damaged character
		float hitDistanceSquared = (character.Transform.position - damagedCharacter.Transform.position).sqrMagnitude;

		// If the Player inflicted damage to an enemy
		if(character.gameObject.layer == Brawler.Layer.Player)
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

	/// <summary>
	/// Returns a float representing the game's current intensity level. The higher the value, the more anxiety the player is feeling.
	/// </summary>
	public float GameIntensity
	{
		get { return gameIntensity; }
	}

	/// <summary>
	/// The epoch number the director is currently at. The higher the value, the more the enemies in the current wave and the higher
	/// the difficulty.
	/// </summary>
	public int Epoch
	{
		get { return epoch; }
	}

	public bool Paused
	{
		get { return paused; }
		set { paused = value; } 
	}
}
