﻿using Characters.Abilities;
using UnityEngine;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;

namespace Characters.PartyMembers
{
    [CreateAssetMenu(fileName = "New Party Member", menuName = "Character/Party Member")]
    public class PartyMember : UnitBase
    {
        [Range(0,4), VerticalGroup("Basic/Info")] public int positionInParty;

        [TabGroup("Tabs","Inventory")]
        public Inventory weaponInventory;
        [TabGroup("Tabs","Inventory")]
        public Inventory armorInventory;
        [TabGroup("Tabs","Inventory")] [SerializeField]
        public WeaponItem equippedWeapon;

        [TabGroup("Tabs", "Abilities")] [InlineEditor]
        public Ability specialAttack;

        [HideInInspector] public ScriptableObject battleOptionsPanel;
        [HideInInspector] public GameObject battlePanel;
        [HideInInspector] public GameObject inventoryDisplay;
        
        public CanvasGroup Container => inventoryDisplay.GetComponent<CanvasGroup>();
        public InventoryDisplay InventoryDisplay => inventoryDisplay.GetComponentInChildren<InventoryDisplay>();

        public override void Heal(float amount)
        {
            CurrentHP += (int) amount;
        }
        
        public bool SetAI() => true;
    }
}
