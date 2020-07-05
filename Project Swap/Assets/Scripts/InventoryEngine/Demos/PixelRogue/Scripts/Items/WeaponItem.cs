using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using Characters.PartyMembers;
using UnityEngine.SceneManagement;

namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "WeaponItem", menuName = "MoreMountains/InventoryEngine/WeaponItem", order = 2)]
	[Serializable]
	public class WeaponItem : InventoryItem 
	{
		[Header("Weapon")]
		// the sprite to use to show the weapon when equipped
		public Sprite WeaponSprite;

		[Header("Weapon Stats")]
		[Range(0, 99)] public int weaponMight;
		[Range(0, 99)] public int magicMight;
		[Range(0, 99)] public int weaponAccuracy;
		[Range(0, 99)] public int weaponCriticalChance;
		
		[Header("Stat Bonuses")]
		[Range(0,99999)] public int health;
		[Range(0,99)] public int strength;
		[Range(0,99)] public int magic;
		[Range(0,99)] public int accuracy;
		[Range(0,99)] public int initiative;
		[Range(0,99)] public int defense;
		[Range(0,99)] public int resistance;
		[Range(0,99)] public int criticalChance;
		
		[Header("Character Who Can Equip")]
		public PartyMember partyMember;

		/// <summary>
		/// What happens when the object is used 
		/// </summary>
		
		public override bool Equip()
		{
			base.Equip();
			Debug.Log("Weapon is equipped");
			// partyMember.health += health;
			// partyMember.strength += strength;
			// partyMember.magic += magic;
			// partyMember.accuracy += accuracy;
			// partyMember.initiative += initiative;
			// partyMember.defense += defense;
			// partyMember.resistance += resistance;
			// partyMember.criticalChance += criticalChance;
			//
			// partyMember.weaponMight += weaponMight;
			// partyMember.magicMight += magicMight;
			// partyMember.weaponAccuracy += weaponAccuracy;
			// partyMember.weaponCriticalChance += weaponCriticalChance;
			//
			// partyMember.equippedWeapon = this;
			//
			// var activeScene = SceneManager.GetActiveScene();
			// if (activeScene.name != "Battle") return true;
			//
			// partyMember.Unit.currentHP += health;
			// partyMember.Unit.currentStrength += strength;
			// partyMember.Unit.currentMagic += magic;
			// partyMember.Unit.currentAccuracy += accuracy;
			// partyMember.Unit.currentInitiative += initiative;
			// partyMember.Unit.currentDefense += defense;
			// partyMember.Unit.currentResistance += resistance;
			// partyMember.Unit.currentCrit += criticalChance;
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
			// partyMember.health -= health;
			// partyMember.strength -= strength;
			// partyMember.magic -= magic;
			// partyMember.accuracy -= accuracy;
			// partyMember.initiative -= initiative;
			// partyMember.defense -= defense;
			// partyMember.resistance -= resistance;
			// partyMember.criticalChance -= criticalChance;
			//
			// partyMember.weaponMight -= weaponMight;
			// partyMember.magicMight -= magicMight;
			// partyMember.weaponAccuracy -= weaponAccuracy;
			// partyMember.weaponCriticalChance -= weaponCriticalChance;
			//
			// partyMember.equippedWeapon = null;
			//
			// var activeScene = SceneManager.GetActiveScene();
			// if (activeScene.name != "Battle") return true;
			//
			// partyMember.Unit.currentHP -= health;
			// partyMember.Unit.currentStrength -= strength;
			// partyMember.Unit.currentMagic -= magic;
			// partyMember.Unit.currentAccuracy -= accuracy;
			// partyMember.Unit.currentInitiative -= initiative;
			// partyMember.Unit.currentDefense -= defense;
			// partyMember.Unit.currentResistance -= resistance;
			// partyMember.Unit.currentCrit -= criticalChance;
			//InventoryDemoGameManager.Instance.Player.SetWeapon(null,this);
            return true;
        }
		
	}
}