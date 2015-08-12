using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Updates the health slider to match the player's health
/// </summary>
public class PlayerAnxietySlider : MonoBehaviour 
{
	/** Cache the slider component for efficient access */
	private Slider slider;

	// Use this for initialization
	void Start () 
	{
		// Cache the slider component for efficient access
		slider = GetComponent<Slider>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		slider.value = AIDirector.Instance.GameIntensity;
	}
}
