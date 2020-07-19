using System.Collections.Generic;
using Characters.ElementalTypes;
using Characters.StatusEffects;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
    public class RequirementBoxControllerUI : MonoBehaviour
    {
        public Enemy enemy;
        public List<Image> icons = new List<Image>();
        private CanvasGroup group;
        private int index;

        public void Initialize()
        {
            index = 0;
            enemy.onNewState += UpdateUI;
            enemy.Unit.onSelect += ShowIcons;
            enemy.Unit.onDeselect += HideIcons;
            group = GetComponentInParent<CanvasGroup>();

            for (var i = 0; i < enemy.checkmateRequirements.Count; i++)
            {
                var icon = transform.GetChild(i);
                icon.gameObject.SetActive(true);
                icons.Add(icon.GetComponent<Image>());
            }
            
            group.alpha = 0;
        }

        private void UpdateUI(UnitStates state)
        {
            if (state != UnitStates.Intermediate && state != UnitStates.Checkmate) return;

            var requirement = enemy.checkmateRequirements[index];
            var type = requirement.GetType();

            icons[index].sprite = type == typeof(ElementalType)
                ? ((ElementalType) requirement).Icon
                : ((StatusEffect) requirement).Icon;

            index++;
        }
        
        private void ShowIcons() => group.alpha = 1;

        private void HideIcons() => group.alpha = 0;

        private void OnDisable()
        {
            enemy.onNewState -= UpdateUI;
            enemy.Unit.onSelect -= ShowIcons;
            enemy.Unit.onDeselect -= HideIcons;
        }
    }
}