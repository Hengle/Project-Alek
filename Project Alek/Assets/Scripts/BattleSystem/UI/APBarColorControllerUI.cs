using Characters;
using Characters.PartyMembers;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class APBarColorControllerUI : MonoBehaviour
    {
        private Unit unit;
        [SerializeField] private Image fill;
        
        private void Start() => unit = transform.parent.transform.parent.
            GetComponentInParent<CharacterPanelControllerUI>().member.Unit;
        
        private void OnEnable()
        {
            if (unit == null) return;
            fill.color = unit.status == Status.Overexerted ?
                 BattleManager.Instance.globalVariables.overexertedAPColor :
                 BattleManager.Instance.globalVariables.originalAPColor;
        }
    }
}
