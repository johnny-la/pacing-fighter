using System;
using UnityEngine;

public class ApplicationInitializer : MonoBehaviour
{
	public const float DEFAULT_SCREEN_WIDTH = 480f;
	public const float DEFAULT_SCREEN_HEIGHT = 320f;
	public void Awake()
	{
		float a = (float)Screen.width / DEFAULT_SCREEN_WIDTH;
		float b = (float)Screen.height / DEFAULT_SCREEN_HEIGHT;
		if (Mathf.Max(a, b) >= 4f)
		{
			tk2dSystem.CurrentPlatform = "4x";
			Debug.Log("4x textures chosen");
		}
		else if (Mathf.Max(a, b) >= 2f)
		{
			tk2dSystem.CurrentPlatform = "2x";
			Debug.Log("2x textures chosen");
		}
		else
		{
			tk2dSystem.CurrentPlatform = "1x";
			Debug.Log("1x textures chosen");
		}
		Application.LoadLevel(1);
	}
}
