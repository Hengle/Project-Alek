using MEC;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagement
{
    [CreateAssetMenu(fileName = "Scene Load Manager", menuName = "Singleton SO/Scene Load Manager")]
    public class SceneLoadManager : ScriptableObjectSingleton<SceneLoadManager>
    {
        [SerializeField] private AsyncOperation operation;
        [SerializeField] private string previousScene;

        public static void LoadBattle(bool restart = false)
        {
            if (!restart) Instance.previousScene = SceneManager.GetActiveScene().path;
            Instance.operation = SceneManager.LoadSceneAsync("Scenes/Battle");
            Instance.operation.allowSceneActivation = false;
            Timing.RunCoroutine(SceneTransitionManager.Instance.BattleTransition()
                .Append(() => Instance.operation.allowSceneActivation = true));
        }

        public static void LoadOverworld()
        {
            Instance.operation = SceneManager.LoadSceneAsync(Instance.previousScene);
            Instance.operation.allowSceneActivation = false;
            Timing.RunCoroutine(SceneTransitionManager.Instance.OverworldTransition(1, true).
                Append(() => Instance.operation.allowSceneActivation = true));
        }

        public static void LoadScene(string scene)
        {
            Timing.RunCoroutine(SceneTransitionManager.Instance.OverworldTransition(1, true).Append(() =>
            {
                Instance.operation = SceneManager.LoadSceneAsync(scene);
            }));
        }
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}