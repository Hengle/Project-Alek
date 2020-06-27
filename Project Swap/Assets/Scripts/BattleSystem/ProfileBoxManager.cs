using BattleSystem.Generator;
using Characters;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class ProfileBoxManager : MonoBehaviour
    {
        [SerializeField] private BattleGeneratorDatabase battleGeneratorDatabase;
        private static Transform profileBox;
        private static Image background;
        private static Image spriteImage;
        private static TextMeshProUGUI description;
        private static TextMeshProUGUI stats;
        public static bool isOpen;

        private void Start() 
        {
            DOTween.Init();
            profileBox = battleGeneratorDatabase.profileBox;
            spriteImage = profileBox.Find("Sprite").GetComponent<Image>();
            description = profileBox.Find("Description").GetChild(0).GetComponent<TextMeshProUGUI>();
            stats = profileBox.Find("Stats").GetChild(0).GetComponent<TextMeshProUGUI>();
            background = profileBox.Find("Background").GetComponent<Image>();
            isOpen = false;
        }
        
        public static void ShowProfileBox(UnitBase unitBase)
        {
            isOpen = true;
            spriteImage.sprite = unitBase.characterPrefab.GetComponent<SpriteRenderer>().sprite;
            description.text = unitBase.description;
            background.color = unitBase.profileBoxColor;
            
            stats.text =
                $"Name: {unitBase.characterName}\n" +
                $"Level: {unitBase.level}\n" +
                $"Health: {unitBase.unit.currentHP}\n" +
                $"STR: {unitBase.unit.currentStrength}\n" +
                $"MAG: {unitBase.unit.currentMagic}\n" +
                $"ACC: {unitBase.unit.currentAccuracy}\n" +
                $"INIT: {unitBase.unit.currentInitiative}\n" +
                $"DEF: {unitBase.unit.currentDefense}\n" +
                $"RES: {unitBase.unit.currentResistance}\n" +
                $"CRIT: {unitBase.unit.currentCrit}";

            BattleHandler.inputModule.move.action.Disable();
            BattleHandler.inputModule.submit.action.Disable();
            BattleHandler.inputModule.cancel.action.Disable();
            
            profileBox.gameObject.SetActive(true);
            profileBox.DOScale(1, 0.5f);
        }

        public static void CloseProfileBox()
        {
            profileBox.DOScale(0.1f, 0.15f).OnComplete(() => profileBox.gameObject.SetActive(false));
            BattleHandler.inputModule.move.action.Enable();
            BattleHandler.inputModule.submit.action.Enable();
            BattleHandler.inputModule.cancel.action.Enable();
            isOpen = false;
        }
    }
}