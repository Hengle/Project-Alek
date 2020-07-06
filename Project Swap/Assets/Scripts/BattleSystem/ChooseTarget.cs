using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Characters;
using Characters.PartyMembers;
using Input;

namespace BattleSystem
{
    public class ChooseTarget : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        public static int _targetOptions = 0;
        public static bool _isMultiTarget;
        [HideInInspector] public UnitBase thisUnitBase;

        private static CharacterType targetOptionsCharacterType;
        private static MenuController menuController;
        private static PartyMember character;

        private static int classOption;
        private static string className;

        public static void GetCurrentCommand(string name, int option)
        {
            className = name;
            classOption = option;

            switch (_targetOptions)
            {
                case 0: targetOptionsCharacterType = CharacterType.Enemy;
                    break;
                case 1: targetOptionsCharacterType = CharacterType.PartyMember;
                    break;
                case 2: break;
            }
        }

        private void AddCommand()
        {
            if (thisUnitBase.Unit.status == Status.Dead) return;
            
            character.Unit.currentTarget = thisUnitBase;
            character.Unit.commandActionName = className;
            character.Unit.commandActionOption = classOption;
            BattleManager._choosingTarget = false;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private static void AddMultiHitCommand() 
        {
            character.Unit.multiHitTargets = new List<UnitBase>();
            character.Unit.damageValueList = new List<int>();
            
            switch (_targetOptions)
            {
                case 0: foreach (var enemy in BattleManager.EnemiesForThisBattle)
                    {
                        character.Unit.multiHitTargets.Add(enemy);
                        enemy.Unit.onDeselect?.Invoke();
                    }
                    break;
                
                case 1: foreach (var member in BattleManager.MembersForThisBattle)
                    {
                        character.Unit.multiHitTargets.Add(member);
                        member.Unit.onDeselect?.Invoke();
                    }
                    break;
                
                case 2: break;
            }
            
            character.Unit.commandActionName = className;
            character.Unit.commandActionOption = classOption;
            BattleManager._choosingTarget = false;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void Update()
        {
            // Need to update the last parameter if when/if I implement target option for everyone
            if (!BattleManager._choosingTarget || !_isMultiTarget || thisUnitBase.id != targetOptionsCharacterType) {
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

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType != CEventType.ChoosingTarget) return;
            
            BattleManager._choosingTarget = true;
            character = (PartyMember) eventType._character;

            menuController = character.battlePanel.GetComponent<MenuController>();
            menuController.SetTargetFirstSelected();
        }

        private void OnEnable() => GameEventsManager.AddListener(this);
        
        private void OnDisable() => GameEventsManager.RemoveListener(this);
    }
}
