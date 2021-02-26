using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "Spell", menuName = "Ability/Spell")]
    public class Spell : RangedAttack
    {
        public GameObject effectPrefab;
        public Vector3 offset;

        protected override void Awake()
        {
            base.Awake();
            damageType = DamageType.Magic;
            hasElemental = true;
        }
    }
}