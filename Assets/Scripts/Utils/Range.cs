using System;

/// <summary>
/// Specifies a range between two floating-point values
/// </summary>
public struct Range
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

	public string ToString()
	{
		return "Min: " + min + ", Max: " + max;
	}
}

