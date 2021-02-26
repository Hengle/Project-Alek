using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "Enemy Manager", menuName = "Singleton SO/Enemy Manager")]
    public class EnemyManager : ScriptableObjectSingleton<EnemyManager>
    {
        [InlineEditor(InlineEditorModes.FullEditor)]
        [SerializeField] private List<Enemy> enemies = new List<Enemy>();

        public static List<Enemy> Enemies => Instance.enemies;

        public static void SetBattleEnemies(IEnumerable<Enemy> battleEnemies) => 
            Instance.enemies = new List<Enemy>(battleEnemies);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}