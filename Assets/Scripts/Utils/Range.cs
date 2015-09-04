using System;

/// <summary>
/// Specifies a range between two floating-point values
/// </summary>
[System.Serializable]
public class Range
{
	/// <summary>
	/// The minimum value for the range.
	/// </summary>
	public float min;
	/// <summary>
	/// The maximum value for the range
	/// </summary>
	public float max;

	/// <summary>
	/// Creates a Range instance with a default min and max value of zero.
	/// </summary>
	public Range() : this(0,0)
	{
	}

	/// <summary>
	/// Creates a Range struct with the specified min and max
	/// </summary>
	public Range(float min, float max)
	{
		this.min = min;
		this.max = max;
	}

	/// <summary>
	/// Set the min and max of this range.
	/// </summary>
	public void Set(float min, float max)
	{
		this.min = min;
		this.max = max;
	}

	/// <summary>
	/// Copies the range values from the given range instance.
	/// </summary>
	public void Set(Range other)
	{
		this.min = other.min;
		this.max = other.max;
	}

	/// <summary>
	/// Returns a random value between the min and max of this range.
	/// </summary>
	/// <returns>The value.</returns>
	public float RandomValue()
	{
		// Return a random value between min and max
		return UnityEngine.Random.Range (min, max);
	}

	public string ToString()
	{
		return "[" + min + ", " + max + "]";
	}
}

