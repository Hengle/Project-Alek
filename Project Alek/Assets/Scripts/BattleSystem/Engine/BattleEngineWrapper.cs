using SingletonScriptableObject;
using UnityEngine;

namespace BattleSystem.Engine
{
    public class BattleEngineWrapper : MonoBehaviorSingleton<BattleEngineWrapper>
    {
        [SerializeField] private GameObject battleResults;
        [SerializeField] private GameObject gameOverCanvas;
        
        private void Start()
        {
            Battle.SetEngine();
            Battle.Engine.battleResults = battleResults;
            Battle.Engine.gameOverCanvas = gameOverCanvas;
            Battle.Engine.OnStart();
        }

        private void OnDisable() => Battle.Engine.OnDisabled();
    }
}