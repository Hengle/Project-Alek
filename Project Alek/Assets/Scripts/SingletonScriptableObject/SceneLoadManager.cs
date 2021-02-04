using MEC;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Scene Load Manager")]
    public class SceneLoadManager : SingletonScriptableObject<SceneLoadManager>
    {
        private AsyncOperation operation;

        public void LoadBattle()
        {
            operation = SceneManager.LoadSceneAsync("Scenes/Battle");
            operation.allowSceneActivation = false;
            Timing.RunCoroutine(SceneTransitionManager.Instance.BattleTransition().
                Append(() => operation.allowSceneActivation = true));
        }

        public void LoadOverworld()
        {
            operation = SceneManager.LoadSceneAsync("Overworld Demo");
            operation.allowSceneActivation = false;
            Timing.RunCoroutine(SceneTransitionManager.Instance.OverworldTransition(1, true).
                Append(() => operation.allowSceneActivation = true));

            //SceneManager.LoadScene("Scenes/Overworld Essentials", LoadSceneMode.Additive);
        }
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}