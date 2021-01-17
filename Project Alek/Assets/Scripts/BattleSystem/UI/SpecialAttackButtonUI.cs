using Characters.PartyMembers;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class SpecialAttackButtonUI : MonoBehaviour
    {
        public PartyMember member;
        private Button button;

        private void Start() => button = GetComponent<Button>();
        
        private void Update() => button.interactable = member.specialAttackBarVal >= 1f;
    }
}