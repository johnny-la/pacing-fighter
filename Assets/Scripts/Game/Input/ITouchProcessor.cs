using System;
using UnityEngine;

public interface ITouchProcessor
{
	/// <summary>
	/// Called when the user clicks the screen. Accepts the clicked GameObject (can be null).
	/// </summary>
	void OnClick(TouchInfo touch, GameObject gameObject);

	void LongPressUp(TouchInfo touch);
	void ShortPressDown(TouchInfo touch);
	void LongPress(TouchInfo touch);

	/// <summary>
	/// Called when the user swiped the screen. Accepts the swiped GameObject.
	/// Null if no object was swiped
	/// </summary>
	void OnSwipe(TouchInfo touch, GameObject gameObject);
}
