using Abilities;
using BattleSystem;
using UnityEngine;

namespace Characters
{
    // Update this later so that each enemy type is a different script obj, not just EnemyData
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Character/Enemy")]
    public class Enemy : UnitBase, IUnitBase
    {
        public void GiveCommand(bool isSwapping)
        {
            BattleHandler.battleFuncs.GetCommand(this,false);
            BattleHandler.performingAction = true;
        }

        public void SetupUnit()
        {
            unit.id = 0;
            unit.level = level;
            unit.status = Status.Normal;
            unit.unitName = characterName;
            unit.nameText.text = characterName.ToUpper();
            unit.CurrentHP = health;
            unit.maxHealthRef = health;
            unit.currentStrength = strength;
            unit.currentMagic = magic;
            unit.currentAccuracy = accuracy;
            unit.currentInitiative = initiative;
            unit.currentCrit = criticalChance;
            unit.currentDefense = defense;
            unit.currentAP = maxAP;
            unit.iUnitRef = this;
        }

        public bool SetAI()
        {
            var rand = Random.Range(0, BattleHandler.membersForThisBattle.Count);
            unit.currentTarget = BattleHandler.membersForThisBattle[rand].GetUnit();
            unit.commandActionName = "UniversalAction";
            unit.commandActionOption = 1;
            // When I add abilities / more options to enemies, set AI to check cost and if it can use ability
            unit.actionCost = 2;

            return true;
        }

        public Ability GetAndSetAbility(int index)
        {
            unit.currentAbility = abilities[index];
            return abilities[index];
        }
        
        public void SetUnitGO(GameObject enemyGO) => unit = enemyGO.GetComponent<Unit>();
    }
}
