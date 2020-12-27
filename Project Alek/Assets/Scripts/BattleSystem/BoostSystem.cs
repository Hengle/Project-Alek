using Characters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleSystem
{
    public enum DamageBoostLvl { Level0 = 0, Level1 = 1, Level2 = 2, Level3 = 3, Level4 = 4, Level5 = 5 }
    public enum DefenseBoostLvl { Level0 = 0, Level1 = 1, Level2 = 2, Level3 = 3, Level4 = 4, Level5 = 5 }
    public class BoostSystem : MonoBehaviour
    {
        private Unit unit;

        [ShowInInspector] private DamageBoostLvl damageBoostLvl;
        [ShowInInspector] private DefenseBoostLvl defenseBoostLvl;

        public float FinalDamageBoostVal
        {
            get
            {
                switch (damageBoostLvl)
                {
                    case DamageBoostLvl.Level0:
                        return 1.0f;
                    case DamageBoostLvl.Level1:
                        return 1.03f;
                    case DamageBoostLvl.Level2:
                        return 1.06f;
                    case DamageBoostLvl.Level3:
                        return 1.09f;
                    case DamageBoostLvl.Level4:
                        return 1.12f;
                    case DamageBoostLvl.Level5:
                        return 1.15f;
                    default: return 1.0f;
                }
            }
        }
        
        public float FinalDefenseBoostVal
        {
            get
            {
                switch (defenseBoostLvl)
                {
                    case DefenseBoostLvl.Level0:
                        return 1.0f;
                    case DefenseBoostLvl.Level1:
                        return 0.97f;
                    case DefenseBoostLvl.Level2:
                        return 0.94f;
                    case DefenseBoostLvl.Level3:
                        return 0.91f;
                    case DefenseBoostLvl.Level4:
                        return 0.88f;
                    case DefenseBoostLvl.Level5:
                        return 0.85f;
                    default: return 1.0f;
                }
            }
        }

        private void Start()
        {
            unit = GetComponent<Unit>();
            damageBoostLvl = DamageBoostLvl.Level0;
            defenseBoostLvl = DefenseBoostLvl.Level0;
            
            unit.onTimedAttack += EvaluateDamageBoostLevel;
            unit.onTimedDefense += EvaluateDefenseBoostLevel;
        }

        private void EvaluateDamageBoostLevel(bool condition)
        {
            if (condition) 
            {
                if (damageBoostLvl < DamageBoostLvl.Level5) damageBoostLvl += 1;
            }
            else damageBoostLvl = DamageBoostLvl.Level0;
            
            unit.onDmgValueChanged.Invoke((int)damageBoostLvl, condition);
        }
        
        private void EvaluateDefenseBoostLevel(bool condition)
        {
            if (condition) {
                if (defenseBoostLvl < DefenseBoostLvl.Level5) defenseBoostLvl += 1;
            }
            else defenseBoostLvl = DefenseBoostLvl.Level0;
            
            unit.onDefValueChanged.Invoke((int)defenseBoostLvl, condition);
        }

        private void OnDisable()
        {
            unit.onTimedAttack -= EvaluateDamageBoostLevel;
            unit.onTimedDefense -= EvaluateDefenseBoostLevel;
        }
    }
}
