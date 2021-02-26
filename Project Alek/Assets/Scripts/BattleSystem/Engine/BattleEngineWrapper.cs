using SingletonScriptableObject;

namespace BattleSystem.Engine
{
    public class BattleEngineWrapper : MonoBehaviorSingleton<BattleEngineWrapper>
    {
        private void Start()
        {
            Battle.SetEngine();
            Battle.Engine.OnStart();
        }

        private void OnDisable() => Battle.Engine.OnDisabled();
    }
}