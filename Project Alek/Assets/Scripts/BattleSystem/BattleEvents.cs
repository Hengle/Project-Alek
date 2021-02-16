using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleSystem
{
    public class BattleEvents : MonoBehaviorSingleton<BattleEvents>
    {
        [FoldoutGroup("Events")] [SerializeField] 
        public GameEvent setupCompleteEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public BattleGameEvent battleEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public CharacterGameEvent characterTurnEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public CharacterGameEvent enemyTurnEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public CharacterGameEvent chooseTargetEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public CharacterGameEvent endOfTurnEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public CharacterGameEvent skipTurnEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public CharacterGameEvent characterAttackEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public CharacterGameEvent commandEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public CharacterGameEvent deathEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public GameEvent thisTurnListCreatedEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public GameEvent nextTurnListCreatedEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public GameEvent retryBattleEvent;
        [FoldoutGroup("Events")] [SerializeField]
        public GameEvent resortNextTurnEvent;
    }
}