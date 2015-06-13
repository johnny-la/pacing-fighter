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

	public CharacterAnimator CharacterAnimator
	{
		get { return this.characterAnimator; }
	}
	
	public CharacterMovement CharacterMovement
	{
		get { return characterMovement; }
	}
}
