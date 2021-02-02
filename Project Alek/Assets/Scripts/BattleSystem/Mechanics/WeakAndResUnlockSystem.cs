﻿using System.Collections.Generic;
using System.Linq;
using Characters;
using Characters.ElementalTypes;
using Characters.Enemies;
using Characters.StatusEffects;
using SingletonScriptableObject;
using UnityEngine;

namespace BattleSystem.Mechanics
{
    public class WeakAndResUnlockSystem : MonoBehaviour
    {
        private UnitBase enemy;

        public void Initialize(UnitBase unitBase)
        {
            enemy = unitBase;
            
            enemy.onElementalDamageReceived += RevealWeakness;
            enemy.onElementalDamageReceived += RevealResistance;
            enemy.onStatusEffectReceived += RevealWeakness;
            enemy.onStatusEffectReceived += RevealResistance;
            enemy.onWeaponDamageTypeReceived += RevealWeakness;
        }

        private void RevealWeakness(ElementalType elementalType)
        {
            var found = false;
            KeyValuePair<UnitBase.ElementalWeaknessStruct, bool> eStruct;
            foreach (var type in enemy._elementalWeaknesses.
                Where(effect => effect.Key._type == elementalType))
            {
                if (type.Value) continue;
                found = true;
                eStruct = type;
                
                var enemies = EnemyManager.Instance.enemies;
                foreach (var t in enemies.Where(t => t.characterName == enemy.characterName))
                {
                    t._elementalWeaknesses[type.Key] = true;
                    break;
                }
            }
            
            if (found) enemy._elementalWeaknesses[eStruct.Key] = true;
        }
        
        private void RevealResistance(ElementalType elementalType)
        {
            var found = false;
            KeyValuePair<UnitBase.ElementalResStruct, bool> eStruct;
            foreach (var type in enemy._elementalResistances.
                Where(effect => effect.Key._type == elementalType))
            {
                if (type.Value) continue;
                found = true;
                eStruct = type;
                
                var enemies = EnemyManager.Instance.enemies;
                foreach (var t in enemies.Where(t => t.characterName == enemy.characterName))
                {
                    t._elementalResistances[type.Key] = true;
                    break;
                }
            }
            
            if (found) enemy._elementalResistances[eStruct.Key] = true;
        }
        
        private void RevealWeakness(StatusEffect statusEffect)
        {
            var found = false;
            KeyValuePair<UnitBase.StatusEffectStruct, bool> eStruct;
            foreach (var type in enemy._statusEffectWeaknesses.
                Where(effect => effect.Key._effect == statusEffect))
            {
                if (type.Value) continue;
                found = true;
                eStruct = type;
                
                var enemies = EnemyManager.Instance.enemies;
                foreach (var t in enemies.Where(t => t.characterName == enemy.characterName))
                {
                    t._statusEffectWeaknesses[type.Key] = true;
                    break;
                }
            }
            
            if (found) enemy._statusEffectWeaknesses[eStruct.Key] = true;
        }
        
        private void RevealResistance(StatusEffect statusEffect)
        {
            var found = false;
            KeyValuePair<UnitBase.StatusEffectStruct, bool> eStruct;
            foreach (var type in enemy._statusEffectResistances.
                Where(effect => effect.Key._effect == statusEffect))
            {
                if (type.Value) continue;
                found = true;
                eStruct = type;
                
                var enemies = EnemyManager.Instance.enemies;
                foreach (var t in enemies.Where(t => t.characterName == enemy.characterName))
                {
                    t._statusEffectResistances[type.Key] = true;
                    break;
                }
            }
            
            if (found) enemy._statusEffectResistances[eStruct.Key] = true;
        }
        
        private void RevealWeakness(WeaponDamageType damageType)
        {
            var found = false;
            KeyValuePair<WeaponDamageType, bool> eStruct;
            foreach (var type in enemy._damageTypeWeaknesses.
                Where(t => t.Key == damageType))
            {
                if (type.Value) continue;
                found = true;
                eStruct = type;
                
                var enemies = EnemyManager.Instance.enemies;
                foreach (var t in enemies.Where(t => t.characterName == enemy.characterName))
                {
                    t._damageTypeWeaknesses[type.Key] = true;
                    break;
                }
            }
            
            if (found) enemy._damageTypeWeaknesses[eStruct.Key] = true;
        }

        private void OnDisable()
        {
            enemy.onElementalDamageReceived -= RevealWeakness;
            enemy.onElementalDamageReceived -= RevealResistance;
            enemy.onStatusEffectReceived -= RevealWeakness;
            enemy.onStatusEffectReceived -= RevealResistance;
            enemy.onWeaponDamageTypeReceived -= RevealWeakness;
        }
    }
}