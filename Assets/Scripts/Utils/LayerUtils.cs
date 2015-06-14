using System;
using UnityEngine;

namespace Brawler
{
	public static class Layer
	{
		public static readonly int PLAYER = LayerMask.NameToLayer ("Player"),
		                           ENEMY = LayerMask.NameToLayer("Enemy");
	}
}
