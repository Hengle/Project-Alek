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
        [SerializeField] public AudioType mainBattleTheme;
        [SerializeField] public AudioType victoryThemeBattle;
        [SerializeField] public AudioType hitWindow;
        [SerializeField] public AudioType levelUp;
        [SerializeField] public AudioType aPConversion;
        [SerializeField] public AudioType gameOver;
        [SerializeField] public AudioType victory;
        [SerializeField] public AudioType heal;
        [SerializeField] public AudioType revive;
    }
}