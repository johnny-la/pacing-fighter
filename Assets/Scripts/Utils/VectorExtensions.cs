using UnityEngine;
using System.Collections;

public static class Vector2Extensions 
{
	public static Vector2 SetMagnitude(this Vector2 vector, float magnitude)
	{
		vector.Normalize ();
		
		// Detemine the angle of the vector
		float angle = Mathf.Atan2(vector.y, vector.x);
		
		// Rescale the vector to the desired magnitude
		vector.Set (magnitude * Mathf.Cos(angle), magnitude * Mathf.Sin(angle));

		return vector;
	}
}

