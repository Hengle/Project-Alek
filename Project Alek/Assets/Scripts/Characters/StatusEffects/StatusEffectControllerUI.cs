using UnityEngine;

namespace Characters.StatusEffects
{
    public class StatusEffectControllerUI : MonoBehaviour
    {
        public UnitBase member;
        private CanvasGroup group;
        [SerializeField] private Checkmate checkmate;

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
            var alreadyHasIcon = transform.Find(effect.name);
            
            if (effect.icon != null && alreadyHasIcon == null) {
                var iconGO = Instantiate(effect.icon, transform, false);
                iconGO.name = effect.name;
                iconGO.GetComponent<StatusEffectTimer>().SetTimer(effect, member);
            }
            
            else if (effect.icon != null && alreadyHasIcon != null) {
                alreadyHasIcon.gameObject.SetActive(true);
                alreadyHasIcon.GetComponent<StatusEffectTimer>().SetTimer(effect, member);
            }
            
            else if (effect.GetType() == typeof(Checkmate))
            {
                if (alreadyHasIcon != null)
                {
                    alreadyHasIcon.gameObject.SetActive(true);
                    alreadyHasIcon.GetComponent<StatusEffectTimer>().SetTimer(effect, member);
                }
                
                var iconGO = Instantiate(checkmate.icon, transform, false);
                iconGO.name = effect.name;
                effect.turnDuration = checkmate.turnDuration;
                iconGO.GetComponent<StatusEffectTimer>().SetTimer(effect, member);
            }
        }

        private void RemoveStatusEffectIcon(StatusEffect effect)
        {
            var iconGO = transform.Find(effect.name);
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