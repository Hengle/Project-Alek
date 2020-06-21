using TMPro;
using UnityEngine;

namespace BattleSystem
{
    [CreateAssetMenu(fileName = "Show Damage")]
    public class ShowDamageSO : ScriptableObject
    {
        public GameObject damagePrefab;
        public GameObject critDamagePrefab;
        public TextMeshPro nameTextWithDmgColor;
    }
}