using System.Collections.Generic;
using UnityEngine;
using Characters;
using Characters.PartyMembers;
using Type = Characters.Type;

namespace BattleSystem
{
    public class ChooseTarget : MonoBehaviour
    {
        public static int targetOptions = 0; // 1 = party member, 0 = enemy, 2 = all
        private static Type targetOptionsType;
        private static int classOption;
        private static string className;
        public static bool isMultiTarget;
        private static MenuController menuController;
        private static Unit memberCurrentlyChoosingTarget;
        private static PartyMember character;
        private Unit thisUnit;

        private void Start() => thisUnit = GetComponent<Unit>();

        private void Update()
        {
            if (!BattleHandler.choosingTarget || !isMultiTarget || thisUnit.id != targetOptionsType) {
                thisUnit.button.interactable = true;
                return;
            }
            
            thisUnit.outline.enabled = true;
            thisUnit.button.interactable = false;
                
            if (BattleHandler.inputModule.cancel.action.triggered) {
                thisUnit.button.interactable = true;
                isMultiTarget = false;
                return;
            }
                
            if (!BattleHandler.inputModule.submit.action.triggered) return;
                
            AddMultiHitCommand();
            thisUnit.outline.enabled = false;
            thisUnit.button.interactable = true;
            isMultiTarget = false;


        }

        public static void ForThisMember(PartyMember member)
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

            switch (targetOptions)
            {
                case 0: targetOptionsType = Type.Enemy;
                    break;
                case 1: targetOptionsType = Type.PartyMember;
                    break;
            }
        }
        
        private void OnMouseOver()
        {
            if (!BattleHandler.choosingTarget) return;
            
            switch (targetOptions)
            {
                case 0 when thisUnit.id == Type.Enemy && thisUnit.status != Status.Dead:
                    thisUnit.outline.enabled = true;
                    if (Input.GetMouseButtonUp(0)) AddCommand();
                    break;
                
                case 1 when thisUnit.id == Type.PartyMember && thisUnit != memberCurrentlyChoosingTarget:
                    thisUnit.outline.enabled = true;
                    if (Input.GetMouseButtonUp(0))
                    {
                        AddCommand();
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
            if (thisUnit.status == Status.Dead) return;
            
            memberCurrentlyChoosingTarget.currentTarget = thisUnit;
            memberCurrentlyChoosingTarget.commandActionName = className;
            memberCurrentlyChoosingTarget.commandActionOption = classOption;
            BattleHandler.choosingTarget = false;
        }

        private void AddMultiHitCommand() 
        {
            memberCurrentlyChoosingTarget.multiHitTargets = new List<UnitBase>();
            memberCurrentlyChoosingTarget.damageValueList = new List<int>();
            
            switch (targetOptions)
            {
                case 0: foreach (var enemy in BattleHandler.enemiesForThisBattle) 
                        memberCurrentlyChoosingTarget.multiHitTargets.Add(enemy);
                    break;
                
                case 1: foreach (var member in BattleHandler.membersForThisBattle)
                        memberCurrentlyChoosingTarget.multiHitTargets.Add(member);
                    break;
                
                case 2:
                    break;
            }
            memberCurrentlyChoosingTarget.commandActionName = className;
            memberCurrentlyChoosingTarget.commandActionOption = classOption;
            BattleHandler.choosingTarget = false;
        }

        private void OnMouseExit() => thisUnit.outline.enabled = false;
    }
}
