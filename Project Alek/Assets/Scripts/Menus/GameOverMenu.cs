using System.Collections.Generic;
using Audio;
using MEC;
using ScriptableObjectArchitecture;
using SingletonScriptableObject;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menus
{
    public class GameOverMenu : MonoBehaviour
    {
        [SerializeField] private Image screen;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private GameObject retryButton;
        [SerializeField] private GameObject quitButton;
        [SerializeField] private GameObject statPanelCanvas;
        [SerializeField] private GameObject turnOrderCanvas;
        [SerializeField] private GameEvent retryBattleEvent;

        private void OnEnable()
        {
            statPanelCanvas.SetActive(false);
            turnOrderCanvas.SetActive(false);
            
            AudioController.PlayAudio(CommonAudioTypes.GameOver);
            Timing.RunCoroutine(FadeToBlack().Append
                (ShowTextAndButtons().Append(SetFirstSelected)));
        }

        private void SetFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(retryButton);
        }

        private IEnumerator<float> FadeToBlack()
        {
            var fixedColor = screen.color;
            fixedColor.a = 1;
            screen.color = fixedColor;
            screen.CrossFadeAlpha(0f, 0f, true);
            screen.CrossFadeAlpha(1, 2, false);
            yield return Timing.WaitForSeconds(2f);
        }

        private IEnumerator<float> ShowTextAndButtons()
        {
            gameOverText.gameObject.SetActive(true);

            yield return Timing.WaitForSeconds(1);
            
            retryButton.gameObject.SetActive(true);
            quitButton.gameObject.SetActive(true);
        }

        public void OnRetryButton() => SceneLoadManager.LoadBattle(true);

        public void OnQuitButton()
        {
            SceneLoadManager.LoadScene("Start");
            gameObject.SetActive(false);
        }
    }
}