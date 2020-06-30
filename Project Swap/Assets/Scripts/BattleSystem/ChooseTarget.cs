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
        public UnitBase thisUnitBase;
        private static Type targetOptionsType;
        private static int classOption;
        private static string className;
        public static bool isMultiTarget;
        private static MenuController menuController;
        private static UnitBase memberCurrentlyChoosingTarget;
        private static PartyMember character;
        //private Unit thisUnit;

        private void Update()
        {
            if (!BattleManager.choosingTarget || !isMultiTarget || thisUnitBase.id != targetOptionsType) {
                thisUnitBase.Unit.button.interactable = true;
                return;
            }
            
            thisUnitBase.Unit.outline.enabled = true;
            thisUnitBase.Unit.button.interactable = false;
                
            if (BattleManager.inputModule.cancel.action.triggered) {
                thisUnitBase.Unit.button.interactable = true;
                isMultiTarget = false;
                return;
            }
                
            if (!BattleManager.inputModule.submit.action.triggered) return;
                
            AddMultiHitCommand();
            thisUnitBase.Unit.outline.enabled = false;
            thisUnitBase.Unit.button.interactable = true;
            isMultiTarget = false;


        }

        public static void ForThisMember(PartyMember member)
        {
            BattleManager.choosingTarget = true;
            character = member;
            memberCurrentlyChoosingTarget = member;
            
            menuController = character.battlePanel.GetComponent<MenuController>();
            menuController.SetTargetFirstSelected();
        }

        public static void GetCurrentCommand(string name, int option)
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
            if (!BattleManager.choosingTarget) return;
            
            switch (targetOptions)
            {
                case 0 when thisUnitBase.id == Type.Enemy && thisUnitBase.Unit.status != Status.Dead:
                    thisUnitBase.Unit.outline.enabled = true;
                    if (Input.GetMouseButtonUp(0)) AddCommand();
                    break;
                
                case 1 when thisUnitBase.id == Type.PartyMember && thisUnitBase.Unit != memberCurrentlyChoosingTarget.Unit:
                    thisUnitBase.Unit.outline.enabled = true;
                    if (Input.GetMouseButtonUp(0))
                    {
                        AddCommand();
                    }
                    break;
                
                case 2 when thisUnitBase.Unit != memberCurrentlyChoosingTarget.Unit && thisUnitBase.Unit.status != Status.Dead:
                    thisUnitBase.Unit.outline.enabled = true;
                    if (Input.GetMouseButtonUp(0)) AddCommand();
                    break;
            }
        }

        public void AddCommand()
        {
            if (thisUnitBase.Unit.status == Status.Dead) return;
            
            memberCurrentlyChoosingTarget.Unit.currentTarget = thisUnitBase;
            memberCurrentlyChoosingTarget.Unit.commandActionName = className;
            memberCurrentlyChoosingTarget.Unit.commandActionOption = classOption;
            BattleManager.choosingTarget = false;
        }

        private void AddMultiHitCommand() 
        {
            memberCurrentlyChoosingTarget.Unit.multiHitTargets = new List<UnitBase>();
            memberCurrentlyChoosingTarget.Unit.damageValueList = new List<int>();
            
            switch (targetOptions)
            {
                case 0: foreach (var enemy in BattleManager.enemiesForThisBattle) 
                        memberCurrentlyChoosingTarget.Unit.multiHitTargets.Add(enemy);
                    break;
                
                case 1: foreach (var member in BattleManager.membersForThisBattle)
                        memberCurrentlyChoosingTarget.Unit.multiHitTargets.Add(member);
                    break;
                
                case 2:
                    break;
            }
            memberCurrentlyChoosingTarget.Unit.commandActionName = className;
            memberCurrentlyChoosingTarget.Unit.commandActionOption = classOption;
            BattleManager.choosingTarget = false;
        }

        private void OnMouseExit() => thisUnitBase.Unit.outline.enabled = false;
    }
}
