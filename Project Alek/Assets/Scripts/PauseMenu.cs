using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
        controls.Overworld.OpenPauseMenu.performed += OpenPauseMenu;
        controls.UI.Cancel.performed += CloseCurrentMenu;
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Overworld.OpenPauseMenu.performed -= OpenPauseMenu;
        controls.UI.Cancel.performed -=CloseCurrentMenu;
    }

    private void OpenPauseMenu(InputAction.CallbackContext ctx)
    {
        if (menu.activeSelf) return;
        TimeManager.PauseTime();
        menu.SetActive(true);
        SetMainMenuFirstSelected();
    }

    private void CloseCurrentMenu(InputAction.CallbackContext ctx)
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

    public void ClosePauseMenu()
    {
        if (!menu.activeSelf) return;
        menu.SetActive(false);
        TimeManager.ResumeTime();
    }

    public void OpenSettingsMenu()
    {
        settingsMenu.SetActive(true);
        SetSettingsMenuFirstSelected();
    }

    public void OnQuitButton()
    {
        // TODO: Go to main menu scene instead of quitting
        Application.Quit();
    }
}