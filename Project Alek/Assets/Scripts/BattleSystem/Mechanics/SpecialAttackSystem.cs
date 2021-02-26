using Characters;
using Characters.PartyMembers;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleSystem
{
    public class SpecialAttackSystem : MonoBehaviour, IGameEventListener<BattleEvent>, IGameEventListener<UnitBase,CharacterGameEvent>
    {
        public PartyMember member;
        private Unit unit;

        [SerializeField] private BattleGameEvent battleEvent;
        [SerializeField] private CharacterGameEvent characterAttackEvent;
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
            
            battleEvent.AddListener(this);
            characterAttackEvent.AddListener(this);
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
            battleEvent.RemoveListener(this);
        }
        
        public void OnEventRaised(BattleEvent value)
        {
            if (value == BattleEvent.NewRound) OnNewRound();
        }

        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value2 != characterAttackEvent) return;
            var partyMember = value1 as PartyMember;
            if (partyMember != null && partyMember.Unit == unit) OnAttack();
        }
    }
}