using UnityEngine;
using UnityEditor;

public class MoveInfoAsset 
{
	[MenuItem("Assets/Create/Brawler/Move")]
	public static void CreateAsset()
	{
		CustomAssetUtility.CreateAsset<MoveInfo>();
	}
}
