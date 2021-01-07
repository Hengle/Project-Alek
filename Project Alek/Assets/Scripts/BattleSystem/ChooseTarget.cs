using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Characters;
using Characters.Animations;
using Characters.PartyMembers;
using MEC;
using Sirenix.OdinInspector;

namespace BattleSystem
{
    public class ChooseTarget : MonoBehaviour, IGameEventListener<CharacterEvents>
    {
        public static int _targetOptions = 0;

        [ShowInInspector] public static bool _isMultiTarget;
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

        public static void GetItemCommand()
        {
            switch (_targetOptions)
            {
                case 0: targetOptionsCharacterType = CharacterType.Enemy;
                    break;
                case 1: targetOptionsCharacterType = CharacterType.PartyMember;
                    break;
                case 2: break;
            }
        }

        // This function is called from an onclick event attached to each character
        private void AddCommand()
        {
            // TODO: Account for revival items and other types
            if (BattleManager.Instance.usingItem)
            {
                if (thisUnitBase.Unit.status == Status.Dead) return;

                var notEnoughAP = character.Unit.currentAP - 2 < 0;
                if (notEnoughAP)
                {
                    var amountToBorrow = 2 - character.Unit.currentAP;
                    if (Unit.CanBorrow(amountToBorrow)) character.Unit.borrowAP(amountToBorrow);
                    else return;
                }

                character.Unit.anim.SetTrigger(AnimationHandler.ItemTrigger);
                BattleManager.Instance.inventoryInputManager.CurrentlySelectedInventorySlot.Use();
                BattleManager.Instance.choosingTarget = false;
                EventSystem.current.SetSelectedGameObject(null);
            }
            
            if (thisUnitBase.Unit.status == Status.Dead) return;
            
            character.Unit.currentTarget = thisUnitBase;
            character.Unit.commandActionName = className;
            character.Unit.commandActionOption = classOption;
            BattleManager.Instance.choosingTarget = false;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private static void AddMultiHitCommand() 
        {
            character.Unit.multiHitTargets = new List<UnitBase>();

            switch (_targetOptions)
            {
                case 0: foreach (var enemy in BattleManager.Instance._enemiesForThisBattle)
                    {
                        character.Unit.multiHitTargets.Add(enemy);
                        enemy.Unit.onDeselect?.Invoke();
                    }
                    break;
                
                case 1: foreach (var member in BattleManager.Instance._membersForThisBattle)
                    {
                        character.Unit.multiHitTargets.Add(member);
                        member.Unit.onDeselect?.Invoke();
                    }
                    break;
                
                case 2: break;
            }
            
            character.Unit.commandActionName = className;
            character.Unit.commandActionOption = classOption;
            BattleManager.Instance.choosingTarget = false;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private IEnumerator<float> WaitForMultiTargetConfirmation()
        {
            if (thisUnitBase.id != targetOptionsCharacterType) yield break;
            
            thisUnitBase.Unit.outline.enabled = true;
            thisUnitBase.Unit.button.interactable = false;

            BattleInput._inputModule.enabled = false;
            BattleInput._canOpenBox = false;

            while (_isMultiTarget)
            {
                if (BattleInput._controls.Menu.Confirm.triggered)
                {
                    AddMultiHitCommand();
                    break;
                }
            
                yield return Timing.WaitForOneFrame;
            }
            
            BattleInput._inputModule.enabled = true;
            BattleInput._canOpenBox = true;

            thisUnitBase.Unit.button.interactable = true;
            thisUnitBase.Unit.outline.enabled = false;
            _isMultiTarget = false;
        }
        
        public void OnGameEvent(CharacterEvents eventType)
        {
            switch (eventType._eventType)
            {
                case CEventType.CharacterTurn:
                    _isMultiTarget = false;
                    break;

                case CEventType.ChoosingTarget:
                    BattleManager.Instance.choosingTarget = true;
                    character = (PartyMember) eventType._character;

                    menuController = character.battlePanel.GetComponent<MenuController>();
                    menuController.SetTargetFirstSelected();
                    
                    if (_isMultiTarget) Timing.RunCoroutine(WaitForMultiTargetConfirmation());
                    break;
            }
        }

        private void OnEnable() => GameEventsManager.AddListener(this);
        
        private void OnDisable() => GameEventsManager.RemoveListener(this);
    }
}
