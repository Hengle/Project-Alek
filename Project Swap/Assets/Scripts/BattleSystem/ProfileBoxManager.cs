using BattleSystem.Generator;
using Characters;
using DG.Tweening;
using Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class ProfileBoxManager : MonoBehaviour
    {
        public static bool _isOpen;

        private static Transform profileBox;
        private static TextMeshProUGUI description;
        private static TextMeshProUGUI stats;
        private static Image background;
        private static Image spriteImage;

        [SerializeField] private BattleGeneratorDatabase battleGeneratorDatabase;

        private void Start() 
        {
            DOTween.Init();
            profileBox = battleGeneratorDatabase.profileBox;
            spriteImage = profileBox.Find("Sprite").GetComponent<Image>();
            description = profileBox.Find("Description").GetChild(0).GetComponent<TextMeshProUGUI>();
            stats = profileBox.Find("Stats").GetChild(0).GetComponent<TextMeshProUGUI>();
            background = profileBox.Find("Background").GetComponent<Image>();
            _isOpen = false;
        }
        
        public static void ShowProfileBox(UnitBase unitBase)
        {
            _isOpen = true;
            spriteImage.sprite = unitBase.characterPrefab.GetComponent<SpriteRenderer>().sprite;
            description.text = unitBase.description;
            background.color = unitBase.profileBoxColor;
            
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

        public static void CloseProfileBox()
        {
            profileBox.DOScale(0.1f, 0.15f).
                OnComplete(() => profileBox.gameObject.SetActive(false));
            
            BattleInputManager._inputModule.move.action.Enable();
            BattleInputManager._inputModule.submit.action.Enable();
            BattleInputManager._inputModule.cancel.action.Enable();
            _isOpen = false;
        }
    }
}