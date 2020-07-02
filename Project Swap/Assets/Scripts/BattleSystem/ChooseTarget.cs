﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Characters;
using Characters.PartyMembers;
using Input;
using Type = Characters.Type;

namespace BattleSystem
{
    public class ChooseTarget : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        public static int _targetOptions = 0;
        public static bool _isMultiTarget;
        [HideInInspector] public UnitBase thisUnitBase;

        private static Type targetOptionsType;
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
                case 0: targetOptionsType = Type.Enemy;
                    break;
                case 1: targetOptionsType = Type.PartyMember;
                    break;
                case 2: targetOptionsType = Type.All;
                    break;
            }
        }

        private static void AddMultiHitCommand() 
        {
            character.Unit.multiHitTargets = new List<UnitBase>();
            character.Unit.damageValueList = new List<int>();
            
            switch (_targetOptions)
            {
                case 0: foreach (var enemy in BattleManager._enemiesForThisBattle) 
                        character.Unit.multiHitTargets.Add(enemy);
                    break;
                case 1: foreach (var member in BattleManager._membersForThisBattle)
                        character.Unit.multiHitTargets.Add(member);
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

        private void AddCommand()
        {
            if (thisUnitBase.Unit.status == Status.Dead) return;
            
            character.Unit.currentTarget = thisUnitBase;
            character.Unit.commandActionName = className;
            character.Unit.commandActionOption = classOption;
            BattleManager._choosingTarget = false;
            EventSystem.current.SetSelectedGameObject(null);
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
