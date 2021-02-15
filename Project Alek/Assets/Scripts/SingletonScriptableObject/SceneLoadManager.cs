using UnityEngine.SceneManagement;
using UnityEngine;
using MEC;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Scene Load Manager", menuName = "Singleton SO/Scene Load Manager")]
    public class SceneLoadManager : ScriptableObjectSingleton<SceneLoadManager>
    {
        [SerializeField] private AsyncOperation operation;
        [SerializeField] private string previousScene;

        public void LoadBattle(bool restart = false)
        {
            if (!restart) previousScene = SceneManager.GetActiveScene().path;
            operation = SceneManager.LoadSceneAsync("Scenes/Battle");
            operation.allowSceneActivation = false;
            Timing.RunCoroutine(SceneTransitionManager.Instance.BattleTransition()
                .Append(() => operation.allowSceneActivation = true));
        }

        public void LoadOverworld()
        {
            operation = SceneManager.LoadSceneAsync(previousScene);
            operation.allowSceneActivation = false;
            Timing.RunCoroutine(SceneTransitionManager.Instance.OverworldTransition(1, true).
                Append(() => operation.allowSceneActivation = true));
        }
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}