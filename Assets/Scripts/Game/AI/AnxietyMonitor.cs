using UnityEngine;
using System.Collections;


/// <summary>
/// The settings used for an AnxietyMonitor instance. This determines the rate at which the anxiety decays in rest mode,
/// how close enemies must be for anxiety to increase, and so on.
/// </summary>
[System.Serializable]
public class AnxietyMonitorSettings
{
	/// <summary>
	/// The rate at which the anxiety increases. This ensures that the character's anxiety never skyrockets upwards too quickly, but
	/// always increases gradually. If set to 1.0, the anxiety increases as quickly as possible. The lower the rate, the more gradually
	/// the anxiety increases
	/// </summary>
	[Tooltip("The rate at which the anxiety increases (in percent/second). The lower the value, the more slowly the anxiety increases")]
	[Range(0,1)]
	public float anxietyGrowthRate;

	/// <summary>
	/// The rate (in percent/second) at which the character's anxiety decreases when he isn't doing anything.
	/// </summary>
	[Tooltip("The rate (in percent/second) at which the character's anxiety decreases.")]
	[Range(0,1)]
	public float anxietyDecayRate;

	/** The amount of rest time required before the character's anxiety starts to decay. */
	//private float anxietyDecayDelay;

	/// <summary>
	/// The rate (in "anxiety units" per second) at which the damage inflicted decreases. For instance, suppose that the character damages
	/// an enemy and gains +2 anxiety. If this value is set to '1', then the +2 anxiety will decrease by -1 every second.
	/// </summary>
	[Tooltip("The rate (in 'anxiety units' per second) at which the anxiety due to damage inflicted decreases")]
	public float damageInflictedDecayRate;

	/// <summary>
	/// If enemies are within this radius, the character's anxiety levels increase. Enemies beyond this radius do not affect the character's anxiety.
	/// </summary>
	[Tooltip("If enemies are within this radius, the character's anxiety levels increase. Enemies beyond this radius do not affect the character's anxiety.")]
	public float dangerRadius;

}

/// <summary>
/// Monitor's a character's level of anxiety (i.e., the amount of difficulty the character is experiencing in combat)
/// </summary>
[System.Serializable]
public class AnxietyMonitor
{
	/** The layer in which this character's enemies are stored. Allows to shoot an OverlapCircle to determine how many enemies are close to the character. */
	private static readonly LayerMask enemyLayer = LayerMask.GetMask ("Enemy");

	/** The amount of time between two successive anxiety updates. If set to '1 / 10.0f', the anxiety level updates 10 times a second.*/
	private const float monitoringTimeStep = 1 / 10.0f; 

	/** The settings for this AnxietyMonitor instance. Determines several factors which affect how fast anxiety increases/decreases. */
	private AnxietyMonitorSettings settings;

	/** The character whose anxiety is being monitored */
	private Character character;

	/** The anxiety felt by the character due to the enemies which fall within this character's 'dangerRadius'. */
	//private float enemyProximityAnxiety;

	/** The anxiety felt by the character due to the damage he inflicts to his enemies. The more damage the enemy deals, the higher his anxiety should be. */
	private float damageInflictedAnxiety;

	/** The level of anxiety the character is currently facing. */
	private float anxiety;

	/** The amount of time for which the character's anxiety has either stayed the same or decreased. */
	private float restTime;

	/// <summary>
	/// Update the anxiety value for the monitored character.
	/// </summary>
	public void Update(float deltaTime)
	{
		// Stores the character's anxiety on the previous update
		float previousAnxiety = anxiety;

		// Stores the character's target anxiety level.
		float targetAnxiety = 0.0f;

		// Increment the character's anxiety by the various factors which affect his anxiety, such as enemy proximity.
		targetAnxiety += EnemyProximityAnxiety ();
		targetAnxiety += HealthAnxiety ();
		targetAnxiety += DamageInflictedAnxiety (deltaTime);

		// Stores the rate at which the character's anxiety interpolates from its previous value to its target value
		float changeRate = 0.0f;

		// Decay the character's anxiety by 'anxietyDecayRate * restTime'. Thus, the longer the character rests, the more his anxiety diminishes
		//anxiety -= settings.anxietyDecayRate * restTime;

		// If the character's anxiety has decreased since the last frame, the character is 'resting'
		if(targetAnxiety < previousAnxiety)
		{
			// Increment the amount of time the character has been resting.
			//restTime += deltaTime;

			// Since the character's anxiety must decrease, set the anxiety's change-rate to the 'decayRate' specified in the settings.
			changeRate = settings.anxietyDecayRate;

		}
		// Else, if the character's anxiety has increased since the last frame
		else 
		{
			// The character is no longer resting, since his anxiety has increased. Thus, set his resting time to zero.
			//restTime = 0.0f;

			// Set the anxiety to change at the rate of the 'anxietyGrowthRate'. Since the character's target anxiety has increased, it must increase at the specified growth rate
			changeRate = settings.anxietyGrowthRate;
		}

		// Interpolate the character's anxiety to its target anxiety. This ensures that any change in anxiety is gradual.
		anxiety = Mathf.Lerp (previousAnxiety, targetAnxiety, changeRate * deltaTime);

		// Rectifies the anxiety value to make sure it never climbs below zero.
		anxiety = Mathf.Clamp (anxiety, 0, anxiety);
	}

	/// <summary>
	/// Returns the amount of anxiety the character is feeling according to the number of enemies that are within his 'dangerRadius'.
	/// </summary>
	private float EnemyProximityAnxiety()
	{
		// Stores the amount of anxiety the 
		float enemyProximityAnxiety = 0.0f;

		// Shoots a CircleCast around the character with a radius of 'battleRadius'. Each enemy within 'dangerRadius' of the character is stored inside an array
		Collider2D[] closeEnemies = Physics2D.OverlapCircleAll (character.Transform.position, settings.dangerRadius, enemyLayer, -10, 100);

		// Cycle through each 
		for(int i = 0; i < closeEnemies.Length; i++)
		{
			// Store the Character component for the enemy which is close to the character being monitored
			Character enemy = closeEnemies[i].GetComponent<Character>();

			// If this enemy is dead, he poses no threat to the character. Thus, ignore the enemy and don't count him towards the character's anxiety level
			if(enemy.CharacterStats.IsDead ())
				continue;

			// Increment the anxiety by the enemy's worth. Therefore, if there are many worthy enemies close to the character, his anxiety will increase
			enemyProximityAnxiety += ((EnemyStats)enemy.CharacterStats).enemyWorth;
		}

		// Return the amount of anxiety the character is experiencing due to the enemies within his 'dangerRadius'
		return enemyProximityAnxiety;

	}

	/// <summary>
	/// Returns the amount of anxiety the character is feeling due to his health. The lower his health, the 
	/// more anxiety the character should be feeling
	/// </summary>
	private float HealthAnxiety()
	{
		// Stores the character's current health percentage.
		float healthPercent = character.CharacterStats.Health / character.CharacterStats.MaxHealth;

		// The amount of anxiety the character is feeling due to his health is the inverse of his percentage of health remaining.
		// Therefore, the less health he has, the more anxiety he feels.
		float healthAnxiety = (1 - healthPercent) * 15;

		// Return the amount of anxiety the character is feeling due to his health.
		return healthAnxiety;
	}

	/// <summary>
	/// Updates and returns the amount of anxiety felt by the character due to the damage he has inflicted. The more the character
	/// attacks, the higher his anxiety.
	/// </summary>
	private float DamageInflictedAnxiety(float deltaTime)
	{
		// As time goes on, decrease the amount of anxiety felt by the character because of the damage he dealt. Like this, the
		// damage the character dealt a long time ago will not affect the character's anxiety as much as the recent damage he did.
		damageInflictedAnxiety -= (settings.damageInflictedDecayRate * deltaTime);

		// Clamp the anxiety to ensure that it never reaches below zero (a negative anxiety does not make sense)
		damageInflictedAnxiety = Mathf.Clamp (damageInflictedAnxiety, 0, damageInflictedAnxiety);

		// Debug.Log("Damage inflicted anxiety: " + damageInflictedAnxiety);

		// Return the amount of anxiety felt by the character due to the amount of damage he is dealing against his enemies
		return damageInflictedAnxiety;
	}

	/// <summary>
	/// Called when the character being monitored for anxiety is hit.
	/// </summary>
	/// <param name="damage">The amount of damage this character received from his adversary.</param>
	public void OnHit(float damage, Character adversary)
	{
		// Stores the squared distance between this character and the adversary he hit
		// float hitDistanceSquared = (character.Transform.position - adversary.Transform.position).sqrMagnitude;

		// Increment the character's anxiety by the amount of damage dealt to him
		//anxiety += damage;
	}

	/// <summary>
	/// Called when the monitored character inflicts damage to an opponent
	/// </summary>
	/// <param name="damage">The amount of damage inflicted to the adversary.</param>
	/// <param name="character">The character which dealt damage to the enemy. This should be the same character as the 
	/// one being monitored .</param>
	/// <param name="adversary">The character which was inflicted by the damage.</param>
	public void OnDamageEnemy(float damage, Character character, Character adversary)
	{
		// Increment the amount of anxiety felt by the character due to the damage he inflicted. This anxiety is directly proportional to the damage he inflicted
		damageInflictedAnxiety += damage / character.CharacterStats.Strength;
	}

	/// <summary>
	/// Returns the anxiety felt by the character being monitored. The anxiety is largely determined by the character's current health,
	/// and the amount of enemies surrounding the character.
	/// </summary>
	public float Anxiety
	{
		get { return anxiety; }
	}

	/// <summary>
	/// The settings for this AnxietyMonitor instance. Determines several factors, such as how fast anxiety decreases during
	/// periods of rest.
	/// </summary>
	public AnxietyMonitorSettings Settings
	{
		get { return settings; }
		set { settings = value; }
	}

	/// <summary>
	/// The character which is being monitored for anxiety.	
	/// </summary>
	public Character CharacterToMonitor
	{
		get { return character; }
		set 
		{ 
			// If the AnxietyMonitor was monitoring another character
			if(character != null)
				// Unsubscribe to the old character's events
				character.CharacterStats.OnDealDamage -= OnDamageEnemy;

			// Update the character being monitored for anxiety
			character = value; 

			// Subscribe to the new character's events. Like this, the AnxietyMonitor will stay up-to-date about the character's state
			character.CharacterStats.OnDealDamage += OnDamageEnemy;
		}
	}


}
