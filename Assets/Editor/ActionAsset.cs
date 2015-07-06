using UnityEngine;
using UnityEditor;

public class ActionAsset 
{
	[MenuItem("Assets/Create/Brawler/Action")]
	public static void CreateAsset()
	{
		CustomAssetUtility.CreateAsset<ActionScriptableObject>();
	}
}
