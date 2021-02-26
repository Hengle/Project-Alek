using BattleSystem.Engine;
using UnityEngine;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Battle Engine Controller", menuName = "Singleton SO/Battle Engine Controller")]
    public class Battle : ScriptableObjectSingleton<Battle>
    {
        [SerializeField] private BattleEngine mainEngine;
        [SerializeField] private BattleEngine engineOverride;
        [SerializeField] private BattleEngine engine;

        public static BattleEngine Engine => Instance.engine;

        private bool overrideEngine;

        public static void SetEngine()
        {
            if (Instance.overrideEngine)
            {
                Instance.overrideEngine = false;
                Instance.engine = Instance.engineOverride;
            }
            else Instance.engine = Instance.mainEngine;
        }

        public static void OverrideEngine(BattleEngine battleEngine)
        {
            Instance.engineOverride = battleEngine;
            Instance.overrideEngine = true;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}