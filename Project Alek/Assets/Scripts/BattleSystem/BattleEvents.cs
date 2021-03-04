using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleSystem
{
    public class BattleEvents : MonoBehaviorSingleton<BattleEvents>
    {
        [FoldoutGroup("Events")] [SerializeField] 
        private GameEvent setupCompleteEvent;
        public static GameEvent SetupCompleteEvent => Instance.setupCompleteEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private BattleGameEvent battleEvent;
        public static BattleGameEvent NormalEvent => Instance.battleEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent characterTurnEvent;
        public static CharacterGameEvent CharacterTurnEvent => Instance.characterTurnEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent enemyTurnEvent;
        public static CharacterGameEvent EnemyTurnEvent => Instance.enemyTurnEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent chooseTargetEvent;
        public static CharacterGameEvent ChooseTargetEvent => Instance.chooseTargetEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent endOfTurnEvent;
        public static CharacterGameEvent EndOfTurnEvent => Instance.endOfTurnEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent skipTurnEvent;
        public static CharacterGameEvent SkipTurnEvent => Instance.skipTurnEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent characterAttackEvent;
        public static CharacterGameEvent CharacterAttackEvent => Instance.characterAttackEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent commandEvent;
        public static CharacterGameEvent CommandEvent => Instance.commandEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private CharacterGameEvent deathEvent;
        public static CharacterGameEvent DeathEvent => Instance.deathEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private GameEvent thisTurnListCreatedEvent;
        public static GameEvent ThisTurnListCreatedEvent => Instance.thisTurnListCreatedEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private GameEvent nextTurnListCreatedEvent;
        public static GameEvent NextTurnListCreatedEvent => Instance.nextTurnListCreatedEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private GameEvent retryBattleEvent;
        public static GameEvent RetryBattleEvent => Instance.retryBattleEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private GameEvent resortNextTurnEvent;
        public static GameEvent ResortNextTurnEvent => Instance.resortNextTurnEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private GameObjectGameEvent overrideButtonEvent;
        public static GameObjectGameEvent OverrideButtonEvent => Instance.overrideButtonEvent;
        
        [FoldoutGroup("Events")] [SerializeField]
        private GameEvent fleeBattleEvent;
        public static GameEvent FleeBattleEvent => Instance.fleeBattleEvent;
    }
}