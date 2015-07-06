using UnityEngine;
using System.Collections;

/// <summary>
/// An action which can be serialized into an asset
/// </summary>
[System.Serializable]
public class ActionScriptableObject:ScriptableObject
{
	/// <summary>
	/// The data container which governs this action's properties
	/// </summary>
	public Action action;
	
	/// <summary>
	/// The data container which governs this action's properties
	/// </summary>
	/*public Action Action
	{
		get { return action; }
		set { this.action = value; }
	}*/
}