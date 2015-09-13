using UnityEngine;
using System.Collections;

/// <summary>
/// The settings for an epoch/wave. Each epoch represents a wave of enemies. During each epoch, the AI Director
/// transitions between the build up, sustain peak, peak fade and relax phases in order to deliver an unpredictable
/// and dramatic gameplay experience to the user. This data container stores the various AI settings used for an epoch
/// </summary>
[System.Serializable]
public class EpochSettings 
{
	/// <summary>
	/// The max number of enemies that can be in the level during an epoch
	/// </summary>
	public int maxEnemies;

	/// <summary>
	/// The amount of enemies per second that spawn in the build-up phase.
	/// </summary>
	[Tooltip("The amount of enemies per second that spawn in the build-up phase.")]
	public float buildUpSpawnRate;

	/// <summary>
	/// The amount of enemies per second that are de-spawned in the peak-fade phase.
	/// </summary>
	[Tooltip("The amount of enemies per second that are de-spawned in the peak-fade phase.")]
	public float peakFadeDespawnRate;

	/// <summary>
	/// The amount of time for which the AI director sustains the peak game intensity (Left4Dead = 3-5 seconds).
	/// </summary>
	public Range sustainPeakDuration = new Range();
	
	/// <summary>
	/// The duration of the relaxing phase. This is the amount of time for which enemies become sparse and weak. 
	/// </summary>
	public Range relaxDuration = new Range();
	
	/// <summary>
	/// The minimum game intensity required for the game to be at its peak difficulty. The peak difficulty is sustained for 'sustainPeakDuration'
	/// seconds and  then dies down.
	/// </summary>
	public float peakIntensityThreshold;
	
	/// <summary>
	/// The game intensity must be lower than this value to enter 'Relax' mode. Ensures that the difficulty is ramped down to an appropriate
	/// value before the game enters 'Relax' mode, and the user gets an adequate amount of time to relax.
	/// </summary>
	public float relaxUpperBound;

	/// <summary>
	/// Creates a new EpochSettings instance, copying the values from the given instance.
	/// </summary>
	public EpochSettings(EpochSettings other)
	{
		// Copies the settings from the given instance
		Set (other);
	}

	/// <summary>
	/// Copies the settings from the given instance.
	/// </summary>
	public void Set(EpochSettings other)
	{
		// Copy the settings from the given instance
		maxEnemies 				= other.maxEnemies;
		buildUpSpawnRate 		= other.buildUpSpawnRate;
		peakFadeDespawnRate 	= other.peakFadeDespawnRate;
		sustainPeakDuration.Set (other.sustainPeakDuration);
		relaxDuration.Set (other.relaxDuration);
		peakIntensityThreshold 	= other.peakIntensityThreshold;
		relaxUpperBound 		= other.relaxUpperBound;
	}

	public override string ToString()
	{
		string desc = "";

		desc += "Max enemies: " + maxEnemies + " ";
		desc += "Build-up spawn rate: " + buildUpSpawnRate + " ";
		desc += "Sustain peak duration: " + sustainPeakDuration.ToString () + " ";
		desc += "Relax duration: " + relaxDuration.ToString () + " ";
		desc += "Peak intensity threshold: " + peakIntensityThreshold + " ";
		desc += "Relax upper bounds: " + relaxUpperBound + " ";

		return desc;
	}
}
