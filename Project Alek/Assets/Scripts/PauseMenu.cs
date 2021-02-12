using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject mainMenuFirstSelected;
    [SerializeField] private GameObject settingsMenuFirstSelected;
    
    private Controls controls;

    private void Awake()
    {
        controls = new Controls();
        controls.Overworld.OpenPauseMenu.performed += ctx => OpenPauseMenu();
        controls.UI.Cancel.performed += ctx => CloseCurrentMenu();
        controls.Enable();
    }

    private void OpenPauseMenu()
    {
        if (menu.activeSelf) return;
        TimeManager.PauseTime();
        menu.SetActive(true);
        SetMainMenuFirstSelected();
    }

    public void OpenSettingsMenu()
    {
        settingsMenu.SetActive(true);
        SetSettingsMenuFirstSelected();
    }

    public void CloseCurrentMenu()
    {
        if (settingsMenu.activeSelf)
        {
            settingsMenu.SetActive(false);
            SetMainMenuFirstSelected();
        }
        else if (menu.activeSelf)
        {
            menu.SetActive(false);
            TimeManager.ResumeTime();
        }
    }

    public void OnQuitButton()
    {
        Application.Quit();
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
}