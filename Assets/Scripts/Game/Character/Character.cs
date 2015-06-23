using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Master class for a character entity. Caches the character's components
/// for easy access from any class.
/// </summary>

public class Character : MonoBehaviour
{
	private CharacterAnimator characterAnimator;
	private CharacterMovement characterMovement;
	private CharacterControl characterControl;
	private CharacterCollider characterCollider;
	private SoundManager soundManager;

	private void Awake()
	{
		// Caches the character's components
		this.characterAnimator = (CharacterAnimator)GetComponent<CharacterAnimator>();
		this.characterMovement = (CharacterMovement)GetComponent<CharacterMovement>();
		this.characterControl = (CharacterControl)GetComponent<CharacterControl>();
		this.characterCollider = (CharacterCollider)GetComponent<CharacterCollider>();
		this.soundManager = GetComponent<SoundManager>();
	}

	private void Update()
	{
	}

	/// <summary>
	/// Component responsible for animating the character
	/// </summary>
	public CharacterAnimator CharacterAnimator
	{
		get { return this.characterAnimator; }
	}

	/// <summary>
	/// Component responsible for moving the player around the world.
	/// </summary>
	public CharacterMovement CharacterMovement
	{
		get { return this.characterMovement; }
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
	/// Plays sounds on the character's AudioSource component
	/// </summary>
	public SoundManager Sound
	{
		get { return this.soundManager; }
		set { this.soundManager = value; }
	}

}
