using UnityEngine;
using System;
using Audio;
using BattleSystem;

namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "HealthBonusItem", menuName = "MoreMountains/InventoryEngine/HealthBonusItem", order = 1)]
	[Serializable]
	public class HealthBonusItem : InventoryItem 
	{
		[Header("Health Bonus")]
		public int HealthBonus;
		
		public override bool Use()
		{
			base.Use();
			var target = BattleEngine.Instance.activeUnit.CurrentTarget;
			target.Heal(HealthBonus);
			AudioController.Instance.PlayAudio(CommonAudioTypes.Instance.heal);
			return true;
		}
	}
}