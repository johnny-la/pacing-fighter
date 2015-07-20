using UnityEngine;
using System.Collections;

public static class Vector2Extensions 
{
	/// <summary>
	/// Returns a new Vector2 with the same direction as the given vector, but with the new given magnitude.
	/// Does not mutate the given vector.
	/// </summary>
	public static Vector2 SetMagnitude(this Vector2 vector, float magnitude)
	{
		vector.Normalize ();
		// Rescale the vector to the desired magnitude
		vector *= magnitude;

		return vector;
	}

	/// <summary>
	/// Changes the angle of the given vector. Returns the vector. The angle must be given in RADIANS
	/// </summary>
	public static Vector2 SetAngle(this Vector2 vector, float angle)
	{
		float magnitude = vector.magnitude;
		vector.Set(magnitude * Mathf.Sin(angle), magnitude * Mathf.Cos(angle));

		return vector;
	}

	/// <summary>
	/// If this vector's magnitude is greater than the given magnitude, the vector's length is clamped to the given length.
	/// Otherwise, the vector is left unchanged. Note: the given vector not mutated. A truncated copy of the Vector2 is returned.
	/// </summary>
	public static Vector2 Truncate(this Vector2 vector, float maxMagnitude)
	{
		// Clamp the vector's magnitude
		if(vector.magnitude > maxMagnitude)
			vector = vector.SetMagnitude (maxMagnitude);

		return vector;
	}
}

