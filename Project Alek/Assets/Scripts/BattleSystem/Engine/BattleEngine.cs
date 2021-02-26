using System.Collections.Generic;
using Characters;
using Characters.Enemies;
using Characters.PartyMembers;

namespace BattleSystem.Engine
{
    public abstract class BattleEngine : BattleEngineFieldsAndProperties
    {
        public abstract void OnStart();

        public abstract void OnDisabled();
        
        protected abstract IEnumerator<float> SetupBattle();

        protected abstract IEnumerator<float> GetNextTurn();

        protected abstract IEnumerator<float> ThisPlayerTurn(PartyMember character);

        protected abstract IEnumerator<float> ThisEnemyTurn(Enemy enemy);
        
        protected abstract IEnumerator<float> WonBattleSequence();

        protected abstract IEnumerator<float> LostBattleSequence();

        protected abstract IEnumerator<float> FleeBattleSequence();
        
        protected abstract void EndOfBattle();

        protected abstract void RemoveFromTurn(UnitBase unit);

        protected abstract void RemoveFromBattle(UnitBase unit);

        protected abstract void AddToBattle(UnitBase unit);
    }
}