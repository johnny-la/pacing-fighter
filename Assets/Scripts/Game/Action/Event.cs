using UnityEngine;
using System.Collections;

namespace Brawler
{
	/// <summary>
	/// An event to perform once a behaviour is performed.
	/// </summary>
	[System.Serializable]
	public class Event
	{
		/// <summary>
		/// Specifies what will type of behaviour will be performed when the event is fired
		/// </summary>
		public EventType type;

		/// <summary>
		/// The action to perform once this event is fired. Only pertinent if 'type == EventType.PerformAction'
		/// </summary>
		public ActionScriptableObject actionToPerform;

		/// <summary>
		/// The basic action to perform once this event is fired. Only used if 'type == EventType.PerformBasicAction'
		/// </summary>
		public BasicAction basicActionToPerform;

		/// <summary>
		/// The slow motion to activate if this event's type is set to EventType.SlowMotion		/// </summary>
		public SlowMotion slowMotion = new SlowMotion();
	}

	/// <summary>
	/// Specifies what will happen when the event is performed
	/// </summary>
	public enum EventType
	{
		None,
		PerformAction,
		PerformBasicAction,
		SlowMotion
	}
}