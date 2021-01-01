using UnityEngine;
using Characters.Animations;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;

namespace Characters.PartyMembers
{
    [CreateAssetMenu(fileName = "New Party Member", menuName = "Character/Party Member")]
    public class PartyMember : UnitBase
    {
        [HideInInspector] public Animator actionPointAnim;
        [Range(0,4), VerticalGroup("Basic/Info")] public int positionInParty;

        [TabGroup("Tabs","Inventory")]
        public Inventory weaponInventory;
        [TabGroup("Tabs","Inventory")]
        public Inventory armorInventory;
        [TabGroup("Tabs","Inventory")]
        public InventoryItem equippedWeapon;

        [HideInInspector] public ScriptableObject battleOptionsPanel;
        [HideInInspector] public GameObject battlePanel;
        [HideInInspector] public GameObject inventoryDisplay;
        private GameObject unitGO;

        public int CurrentAP 
        {
            get => Unit.currentAP;
            set
            {
                Unit.currentAP = value < 0 ? 0 : value;
                actionPointAnim.SetInteger(AnimationHandler.APVal, Unit.currentAP);
            }
        }

        public override void Heal(float amount)
        {
            CurrentHP += (int) amount;
        }
        
        public bool SetAI() => true;
    }
}
