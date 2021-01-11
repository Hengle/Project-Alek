using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using Characters.PartyMembers;
using Kryz.CharacterStats;

namespace MoreMountains.InventoryEngine
{
	public enum WeaponDamageType { Pierce, Slice, Blunt, Null }
	[CreateAssetMenu(fileName = "WeaponItem", menuName = "MoreMountains/InventoryEngine/WeaponItem", order = 2)]
	[Serializable]
	public class WeaponItem : InventoryItem 
	{
		[Header("Weapon")]
		public Sprite WeaponSprite;

		public WeaponDamageType damageType;

		[Header("Weapon Stats")]
		[Range(0, 99)] public int weaponMight;
		[Range(0, 99)] public int magicMight;
		[Range(0, 99)] public int weaponAccuracy;
		[Range(0, 99)] public int weaponCriticalChance;
		
		[Header("Stat Bonuses")]
		//[Range(0,9999)] public int health;
		[Range(0,99)] public int strength;
		[Range(0,99)] public int magic;
		[Range(0,99)] public int accuracy;
		[Range(0,99)] public int initiative;
		[Range(0,99)] public int defense;
		[Range(0,99)] public int resistance;
		[Range(0,99)] public int criticalChance;
		
		[Header("Character Who Can Equip")]
		public PartyMember partyMember;

		public override bool Equip()
		{
			base.Equip();

			partyMember.weaponMight = weaponMight;
			partyMember.magicMight = magicMight;
			partyMember.weaponAccuracy = weaponAccuracy;
			partyMember.weaponCriticalChance = weaponCriticalChance;

			if (strength > 0) partyMember.strength.AddModifier(new StatModifier(strength, StatModType.Flat, this));
			if (magic > 0) partyMember.magic.AddModifier(new StatModifier(magic, StatModType.Flat, this));
			if (accuracy > 0) partyMember.accuracy.AddModifier(new StatModifier(accuracy, StatModType.Flat, this));
			if (initiative > 0) partyMember.initiative.AddModifier(new StatModifier(initiative, StatModType.Flat, this));
			if (defense > 0) partyMember.defense.AddModifier(new StatModifier(defense, StatModType.Flat, this));
			if (resistance > 0) partyMember.resistance.AddModifier(new StatModifier(resistance, StatModType.Flat, this));
			if (criticalChance > 0) partyMember.criticalChance.AddModifier(new StatModifier(criticalChance, StatModType.Flat, this));
			
			// For some stupid fucking reason this line of code causes the equipped weapon to be set to null when exiting play mode
			//partyMember.equippedWeapon = this;
			return true;
		}
		
		public override bool UnEquip()
		{
			base.UnEquip();
			Debug.Log("Weapon is unequipped");
			partyMember.weaponMight = 0;
			partyMember.magicMight = 0;
			partyMember.weaponAccuracy = 0;
			partyMember.weaponCriticalChance = 0;
			
			partyMember.health.RemoveAllModifiersFromSource(this);
			partyMember.strength.RemoveAllModifiersFromSource(this);
			partyMember.magic.RemoveAllModifiersFromSource(this);
			partyMember.accuracy.RemoveAllModifiersFromSource(this);
			partyMember.initiative.RemoveAllModifiersFromSource(this);
			partyMember.defense.RemoveAllModifiersFromSource(this);
			partyMember.resistance.RemoveAllModifiersFromSource(this);
			partyMember.criticalChance.RemoveAllModifiersFromSource(this);
            return true;
        }
	}
}