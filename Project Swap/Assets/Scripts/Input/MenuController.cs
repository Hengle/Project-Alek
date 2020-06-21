using System;
using System.Collections;
using System.Collections.Generic;
using BattleSystem;
using Characters;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;

public class MenuController : MonoBehaviour
{
    public List<GameObject> enemySelectable = new List<GameObject>();
    public List<GameObject> memberSelectable = new List<GameObject>();
    
    [SerializeField]
    private GameObject mainMenu;
    [SerializeField]
    private GameObject abilityMenu;
    [SerializeField]
    private GameObject mainMenuFirstSelected;
    [SerializeField]
    private GameObject abilityMenuFirstSelected;

    private GameObject swapButtonGO;
    private Button swapButton;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        mainMenu = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        mainMenuFirstSelected = mainMenu.transform.GetChild(0).gameObject;
        abilityMenu = transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;
        abilityMenuFirstSelected = abilityMenu.transform.GetChild(0).gameObject;
        swapButton = abilityMenu.transform.GetChild(0).GetComponent<Button>();
        swapButtonGO = swapButton.transform.GetChild(1).gameObject;
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
    }

    private void Update()
    {
        swapButton.enabled = !BattleHandler.partyHasChosenSwap;
    }

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);

        if (BattleHandler.partyHasChosenSwap) swapButton.enabled = false;
    }
    
    public void SetMainMenuFirstSelected()
    {
        swapButtonGO.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(mainMenuFirstSelected);
    }

    public void SetAbilityMenuFirstSelected()
    {
        swapButtonGO.SetActive(!swapButton.enabled);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(abilityMenuFirstSelected);
    }

    public void SetTargetFirstSelected()
    {
        switch (ChooseTarget.targetOptions)
        {
            case 0:
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(enemySelectable[0]);
                break;
            case 1:
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(memberSelectable[0]);
                break;
            case 2:
                break;
        }
    }

    public bool SetSelectables()
    {
        for (var i = 0; i < enemySelectable.Count; i++)
        {
            var unit = enemySelectable[i].GetComponent<Unit>();
            var nav = unit.GetComponent<Selectable>().navigation;

            nav.selectOnDown = i + 1 < enemySelectable.Count ?
                enemySelectable[i + 1].gameObject.GetComponent<Button>() : enemySelectable[0].gameObject.GetComponent<Button>();
            
            if (i - 1 >= 0) nav.selectOnUp = enemySelectable[i - 1].gameObject.GetComponent<Button>();
            else if (i == 0) nav.selectOnUp = enemySelectable[enemySelectable.Count-1].gameObject.GetComponent<Button>();

            unit.button.navigation = nav;
        }
        
        for (var i = 0; i < memberSelectable.Count; i++)
        {
            var unit = memberSelectable[i].GetComponent<Unit>();
            var nav = unit.GetComponent<Selectable>().navigation;

            nav.selectOnDown = i + 1 < memberSelectable.Count ?
                memberSelectable[i + 1].gameObject.GetComponent<Button>() : memberSelectable[0].gameObject.GetComponent<Button>();
            
            if (i - 1 >= 0) nav.selectOnUp = memberSelectable[i - 1].gameObject.GetComponent<Button>();
            else if (i == 0) nav.selectOnUp = memberSelectable[memberSelectable.Count-1].gameObject.GetComponent<Button>();

            unit.button.navigation = nav;
        }
        
        return true;
    }
}
