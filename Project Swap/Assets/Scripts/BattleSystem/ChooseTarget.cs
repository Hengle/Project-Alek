using UnityEngine;
using Characters;
using UnityEngine.InputSystem;

// Attach this script to all enemies and party members
namespace BattleSystem
{
    public class ChooseTarget : MonoBehaviour
    {
        public static int targetOptions = 0; // 1 = party member, 0 = enemy, 2 = all
        private static int classOption;
        private static string className;
        private static bool isSwapOption;
        private static MenuController menuController;
        private static Unit memberCurrentlyChoosingTarget;
        private static PartyMember character;
        private Unit thisUnit;

        private void Start() => thisUnit = GetComponent<Unit>();

        public static void GetPartyMember(PartyMember member)
        {
            BattleHandler.choosingTarget = true;
            character = member;
            memberCurrentlyChoosingTarget = member.unit;
            
            menuController = character.battlePanel.GetComponent<MenuController>();
            menuController.SetTargetFirstSelected();
        }

        public static void GetCurrentCommand(string name, int option, bool isSwap)
        {
            className = name;
            classOption = option;
            isSwapOption = isSwap;
        }
        
        private void OnMouseOver()
        {
            if (!BattleHandler.choosingTarget) return;
            
            switch (targetOptions)
            {
                case 0 when thisUnit.id == 0 && thisUnit.status != Status.Dead:
                    thisUnit.outline.enabled = true;
                    if (Input.GetMouseButtonUp(0)) AddCommand();
                    break;
                
                case 1 when thisUnit.id == 1 && thisUnit != memberCurrentlyChoosingTarget:
                    thisUnit.outline.enabled = true;
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (isSwapOption) AddSwapCommand();
                        else AddCommand();
                    }
                    break;
                
                case 2 when thisUnit != memberCurrentlyChoosingTarget && thisUnit.status != Status.Dead:
                    thisUnit.outline.enabled = true;
                    if (Input.GetMouseButtonUp(0)) AddCommand();
                    break;
            }
        }

        public void AddCommand()
        {
            if (isSwapOption && memberCurrentlyChoosingTarget != thisUnit) { AddSwapCommand(); return; }
            if (isSwapOption && memberCurrentlyChoosingTarget == thisUnit) return;
            if (thisUnit.status == Status.Dead) return;
            
            memberCurrentlyChoosingTarget.currentTarget = thisUnit;
            memberCurrentlyChoosingTarget.commandActionName = className;
            memberCurrentlyChoosingTarget.commandActionOption = classOption;
            BattleHandler.choosingTarget = false;
        }

        private void AddSwapCommand()
        {
            if (thisUnit.status == Status.Dead) return;
            isSwapOption = false;
            memberCurrentlyChoosingTarget.isSwapping = true;
            
            BattleHandler.partyHasChosenSwap = true;
            BattleHandler.partyMemberWhoChoseSwap = character;
            BattleHandler.partySwapTarget = thisUnit;
            BattleHandler.choosingTarget = false;
        }

        private void OnMouseExit() => thisUnit.outline.enabled = false;
    }
}
