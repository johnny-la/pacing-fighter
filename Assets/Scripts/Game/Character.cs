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

	private void Awake()
	{
		// Caches the character's components
		this.characterAnimator = (CharacterAnimator)GetComponent<CharacterAnimator>();
		this.characterMovement = (CharacterMovement)GetComponent<CharacterMovement>();
		this.characterControl = (CharacterControl)GetComponent<CharacterControl>();

        Debug.Log(characterControl);
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

}
