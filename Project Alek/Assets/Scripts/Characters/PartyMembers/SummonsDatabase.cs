using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Enemies;
using SingletonScriptableObject;
using Sirenix.OdinInspector;
using UnityEngine;
using Events;

namespace Characters.PartyMembers
{
    public enum SummonEvent { EquippedSummon, UneQuippedSummon, RegisteredSummon }
    [CreateAssetMenu(fileName = "Summons Database", menuName = "Summons Database")]
    public class SummonsDatabase : ScriptableObject
    {
        [SerializeField] private bool debug;
        [SerializeField] private SummonEvents summonEvents;
        [SerializeField] [ReadOnly] private List<Enemy> registeredSummons = new List<Enemy>();
        [SerializeField] [ReadOnly] private Enemy[] equippedSummons = new Enemy[3];

        [Button] public bool RegisterSummon(Enemy enemy, bool tryToEquip = false)
        {
            if (DatabaseContains(enemy))
            {
                PrintWarning($"{enemy.characterName} is already registered as a summon!");
                return false;
            }

            var summon = enemy;
            if (Application.isPlaying)
            {
                var summonMatch = EnemyManager.Enemies.FirstOrDefault
                    (e => e.characterName == enemy.characterName);

                if (summonMatch == null)
                {
                    PrintError("The matching enemy could not be found!");
                    return false;
                }

                summon = summonMatch;
            }

            registeredSummons.Add(summon);
            summonEvents.Raise(SummonEvent.RegisteredSummon);
            PrintMessage($"Successfully registered {enemy.characterName} as a summon");
            
            if (tryToEquip) EquipSummon(summon);
            return true;
        }

        [Button] public bool EquipSummon(Enemy enemy)
        {
            if (!DatabaseContains(enemy) || EquippedSummonsContains(enemy))
            {
                PrintWarning($"{enemy.characterName} is either already equipped or you do not have this summon registered");
                return false;
            }
     
            for (var i = 0; i < 3; i++)
            {
                if (equippedSummons[i] != null) continue;
                equippedSummons[i] = enemy;
                
                summonEvents.Raise(SummonEvent.EquippedSummon);
                PrintMessage($"Successfully equipped {enemy.characterName}");
                return true;
            }
            
            PrintError($"Unable to add {enemy.characterName}! Size may have reached maximum (3)");
            return false;
        }

        [Button] public bool UnEquipSummon(Enemy enemy)
        {
            if (!EquippedSummonsContains(enemy))
            {
                PrintWarning("This summon is not equipped");
                return false;
            }

            var newArray = equippedSummons.Where(s => s != enemy).ToArray();
            equippedSummons = new[] { newArray[0], newArray[1], null };
            
            summonEvents.Raise(SummonEvent.UneQuippedSummon);
            PrintMessage($"Successfully unequipped {enemy.characterName}");
            return true;
        }

        [Button] private bool UnregisterSummon(Enemy enemy)
        {
            if (!DatabaseContains(enemy))
            {
                PrintWarning($"{enemy.characterName} is not registered as a summon!");
                return false;
            }

            registeredSummons.Remove(enemy);
            if (EquippedSummonsContains(enemy)) UnEquipSummon(enemy);
            PrintMessage($"Successfully Unregistered {enemy.characterName} as a summon");
            return true;
        }

        [Button] private void EmptySummonDatabase()
        {
            registeredSummons.Clear();
            EmptyEquippedSummons();
        }

        [Button] private void EmptyEquippedSummons() => equippedSummons = new Enemy[3];

        public IEnumerable<Enemy> GetEquippedSummons() => equippedSummons.Where(DatabaseContains);

        private bool DatabaseContains(Enemy enemy) => registeredSummons.Exists
            (e => e.characterName == enemy.characterName);

        private bool EquippedSummonsContains(Enemy enemy) => Array.Exists
            (equippedSummons, e => e != null && e.characterName == enemy.characterName);

        private void PrintMessage(string message) { if (debug) Debug.Log(message); }

        private void PrintWarning(string message) { if (debug) Debug.LogWarning(message); }
        
        private void PrintError(string message) { if (debug) Debug.LogError(message); }
    }
}