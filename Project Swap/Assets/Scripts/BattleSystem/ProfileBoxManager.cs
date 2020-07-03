using System;
using BattleSystem.Generator;
using Characters;
using DG.Tweening;
using Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class ProfileBoxManager : MonoBehaviour, IGameEventListener<UIEvents>
    {
        [SerializeField] private Transform profileBox;
        public TextMeshProUGUI description;
        public TextMeshProUGUI stats;
        public Image background;
        public Image spriteImage;
        public UnitBase unitBase;
        private bool isOpen;

        //[SerializeField] private BattleGeneratorDatabase battleGeneratorDatabase;

        private void Start() 
        {
            DOTween.Init();
            //profileBox = transform.Find("Profile Box");
            spriteImage = profileBox.Find("Sprite").GetComponent<Image>();
            description = profileBox.Find("Description").GetChild(0).GetComponent<TextMeshProUGUI>();
            stats = profileBox.Find("Stats").GetChild(0).GetComponent<TextMeshProUGUI>();
            background = profileBox.Find("Background").GetComponent<Image>();
            isOpen = false;
            GameEventsManager.AddListener(this);
        }
        
        public void SetupProfileBox(UnitBase character)
        {
            unitBase = character;
            
            Logger.Log(character.characterName);
            // spriteImage.sprite = character.Unit.gameObject.GetComponent<SpriteRenderer>().sprite;
            // description.text = character.description;
            // background.color = character.profileBoxColor;
        }

        private void ShowProfileBox()
        {
            isOpen = true;

            stats.text =
                $"Name: {unitBase.characterName}\n" +
                $"Level: {unitBase.level}\n" +
                $"Health: {unitBase.Unit.currentHP}\n" +
                $"STR: {unitBase.Unit.currentStrength}\n" +
                $"MAG: {unitBase.Unit.currentMagic}\n" +
                $"ACC: {unitBase.Unit.currentAccuracy}\n" +
                $"INIT: {unitBase.Unit.currentInitiative}\n" +
                $"DEF: {unitBase.Unit.currentDefense}\n" +
                $"RES: {unitBase.Unit.currentResistance}\n" +
                $"CRIT: {unitBase.Unit.currentCrit}";
            
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