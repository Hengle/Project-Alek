using System;
using UnityEngine;
using Characters.Animations;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;

namespace Characters.PartyMembers
{
    [CreateAssetMenu(fileName = "New Party Member", menuName = "Character/Party Member")]
    public class PartyMember : UnitBase
    {
        //[HideInInspector] public Animator actionPointAnim;
        [Range(0,4), VerticalGroup("Basic/Info")] public int positionInParty;

        [TabGroup("Tabs","Inventory")]
        public Inventory weaponInventory;
        [TabGroup("Tabs","Inventory")]
        public Inventory armorInventory;
        [TabGroup("Tabs","Inventory")] [SerializeField]
        public WeaponItem equippedWeapon;

        [HideInInspector] public ScriptableObject battleOptionsPanel;
        [HideInInspector] public GameObject battlePanel;
        [HideInInspector] public GameObject inventoryDisplay;

        private GameObject unitGO;
        
        public override void Heal(float amount)
        {
            CurrentHP += (int) amount;
        }
        
        public bool SetAI() => true;
    }
}
