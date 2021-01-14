using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using BattleSystem;
using Characters;
using UnityEngine.EventSystems;

namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "HealthBonusItem", menuName = "MoreMountains/InventoryEngine/HealthBonusItem", order = 1)]
	[Serializable]
	/// <summary>
	/// Demo class for a health item
	/// </summary>
	public class HealthBonusItem : InventoryItem 
	{
		[Header("Health Bonus")]
		public int HealthBonus;
		
		public override bool Use()
		{
			base.Use();
			UnitBase target = EventSystem.current.currentSelectedGameObject.GetComponent<Unit>().parent;
			target.Heal(HealthBonus);
			return true;
		}
	}
}