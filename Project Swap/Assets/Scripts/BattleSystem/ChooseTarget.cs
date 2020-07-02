using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Characters;
using Characters.PartyMembers;
using Input;
using Type = Characters.Type;

namespace BattleSystem
{
    public class ChooseTarget : MonoBehaviour
    {
        public static int _targetOptions = 0;
        public static bool _isMultiTarget;
        [HideInInspector] public UnitBase thisUnitBase;

        private static Type targetOptionsType;
        private static MenuController menuController;
        private static UnitBase memberCurrentlyChoosingTarget;
        private static PartyMember character;

        private static int classOption;
        private static string className;

        private void Update()
        {
            if (!BattleManager._choosingTarget || !_isMultiTarget || thisUnitBase.id != targetOptionsType) {
                thisUnitBase.Unit.button.interactable = true;
                return;
            }
            
            thisUnitBase.Unit.outline.enabled = true;
            thisUnitBase.Unit.button.interactable = false;
                
            if (BattleInputManager._inputModule.cancel.action.triggered) {
                thisUnitBase.Unit.button.interactable = true;
                _isMultiTarget = false;
                return;
            }
                
            if (!BattleInputManager._inputModule.submit.action.triggered) return;
                
            AddMultiHitCommand();
            thisUnitBase.Unit.outline.enabled = false;
            thisUnitBase.Unit.button.interactable = true;
            _isMultiTarget = false;
        }

        public static void ForThisMember(PartyMember member)
        {
            BattleManager._choosingTarget = true;
            character = member;
            memberCurrentlyChoosingTarget = member;
            
            menuController = character.battlePanel.GetComponent<MenuController>();
            menuController.SetTargetFirstSelected();
        }

        public static void GetCurrentCommand(string name, int option)
        {
            className = name;
            classOption = option;

            switch (_targetOptions)
            {
                case 0: targetOptionsType = Type.Enemy;
                    break;
                case 1: targetOptionsType = Type.PartyMember;
                    break;
                case 2: targetOptionsType = Type.All;
                    break;
            }
        }

        public void AddCommand()
        {
            if (thisUnitBase.Unit.status == Status.Dead) return;
            
            memberCurrentlyChoosingTarget.Unit.currentTarget = thisUnitBase;
            memberCurrentlyChoosingTarget.Unit.commandActionName = className;
            memberCurrentlyChoosingTarget.Unit.commandActionOption = classOption;
            BattleManager._choosingTarget = false;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private static void AddMultiHitCommand() 
        {
            memberCurrentlyChoosingTarget.Unit.multiHitTargets = new List<UnitBase>();
            memberCurrentlyChoosingTarget.Unit.damageValueList = new List<int>();
            
            switch (_targetOptions)
            {
                case 0: foreach (var enemy in BattleManager._enemiesForThisBattle) 
                        memberCurrentlyChoosingTarget.Unit.multiHitTargets.Add(enemy);
                    break;
                case 1: foreach (var member in BattleManager._membersForThisBattle)
                        memberCurrentlyChoosingTarget.Unit.multiHitTargets.Add(member);
                    break;
                case 2: break;
            }
            
            memberCurrentlyChoosingTarget.Unit.commandActionName = className;
            memberCurrentlyChoosingTarget.Unit.commandActionOption = classOption;
            BattleManager._choosingTarget = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
