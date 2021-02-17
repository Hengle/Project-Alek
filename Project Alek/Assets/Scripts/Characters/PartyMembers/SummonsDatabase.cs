using System.Collections.Generic;
using System.Linq;
using Characters.Enemies;
using Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.PartyMembers
{
    public enum SummonEvent { EquippedSummon, UneQuippedSummon, RegisteredSummon }
    [CreateAssetMenu(fileName = "Summons Database", menuName = "Summons Database")]
    public class SummonsDatabase : ScriptableObject
    {
        [SerializeField] private bool debug;
        [SerializeField] private SummonEvents summonEvents;
        [SerializeField] private List<Enemy> summons = new List<Enemy>();
        [SerializeField] [ReadOnly] private Enemy[] equippedSummons = new Enemy[3];

        [Button] public void RegisterSummon(Enemy enemy, bool tryToEquip = false)
        {
            if (summons.Contains(enemy))
            {
                PrintWarning("This summon is already registered!");
                return;
            }
            summons.Add(enemy);
            summonEvents.Raise(SummonEvent.RegisteredSummon);
            if (tryToEquip) EquipSummon(enemy);
        }

        [Button] public void EquipSummon(Enemy enemy)
        {
            if (equippedSummons.Contains(enemy) || !summons.Contains(enemy))
            {
                PrintWarning("This summon is either already equipped or you do not have this summon registered");
                return;
            }
            
            var hasAdded = false;
            for (var i = 0; i < 3; i++)
            {
                if (equippedSummons[i] != null) continue;
                equippedSummons[i] = enemy;
                summonEvents.Raise(SummonEvent.EquippedSummon);
                hasAdded = true;
                break;
            }
            
            if (!hasAdded) PrintError("Unable to add enemy! Size may have reached maximum (3)");
        }

        [Button] public void UnEquipSummon(Enemy enemy)
        {
            if (!equippedSummons.Contains(enemy))
            {
                PrintWarning("This summon is not equipped");
                return;
            }

            var newArray = equippedSummons.Where(s => s != enemy).ToArray();
            equippedSummons = new[] { newArray[0], newArray[1], null };
            
            summonEvents.Raise(SummonEvent.UneQuippedSummon);
        }

        public IEnumerable<Enemy> GetEquippedSummons()
        {
            return equippedSummons.Where(s => summons.Contains(s));
        }

        [Button] private void ResetEquippedSummons()
        {
            equippedSummons = new Enemy[3];
        }

        private void PrintWarning(string message)
        {
            if (debug) Debug.LogWarning(message);
        }
        
        private void PrintError(string message)
        {
            if (debug) Debug.LogError(message);
        }
    }
}