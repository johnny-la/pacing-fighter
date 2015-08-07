using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Called the instant a character dies and is supposed to be erased from the screen. This is only
/// called once the death animation completes and the character can disappear
/// </summary>
public delegate void OnDeathHandler(Character character);

/// <summary>
/// Master class for a character entity. Caches the character's components
/// for global access from a single class.
/// </summary>

public class Character : MonoBehaviour
{
	/** Caches the Character's main components. Acts as a global point of access. */
	private CharacterAnimator characterAnimator;
	private CharacterStats characterStats;
	private CharacterMovement characterMovement;
	private CharacterForces characterForces;
	private CharacterControl characterControl;
	private CharacterCollider characterCollider;
	private CharacterAI characterAI;
	private CharacterTarget characterTarget;
	private SoundManager soundManager;

	/// <summary>
	/// Called the instant a character dies and is supposed to be erased from the screen. This is only
	/// called once the death animation completes and the character's functionality can be deactivated
	/// </summary>
	public event OnDeathHandler OnDeath;

	/** Cache the character's default MonoBehaviour components */
	private new Transform transform;

	private void Awake()
	{
		// Caches the character's components
		this.characterAnimator = (CharacterAnimator)GetComponent<CharacterAnimator>();
		this.characterStats    = (CharacterStats)GetComponent<CharacterStats>();
		this.characterMovement = (CharacterMovement)GetComponent<CharacterMovement>();
		this.characterForces   = (CharacterForces)GetComponent<CharacterForces>();
		this.characterControl  = (CharacterControl)GetComponent<CharacterControl>();
		this.characterCollider = (CharacterCollider)GetComponent<CharacterCollider>();
		this.characterAI       = (CharacterAI)GetComponent<CharacterAI>();
		this.characterTarget   = (CharacterTarget)GetComponent<CharacterTarget>();
		this.soundManager      = GetComponent<SoundManager>();

		// Cache the default MonoBehaviour components
		transform = GetComponent<Transform>();
	}

	private void Update()
	{
	}

	/// <summary>
	/// Disable the Character and all its components. Called in 'CharacterControl.OnDeath()' when this character's
	/// 'Die' event	is triggered
	/// </summary>
	public void Disable()
	{
		characterAnimator.enabled = false;
		characterStats.enabled = false;
		characterMovement.enabled = false;
		characterForces.enabled = false;
		characterControl.enabled = false;
		characterCollider.enabled = false;
		characterAI.enabled = false;
		characterTarget.enabled = false;

		transform.FindChild ("Colliders").gameObject.SetActive (false);
	}

	/// <summary>
	/// Called by CharacterControl.cs the instant a character dies and is supposed to be erased from the screen. This is only
	/// called once the death animation completes and the character can disappear
	/// </summary>
	public void Die()
	{
		// Call this character's 'OnDeath' event. Informs the character's components that this character died.
		if(OnDeath != null)
			OnDeath(this);

		// Disable the character's components to ensure it can no longer interact with the world.
		Disable ();
	}

	/// <summary>
	/// Component responsible for animating the character
	/// </summary>
	public CharacterAnimator CharacterAnimator
	{
		get { return this.characterAnimator; }
	}

	/// <summary>
	/// The character's stats. Controls character health and other relevant statistics
	/// </summary>
	public CharacterStats CharacterStats
	{
		get { return this.characterStats; }
	}

	/// <summary>
	/// Component responsible for moving the player around the world.
	/// </summary>
	public CharacterMovement CharacterMovement
	{
		get { return this.characterMovement; }
	}

	/// <summary>
	/// Responsible for applying forces on the character.
	/// </summary>
	/// <value>The character forces.</value>
	public CharacterForces CharacterForces
	{
		get { return this.characterForces; }
	}

	/// <summary>
	/// Responsible for controlling the character and activating his attack moves
	/// </summary>
	public CharacterControl CharacterControl
	{
		get { return this.characterControl; }
	}

	/// <summary>
	/// Manages the hit boxes attached to the character
	/// </summary>
	public CharacterCollider CharacterCollider
	{
		get { return this.characterCollider; }
	}

	/// <summary>
	/// Return the component which governs this character's artificial intelligence.
	/// </summary>
	public CharacterAI CharacterAI
	{
		get { return this.characterAI; }
	}

	/// <summary>
	/// A container which keeps track of targets, which denote points of interest for the character.	
	/// </summary>
	public CharacterTarget CharacterTarget
	{
		get { return this.characterTarget; }
	}

	/// <summary>
	/// Plays sounds on the character's AudioSource component
	/// </summary>
	public SoundManager Sound
	{
		get { return this.soundManager; }
		set { this.soundManager = value; }
	}

	/// <summary>
	/// A cached reference to the character's Transform
	/// </summary>
	public Transform Transform
	{
		get { return transform; }
		set { this.transform = value; }
	}

}
