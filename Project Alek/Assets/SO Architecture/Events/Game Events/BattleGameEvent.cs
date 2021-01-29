using UnityEngine;

namespace ScriptableObjectArchitecture
{
    public enum BattleEvent { NewRound, WonBattle, LostBattle }
    [System.Serializable]
    [CreateAssetMenu(
        fileName = "BattleGameEvent.asset",
        menuName = SOArchitecture_Utility.GAME_EVENT + "Battle")]
    public class BattleGameEvent : GameEventBase<BattleEvent>
    {
    }
}