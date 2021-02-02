using System.Collections.Generic;
using Characters.Enemies;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Enemy Manager")]
    public class EnemyManager : SingletonScriptableObject<EnemyManager>
    {
        [InlineEditor(InlineEditorModes.FullEditor)]
        public List<Enemy> enemies = new List<Enemy>();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}