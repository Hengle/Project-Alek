using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Add AudioTypes to this class that are called from scripts that are attached to multiple different prefabs. This way
    /// You do not have to put the reference for the AudioType for each prefab. Make sure these sounds are on
    /// the AudioController prefab for that scene
    /// </summary>
    public class CommonAudioTypes : MonoBehaviorSingleton<CommonAudioTypes>
    {
        [SerializeField] private AudioType mainBattleTheme;
        public static AudioType MainBattleTheme => Instance.mainBattleTheme;
        
        [SerializeField] private AudioType victoryThemeBattle;
        public static AudioType VictoryThemeBattle => Instance.victoryThemeBattle;
        
        [SerializeField] private AudioType hitWindow;
        public static AudioType HitWindow => Instance.hitWindow;
        
        [SerializeField] private AudioType levelUp;
        public static AudioType LevelUp => Instance.levelUp;
        
        [SerializeField] private AudioType aPConversion;
        public static AudioType APConversion => Instance.aPConversion;
        
        [SerializeField] private AudioType gameOver;
        public static AudioType GameOver => Instance.gameOver;
        
        [SerializeField] private AudioType victory;
        public static AudioType Victory => Instance.victory;
        
        [SerializeField] private AudioType heal;
        public static AudioType Heal => Instance.heal;
        
        [SerializeField] private AudioType revive;
        public static AudioType Revive => Instance.revive;
        
        [SerializeField] private AudioType chestOpen;
        public static AudioType ChestOpen => Instance.chestOpen;
    }
}