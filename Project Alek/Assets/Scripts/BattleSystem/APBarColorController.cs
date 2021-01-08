using Characters;
using Characters.PartyMembers;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class APBarColorController : MonoBehaviour
    {
        private Unit unit;
        [SerializeField] private Image fill;
        
        private void Start() => unit = transform.parent.transform.parent.
            GetComponentInParent<CharacterPanelController>().member.Unit;
        
        private void OnEnable()
        {
            if (unit == null) return;
            fill.color = unit.status == Status.Overexerted ?
                 BattleManager.Instance.globalVariables.overexertedAPColor :
                 BattleManager.Instance.globalVariables.originalAPColor;
        }
    }
}
