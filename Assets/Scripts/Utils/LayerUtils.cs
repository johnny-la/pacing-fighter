using System;
using UnityEngine;

namespace Brawler
{
	public static class Layer
	{
		public static readonly int Player = LayerMask.NameToLayer ("Player"),
		                           Enemy = LayerMask.NameToLayer("Enemy"),
		                           HurtBox = LayerMask.NameToLayer ("HurtBox"),
		                           HitBox = LayerMask.NameToLayer ("HitBox"),
								   Obstacle = LayerMask.NameToLayer ("Obstacle");
	}
}
