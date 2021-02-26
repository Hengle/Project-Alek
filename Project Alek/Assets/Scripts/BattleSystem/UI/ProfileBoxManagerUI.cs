using Characters;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utils;

namespace BattleSystem
{
    public class ProfileBoxManagerUI : MonoBehaviour
    {
        #region FieldsAndProperties
        
        [SerializeField] [ReadOnly] private UnitBase unitBase;
        [SerializeField] [ReadOnly] private bool isOpen;

        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI stats;
        
        [SerializeField] private Image spriteImage;

        [SerializeField] private Transform profileBox;
        [SerializeField] private Transform weaknessesBox;
        [SerializeField] private Transform resistancesBox;

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

            character._elementalResistances.ForEach(e =>
            {
                var obj = Instantiate(e.Key._type.icon, resistancesBox, false);
                obj.name = e.Key._type.icon.name;
                obj.GetComponent<Image>().sprite = e.Value ?
                    e.Key._type.Icon : GlobalVariables.Instance.mysteryIcon;
            });
            
            character._elementalWeaknesses.ForEach(e =>
            {
                var obj = Instantiate(e.Key._type.icon, weaknessesBox, false);
                obj.name = e.Key._type.icon.name;
                obj.GetComponent<Image>().sprite = e.Value ?
                    e.Key._type.Icon : GlobalVariables.Instance.mysteryIcon;
            });

            character._statusEffectResistances.ForEach(status =>
            {
                var obj = Instantiate(status.Key._effect.icon, resistancesBox, false);
                obj.name = status.Key._effect.icon.name;
                obj.GetComponent<Image>().sprite = status.Value ?
                    status.Key._effect.Icon : GlobalVariables.Instance.mysteryIcon;
                obj.GetComponent<StatusEffectTimer>().enabled = false;
            });

            character._statusEffectWeaknesses.ForEach(status =>
            {
                var obj = Instantiate(status.Key._effect.icon, weaknessesBox, false);
                obj.name = status.Key._effect.icon.name;
                obj.GetComponent<Image>().sprite = status.Value ?
                    status.Key._effect.Icon : GlobalVariables.Instance.mysteryIcon;
                obj.GetComponent<StatusEffectTimer>().enabled = false;
            });
            
            character._damageTypeWeaknesses.ForEach(t =>
            {
                var obj = Instantiate(t.Key.icon, weaknessesBox, false);
                obj.name = t.Key.icon.name;
                obj.GetComponent<Image>().sprite = t.Value ?
                    t.Key.Icon : GlobalVariables.Instance.mysteryIcon;
            });
            
            profileBox.gameObject.SetActive(false);

            unitBase.onElementalDamageReceived += RevealWeakness;
            unitBase.onElementalDamageReceived += RevealResistance;
            unitBase.onStatusEffectReceived += RevealWeakness;
            unitBase.onStatusEffectReceived += RevealResistance;
            unitBase.onWeaponDamageTypeReceived += RevealWeakness;
        }

        private void ShowProfileBox()
        {
            BattleInput._inputModule.enabled = false;
            BattleInput._controls.Disable();
            BattleInput._controls.Battle.TopButton.Enable();
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
            

            profileBox.gameObject.SetActive(true);
            profileBox.DOScale(0.75f, 0.5f);
        }

        private void CloseProfileBox()
        {
            if (!isOpen) return;
            profileBox.DOScale(0.1f, 0.15f).
                OnComplete(() => profileBox.gameObject.SetActive(false));
            
            BattleInput._inputModule.enabled = true;
            BattleInput._controls.Enable();
            isOpen = false;
        }

        private void OnProfileBoxButton(InputAction.CallbackContext ctx)
        {
            if (!BattleInput._canOpenBox) return;
            if (!EventSystem.current.currentSelectedGameObject.TryGetComponent(out Unit unit)) return;
            if (unit != unitBase.Unit) return;
            
            if (!isOpen) ShowProfileBox();
            else CloseProfileBox();
        }

        #region RevealFunctions
        
        private void RevealWeakness(ElementalType elementalType)
        {
            var list = weaknessesBox.transform.GetComponentsInChildren<Image>();
            foreach (var image in list)
            {
                if (image.name != elementalType.icon.name) continue;
                if (image.GetComponent<Image>().sprite == elementalType.Icon) return;
                
                image.GetComponent<Image>().sprite = elementalType.Icon;
            }
        }
        
        private void RevealResistance(ElementalType elementalType)
        {
            var list = resistancesBox.transform.GetComponentsInChildren<Image>();
            foreach (var image in list)
            {
                if (image.name != elementalType.icon.name) continue;
                if (image.GetComponent<Image>().sprite == elementalType.Icon) return;

                image.GetComponent<Image>().sprite = elementalType.Icon;
            }
        }
        
        private void RevealWeakness(StatusEffect effect)
        {
            var list = weaknessesBox.transform.GetComponentsInChildren<Image>();
            foreach (var image in list)
            {
                if (image.name != effect.icon.name) continue;
                if (image.GetComponent<Image>().sprite == effect.Icon) return;

                image.GetComponent<Image>().sprite = effect.Icon;
            }
        }
        
        private void RevealResistance(StatusEffect effect)
        {
            var list = resistancesBox.transform.GetComponentsInChildren<Image>();
            foreach (var image in list)
            {
                if (image.name != effect.icon.name) continue;
                if (image.GetComponent<Image>().sprite == effect.Icon) return;
     
                image.GetComponent<Image>().sprite = effect.Icon;
            }
        }
        
        private void RevealWeakness(WeaponDamageType damageType)
        {
            var list = weaknessesBox.transform.GetComponentsInChildren<Image>();
            foreach (var image in list)
            {
                if (image.name != damageType.icon.name) continue;
                if (image.GetComponent<Image>().sprite == damageType.Icon) return;
                
                image.GetComponent<Image>().sprite = damageType.Icon;
            }
        }
        
        #endregion

        private void OnDisable()
        {
            unitBase.onElementalDamageReceived -= RevealWeakness;
            unitBase.onElementalDamageReceived -= RevealResistance;
            unitBase.onStatusEffectReceived -= RevealWeakness;
            unitBase.onStatusEffectReceived -= RevealResistance;
            unitBase.onWeaponDamageTypeReceived -= RevealWeakness;
            
            BattleInput._controls.Battle.TopButton.performed -= OnProfileBoxButton;
        }

        private void OnEnable() => BattleInput._controls.Battle.TopButton.performed += OnProfileBoxButton;
    }
}