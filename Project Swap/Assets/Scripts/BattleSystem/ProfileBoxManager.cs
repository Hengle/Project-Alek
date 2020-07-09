using System.Linq;
using Characters;
using Characters.StatusEffects;
using DG.Tweening;
using Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class ProfileBoxManager : MonoBehaviour, IGameEventListener<UIEvents>
    {
        [SerializeField] [ReadOnly] private UnitBase unitBase;
        [SerializeField] [ReadOnly] private bool isOpen;

        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI stats;

        [SerializeField] private Image background;
        [SerializeField] private Image spriteImage;

        [SerializeField] private Transform profileBox;
        [SerializeField] private Transform weaknessesBox;
        [SerializeField] private Transform resistancesBox;

        private void Start()
        {
            DOTween.Init();
            isOpen = false;
        }
        
        public void SetupProfileBox(UnitBase character)
        {
            unitBase = character;
            profileBox.name = $"{character.characterName} profile";
            description.text = character.description;
            spriteImage.sprite = character.Unit.gameObject.GetComponent<SpriteRenderer>().sprite;
            description.text = character.description;
            background.color = character.profileBoxColor;

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
                $"Health: {unitBase.Unit.currentHP}\n" +
                $"STR: {unitBase.strength.Value} ({unitBase.strength.Value - unitBase.strength.BaseValue})\n" +
                $"MAG: {unitBase.magic.Value} ({unitBase.magic.Value - unitBase.magic.BaseValue})\n" +
                $"ACC: {unitBase.accuracy.Value} ({unitBase.accuracy.Value - unitBase.accuracy.BaseValue})\n" +
                $"INIT: {unitBase.initiative.Value} ({unitBase.initiative.Value - unitBase.initiative.BaseValue})\n" +
                $"DEF: {unitBase.defense.Value} ({unitBase.defense.Value - unitBase.defense.BaseValue})\n" +
                $"RES: {unitBase.resistance.Value} ({unitBase.resistance.Value - unitBase.resistance.BaseValue})\n" +
                $"CRIT: {unitBase.criticalChance.Value} ({unitBase.criticalChance.Value - unitBase.criticalChance.BaseValue})";
            
            BattleInputManager._inputModule.move.action.Disable();
            BattleInputManager._inputModule.submit.action.Disable();
            BattleInputManager._inputModule.cancel.action.Disable();
            
            profileBox.gameObject.SetActive(true);
            profileBox.DOScale(1, 0.5f);
        }

        private void CloseProfileBox()
        {
            if (!isOpen) return;
            profileBox.DOScale(0.1f, 0.15f).
                OnComplete(() => profileBox.gameObject.SetActive(false));
            
            BattleInputManager._inputModule.move.action.Enable();
            BattleInputManager._inputModule.submit.action.Enable();
            BattleInputManager._inputModule.cancel.action.Enable();
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