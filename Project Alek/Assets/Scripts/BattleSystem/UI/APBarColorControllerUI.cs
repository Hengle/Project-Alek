using Characters;
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
                 BattleEngine.Instance.globalVariables.overexertedAPColor :
                 BattleEngine.Instance.globalVariables.originalAPColor;
        }
    }
}
