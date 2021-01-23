using Characters;
using Characters.PartyMembers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleSystem.Mechanics
{
    public class SpecialAttackSystem : MonoBehaviour, IGameEventListener<BattleEvents>, IGameEventListener<CharacterEvents>
    {
        public PartyMember member;
        private Unit unit;

        [SerializeField] [ReadOnly] private float specialBarVal;
        
        private const float MaxBarVal = 1f;
        private const float TimedAttackPointAmount = 0.05f;
        private const float TimedDefensePointAmount = 0.05f;
        private const float RegAttackPointAmount = 0.05f;
        private const float NewRoundPointAmount = 0.05f;

        public float SpecialBarVal
        {
            get => specialBarVal;
            set
            {
                specialBarVal = value > MaxBarVal ? MaxBarVal : value;
                member.specialAttackBarVal = specialBarVal;
                unit.onSpecialBarValChanged?.Invoke(specialBarVal);
            }
        }

        private void Awake()
        {
            unit = GetComponent<Unit>();

            unit.onTimedAttack += OnTimedAttack;
            unit.onTimedDefense += OnTimedDefense;
            unit.onSpecialAttack += OnSpecialAttack;
            GameEventsManager.AddListener<BattleEvents>(this);
            GameEventsManager.AddListener<CharacterEvents>(this);
        }

        private void OnSpecialAttack() => SpecialBarVal = 0;

        private void OnTimedAttack(bool result) { if (result) SpecialBarVal += TimedAttackPointAmount; }

        private void OnTimedDefense(bool result) { if (result) SpecialBarVal += TimedDefensePointAmount; }

        private void OnAttack() => SpecialBarVal += RegAttackPointAmount;

        private void OnNewRound() => SpecialBarVal += NewRoundPointAmount;

        private void OnDisable()
        {
            unit.onTimedAttack -= OnTimedAttack;
            unit.onTimedDefense -= OnTimedDefense;
            unit.onSpecialAttack -= OnSpecialAttack;
            GameEventsManager.RemoveListener<BattleEvents>(this);
            GameEventsManager.RemoveListener<CharacterEvents>(this);
        }

        public void OnGameEvent(BattleEvents eventType)
        {
            if (eventType._battleEventType == BattleEventType.NewRound)
            {
                OnNewRound();
            }
        }

        public void OnGameEvent(CharacterEvents eventType)
        {
            if (eventType._eventType != CEventType.CharacterAttacking) return;
            var partyMember = eventType._character as PartyMember;
            if (partyMember != null && partyMember.Unit == unit) OnAttack();
        }
    }
}