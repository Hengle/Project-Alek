using System.Linq;
using Characters;
using Characters.StatusEffects;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class ProfileBoxManager : MonoBehaviour, IGameEventListener<UIEvents>
    {
        #region FieldsAndProperties
        
        [SerializeField] [ReadOnly] private UnitBase unitBase;
        [SerializeField] [ReadOnly] private bool isOpen;

        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI stats;

        [SerializeField] private Image background;
        [SerializeField] private Image spriteImage;

        [SerializeField] private Transform profileBox;
        [SerializeField] private Transform weaknessesBox;
        [SerializeField] private Transform resistancesBox;

        [SerializeField] private Color color;
        
        #endregion

        #region StatValDifferences
        
        private string StrDiff {
            get
            {
                if (unitBase.strength.Value - unitBase.strength.BaseValue > 0) 
                    return $"( +{unitBase.strength.Value - unitBase.strength.BaseValue})";
                
                return unitBase.strength.Value - unitBase.strength.BaseValue < 0?
                    $"( {unitBase.strength.Value - unitBase.strength.BaseValue})" : "";
            }
        }

        private string MagDiff {
            get
            {
                if (unitBase.magic.Value - unitBase.magic.BaseValue > 0) 
                    return $"( +{unitBase.magic.Value - unitBase.magic.BaseValue})";
                
                return unitBase.magic.Value - unitBase.magic.BaseValue < 0?
                    $"( {unitBase.magic.Value - unitBase.magic.BaseValue})" : "";
            }
        }
        
        private string AccDiff {
            get
            {
                if (unitBase.accuracy.Value - unitBase.accuracy.BaseValue > 0) 
                    return $"( +{unitBase.accuracy.Value - unitBase.accuracy.BaseValue})";
                
                return unitBase.accuracy.Value - unitBase.accuracy.BaseValue < 0?
                    $"( {unitBase.accuracy.Value - unitBase.accuracy.BaseValue})" : "";
            }
        }
        
        private string InitDiff {
            get
            {
                if (unitBase.initiative.Value - unitBase.initiative.BaseValue > 0) 
                    return $"( +{unitBase.initiative.Value - unitBase.initiative.BaseValue})";
                
                return unitBase.initiative.Value - unitBase.initiative.BaseValue < 0?
                    $"( {unitBase.initiative.Value - unitBase.initiative.BaseValue})" : "";
            }
        }
        
        private string DefDiff {
            get
            {
                if (unitBase.defense.Value - unitBase.defense.BaseValue > 0) 
                    return $"( +{unitBase.defense.Value - unitBase.defense.BaseValue})";
                
                return unitBase.defense.Value - unitBase.defense.BaseValue < 0?
                    $"( {unitBase.defense.Value - unitBase.defense.BaseValue})" : "";
            }
        }
        
        private string ResDiff {
            get
            {
                if (unitBase.resistance.Value - unitBase.resistance.BaseValue > 0) 
                    return $"( +{unitBase.resistance.Value - unitBase.resistance.BaseValue})";
                
                return unitBase.resistance.Value - unitBase.resistance.BaseValue < 0?
                    $"( {unitBase.resistance.Value - unitBase.resistance.BaseValue})" : "";
            }
        }
        
        private string CritDiff {
            get
            {
                if (unitBase.criticalChance.Value - unitBase.criticalChance.BaseValue > 0) 
                    return $"( +{unitBase.criticalChance.Value - unitBase.criticalChance.BaseValue})";
                
                return unitBase.criticalChance.Value - unitBase.criticalChance.BaseValue < 0?
                    $"( {unitBase.criticalChance.Value - unitBase.criticalChance.BaseValue})" : "";
            }
        }
        
        #endregion

        private void Start() => DOTween.Init();

        public void SetupProfileBox(UnitBase character)
        {
            isOpen = false;
            
            unitBase = character;
            profileBox.name = $"{character.characterName} profile";
            description.text = character.description;
            spriteImage.sprite = character.Unit.gameObject.GetComponent<SpriteRenderer>().sprite;
            description.text = character.description;
            background.color = color;

            foreach (var element in character._elementalResistances)
            {
                Instantiate(element.Key.icon, resistancesBox, false);
            }

            foreach (var element in character._elementalWeaknesses)
            {
                Instantiate(element.Key.icon, weaknessesBox, false);
            }

            foreach (var icon in character._statusEffectResistances.Select
                (status => Instantiate(status.Key.icon, resistancesBox, false)))
            {
                icon.GetComponent<StatusEffectTimer>().enabled = false;
            }

            foreach (var icon in character._statusEffectWeaknesses.Select
                (status => Instantiate(status.Key.icon, weaknessesBox, false)))
            {
                icon.GetComponent<StatusEffectTimer>().enabled = false;
            }
            
            profileBox.gameObject.SetActive(false);
            GameEventsManager.AddListener(this);
        }

        private void ShowProfileBox()
        {
            isOpen = true;

            stats.text =
                $"Name: {unitBase.characterName}\n" +
                $"Level: {unitBase.level}\n" +
                $"STR: {unitBase.strength.Value} {StrDiff}\n" +
                $"MAG: {unitBase.magic.Value} {MagDiff}\n" +
                $"ACC: {unitBase.accuracy.Value} {AccDiff}\n" +
                $"INIT: {unitBase.initiative.Value} {InitDiff}\n" +
                $"DEF: {unitBase.defense.Value} {DefDiff}\n" +
                $"RES: {unitBase.resistance.Value} {ResDiff}\n" +
                $"CRIT: {unitBase.criticalChance.Value} {CritDiff}";
            
            BattleInputManager._controls.Menu.Move.Disable();
            BattleInputManager._controls.Menu.Confirm.Disable();
            BattleInputManager._controls.Menu.Back.Disable();
            // BattleInputManager._inputModule.move.action.Disable();
            // BattleInputManager._inputModule.submit.action.Disable();
            // BattleInputManager._inputModule.cancel.action.Disable();
            
            profileBox.gameObject.SetActive(true);
            profileBox.DOScale(1, 0.5f);
        }

        private void CloseProfileBox()
        {
            if (!isOpen) return;
            profileBox.DOScale(0.1f, 0.15f).
                OnComplete(() => profileBox.gameObject.SetActive(false));
            
            BattleInputManager._controls.Menu.Move.Enable();
            BattleInputManager._controls.Menu.Confirm.Enable();
            BattleInputManager._controls.Menu.Back.Enable();
            // BattleInputManager._inputModule.move.action.Enable();
            // BattleInputManager._inputModule.submit.action.Enable();
            // BattleInputManager._inputModule.cancel.action.Enable();
            isOpen = false;
        }

        public void OnGameEvent(UIEvents eventType)
        {
            if (eventType._eventType != UIEventType.ToggleProfileBox) return;
            if (!eventType._gameObject.TryGetComponent(out Unit unit) || unit != unitBase.Unit) return;
            
            if (!isOpen) ShowProfileBox();
            else CloseProfileBox();
        }
    }
}