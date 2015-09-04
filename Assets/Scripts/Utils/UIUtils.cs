using UnityEngine;
using System.Collections;

/// <summary>
/// User interface utilities.
/// </summary>
public static class UIUtils
{
	/// <summary>
	/// Converts the given world coordinates into canvas coordinates, using the given canvas
	/// </summary>
	public static Vector3 WorldToCanvasPosition(RectTransform canvasRect, Vector3 worldPosition)
	{
		// Convert the world position into a viewport position
		Vector2 viewportPosition = GameManager.Instance.GameCamera.Camera.WorldToViewportPoint (worldPosition);
		
		// Convert the world position into canvas coordinates
		Vector2 canvasPosition = new Vector2(
			((viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x*0.5f)),
			((viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y*0.5f))
		);
		
		// Return the world position, converted to canvas coordinates
		return canvasPosition;
	}
}

