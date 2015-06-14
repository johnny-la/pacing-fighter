using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class Character : MonoBehaviour
{
	private CharacterAnimator characterAnimator;
	private CharacterMovement characterMovement;

	private void Awake()
	{
		this.characterAnimator = (CharacterAnimator)GetComponent<CharacterAnimator>();
		this.characterMovement = (CharacterMovement)GetComponent<CharacterMovement>();
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
		get { return characterMovement; }
	}
}
