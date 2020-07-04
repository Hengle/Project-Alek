using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "WeaponItem", menuName = "MoreMountains/InventoryEngine/WeaponItem", order = 2)]
	[Serializable]
	public class WeaponItem : InventoryItem 
	{
		[Header("Weapon")]
		// the sprite to use to show the weapon when equipped
		public Sprite WeaponSprite;

		/// <summary>
		/// What happens when the object is used 
		/// </summary>
		public override bool Equip()
		{
			base.Equip();
			Debug.Log("Weapon is equipped");
			//InventoryDemoGameManager.Instance.Player.SetWeapon(WeaponSprite,this);
            return true;
		}

		/// <summary>
		/// What happens when the object is used 
		/// </summary>
		public override bool UnEquip()
		{
			base.UnEquip();
			Debug.Log("Weapon is unequipped");
			//InventoryDemoGameManager.Instance.Player.SetWeapon(null,this);
            return true;
        }
		
	}
}