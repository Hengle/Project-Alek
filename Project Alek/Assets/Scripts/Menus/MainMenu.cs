using System.Collections.Generic;
using Audio;
using MEC;
using SingletonScriptableObject;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using AudioType = Audio.AudioType;
using Object = UnityEngine.Object;

namespace Menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Object scene;
        [SerializeField] private AudioType theme;
        [SerializeField] private GameObject menu;
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private GameObject mainMenuFirstSelected;
        [SerializeField] private GameObject settingsMenuFirstSelected;
    
        private Controls controls;

        private void Awake()
        {
            controls = new Controls();
            controls.UI.Cancel.performed += CloseSettingsMenu;
            controls.Enable();
            Invoke(nameof(SetMainMenuFirstSelected), 2);
        }

        private void Start()
        {
            AudioController.Instance.PlayAudio(theme);
        }

        private void OnDisable()
        {
            controls.UI.Cancel.performed -= CloseSettingsMenu;
        }

        private void CloseSettingsMenu(InputAction.CallbackContext ctx)
        {
            if (!settingsMenu.activeSelf) return;
            settingsMenu.SetActive(false);
            SetMainMenuFirstSelected();
        }

        private void SetMainMenuFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
        }

        private void SetSettingsMenuFirstSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(settingsMenuFirstSelected);
        }

        private IEnumerator<float> StartGameCoroutine()
        {
            AudioController.Instance.StopAudio(theme, true, 2);
            yield return Timing.WaitForSeconds(1);
            //SceneLoadManager.Instance.LoadScene(scene.name);
            SceneLoadManager.Instance.LoadScene("FAE_Demo1");
        }

        public void OnStartButton()
        {
            Timing.RunCoroutine(StartGameCoroutine());
        }
    
        public void OpenSettingsMenu()
        {
            settingsMenu.SetActive(true);
            SetSettingsMenuFirstSelected();
        }

        public void OnQuitButton()
        {
            Application.Quit();
        }
    }
}