using UnityEngine;

namespace Characters
{
    public class StatusEffectControllerUI : MonoBehaviour
    {
        public UnitBase member;
        private CanvasGroup group;
        [SerializeField] private Transform uniqueEffectContainer;

        public void Initialize()
        {
            member.onStatusEffectReceived += AddStatusEffectIcon;
            member.onStatusEffectRemoved += RemoveStatusEffectIcon;
            
            if (member.id != CharacterType.Enemy) return;
            member.Unit.onSelect += ShowIcons;
            member.Unit.onDeselect += HideIcons;
            group = GetComponentInParent<CanvasGroup>();
        }

        private void AddStatusEffectIcon(StatusEffect effect)
        {
            var parent = effect.effectType == EffectType.Unique ? uniqueEffectContainer.transform : transform;
            var alreadyHasIcon = parent.Find(effect.name);
            
            if (effect.icon != null && alreadyHasIcon == null) {
                var iconGO = Instantiate(effect.icon, parent, false);
                iconGO.name = effect.name;
                iconGO.GetComponent<StatusEffectTimer>().SetTimer(effect, member);
            }
            
            else if (effect.icon != null && alreadyHasIcon != null) {
                alreadyHasIcon.gameObject.SetActive(true);
                alreadyHasIcon.GetComponent<StatusEffectTimer>().SetTimer(effect, member);
            }
        }

        private void RemoveStatusEffectIcon(StatusEffect effect)
        {
            var parent = effect.effectType == EffectType.Unique ? uniqueEffectContainer.transform : transform;
            var iconGO = parent.Find(effect.name);
            if (iconGO != null) iconGO.gameObject.SetActive(false);
        }

        private void ShowIcons() => group.alpha = 1;

        private void HideIcons() => group.alpha = 0;
        
        private void OnDisable()
        {
            member.onStatusEffectReceived -= AddStatusEffectIcon;
            member.onStatusEffectRemoved -= RemoveStatusEffectIcon;
        }
    }
}