using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Characters;
using Characters.Abilities;
using Characters.Animations;
using Characters.PartyMembers;
using MEC;
using MoreMountains.InventoryEngine;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace BattleSystem
{
    public class ChooseTarget : MonoBehaviour, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        public static int _targetOptions = 0;

        [SerializeField] private CharacterGameEvent characterTurnEvent;
        [SerializeField] private CharacterGameEvent chooseTargetEvent;
        
        [ShowInInspector] public static bool _isMultiTarget;
        [HideInInspector] public UnitBase thisUnitBase;
        private Selectable button;
        [SerializeField] private Selectable selectOnLeft;
        [SerializeField] private Selectable selectOnRight;
        [SerializeField] private Selectable selectOnUp;
        [SerializeField] private Selectable selectOnDown;

        private static TargetOptions targetOptionsCharacterType;
        private static PartyMember character;
        public static InventoryItem _currentlySelectedItem;

        private static int classOption;
        private static string className;

        private void Awake() => button = GetComponent<Selectable>();
        
        public void Setup()
        {
            var navigation = button.navigation;
            selectOnLeft = navigation.selectOnLeft;
            selectOnRight = navigation.selectOnRight;

            selectOnUp = navigation.selectOnUp;
            selectOnDown = navigation.selectOnDown;
            
            button.navigation = navigation;
        }

        private void SetNavigation()
        {
            var navigation = button.navigation;
            if (navigation.selectOnLeft) selectOnLeft = navigation.selectOnLeft;
            if (navigation.selectOnRight) selectOnRight = navigation.selectOnRight;
            if (navigation.selectOnUp) selectOnUp = navigation.selectOnUp;
            if (navigation.selectOnDown) selectOnDown = navigation.selectOnDown;

            switch (targetOptionsCharacterType)
            {
                case TargetOptions.Enemies: navigation.selectOnLeft = null;
                    button.navigation = navigation;
                    break;
                case TargetOptions.PartyMembers: navigation.selectOnRight = null;
                    button.navigation = navigation;
                    break;
                case TargetOptions.Both:
                    navigation.selectOnLeft = selectOnLeft;
                    navigation.selectOnRight = selectOnRight;
                    button.navigation = navigation;
                    break;
                case TargetOptions.Self:
                    navigation.selectOnLeft = null;
                    navigation.selectOnRight = null;
                    navigation.selectOnUp = null;
                    navigation.selectOnDown = null;
                    break;
            }
        }

        public static void GetCurrentCommand(string name, int option)
        {
            className = name;
            classOption = option;

            targetOptionsCharacterType = (TargetOptions) _targetOptions;
        }

        public static void GetItemCommand()
        {
            targetOptionsCharacterType = (TargetOptions) _targetOptions;
        }

        // This function is called from an onclick event attached to each character
        private void AddCommand()
        {
            // TODO: Account for revival items and other types
            if (BattleEngine.Instance.usingItem)
            {
                if (thisUnitBase.Unit.status == Status.Dead && _currentlySelectedItem as RevivalItem)
                {
                    goto try_use_item;
                }
                if (thisUnitBase.Unit.status == Status.Dead) return;

                try_use_item:
                var notEnoughAP = character.Unit.currentAP - 2 < 0;
                if (notEnoughAP)
                {
                    var amountToBorrow = 2 - character.Unit.currentAP;
                    if (Unit.CanBorrow(amountToBorrow)) character.Unit.borrowAP(amountToBorrow);
                    else return;
                }

                character.Unit.anim.SetTrigger(AnimationHandler.ItemTrigger);
                BattleEngine.Instance.inventoryInputManager.CurrentlySelectedInventorySlot.Use();
                BattleEngine.Instance.choosingTarget = false;
                EventSystem.current.SetSelectedGameObject(null);
            }
            
            if (thisUnitBase.Unit.status == Status.Dead) return;
            
            character.Unit.currentTarget = thisUnitBase;
            character.Unit.commandActionName = className;
            character.Unit.commandActionOption = classOption;

            if (character.CurrentAbility && character.CurrentAbility.endTurnOnUse)
                BattleEngine.Instance.endTurnAfterCommand = true;
            BattleEngine.Instance.choosingTarget = false;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private static void AddMultiHitCommand() 
        {
            character.Unit.multiHitTargets = new List<UnitBase>();

            switch (_targetOptions)
            {
                case 0: foreach (var enemy in BattleEngine.Instance._enemiesForThisBattle)
                    {
                        character.Unit.multiHitTargets.Add(enemy);
                        enemy.Unit.onDeselect?.Invoke();
                    }
                    break;
                
                case 1: foreach (var member in BattleEngine.Instance._membersForThisBattle)
                    {
                        character.Unit.multiHitTargets.Add(member);
                        member.Unit.onDeselect?.Invoke();
                    }
                    break;
                
                case 2: break;
            }
            
            character.Unit.commandActionName = className;
            character.Unit.commandActionOption = classOption;
            BattleEngine.Instance.choosingTarget = false;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private IEnumerator<float> WaitForMultiTargetConfirmation()
        {
            if ((int) thisUnitBase.id != (int) targetOptionsCharacterType) yield break;
            
            thisUnitBase.Unit.outline.enabled = true;
            thisUnitBase.Unit.button.interactable = false;
            thisUnitBase.Unit.onSelect?.Invoke();

            BattleInput._inputModule.enabled = false;
            BattleInput._canOpenBox = false;

            while (_isMultiTarget)
            {
                if (BattleInput._controls.Battle.Confirm.triggered)
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
            thisUnitBase.Unit.onDeselect?.Invoke();
        }

        private void OnEnable()
        {
            characterTurnEvent.AddListener(this);
            chooseTargetEvent.AddListener(this);
        }

        private void OnDisable()
        {
            characterTurnEvent.RemoveListener(this);
            chooseTargetEvent.RemoveListener(this);
        }

        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 == characterTurnEvent) _isMultiTarget = false;
            else if (value2 == chooseTargetEvent)
            {
                BattleEngine.Instance.choosingTarget = true;
                character = (PartyMember) value1;
                    
                SetNavigation();
                MenuController.SetTargetFirstSelected();
                    
                if (_isMultiTarget) Timing.RunCoroutine(WaitForMultiTargetConfirmation());
            }
        }
    }
}
