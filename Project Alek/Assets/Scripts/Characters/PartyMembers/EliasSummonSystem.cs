using System.Collections.Generic;
using System.Linq;
using BattleSystem;
using Characters.Abilities;
using Characters.Enemies;
using JetBrains.Annotations;
using MEC;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Characters.PartyMembers
{
    public enum SummonState { Summon, Command }
    public class EliasSummonSystem : MonoBehaviour, IGameEventListener<GameObject>
    {
        [SerializeField] private PartyMember elias;
        [SerializeField] private Ability summonAbility;
        [SerializeField] private SummonsDatabase database;
        [SerializeField] private Material summonMaterial;
        [SerializeField] private GameObject summonsMenuGO;

        [SerializeField] [ReadOnly] private Enemy currentSummon;
        [SerializeField] [ReadOnly] private GameObject currentSummonGO;
        [SerializeField] [ReadOnly] private GameObject activeMenu;
        [SerializeField] [ReadOnly] private GameObject commandMenuFirstSelected;
        [SerializeField] [ReadOnly] private GameObject abilityMenuLastSelected;
        [SerializeField] [ReadOnly] private SummonState summonState;
        [SerializeField] [ReadOnly] private int summonIndex;
        [SerializeField] [ReadOnly] private Enemy[] availableSummons;

        [SerializeField] private GameObject summonMenuCanvas;
        [SerializeField] private GameObject summonsMenu;
        [SerializeField] private GameObject[] summonOptionButtons;
        [SerializeField] private GameObject[] commandMenus;
        [SerializeField] private GameObject summonsMenuFirstSelected;
        [SerializeField] private GameObject summonButtonGO;
        private Controls controls;

        private void Awake()
        {
            if (!elias.abilities.Contains(summonAbility)) return;
            
            controls = new Controls();
            controls.Enable();
            controls.Battle.Back.performed += ctx => CloseSubMenu();
            
            BattleEvents.Instance.overrideButtonEvent.AddListener(this);
        }

        private void OnDisable()
        {
            if (!elias.abilities.Contains(summonAbility)) return;
            BattleEvents.Instance.overrideButtonEvent.RemoveListener(this);
        }

        private void FindDependencies(Transform summonMenuGO)
        {
            //summonsCanvas = GameObject.Find("Summons Canvas").transform;
            summonsMenu = summonMenuGO.Find("Summon Options Menu").gameObject;
            var commandMenuTransform = summonMenuGO.Find("Command Menus");
            
            summonOptionButtons = new GameObject[3];
            commandMenus = new GameObject[3];

            var i = 0;
            foreach (Transform child in summonsMenu.transform)
            {
                summonOptionButtons[i] = child.gameObject;
                i++;
            }

            i = 0;
            foreach (Transform child in commandMenuTransform)
            {
                commandMenus[i] = child.gameObject;
                i++;
            }

            summonsMenuFirstSelected = summonOptionButtons[0];
            //summonMenuGO.SetParent(elias.battlePanel.transform);
            summonMenuCanvas.SetActive(false);
        }

        private bool InstantiateGO()
        {
            // var position = summonButton.localPosition;
            // var newPosition = new Vector3(position.x, position.y, position.z + 3);
            summonMenuCanvas = Instantiate(summonsMenuGO, elias.battlePanel.transform);
            //summonMenuCanvas.transform.localPosition = newPosition;
            FindDependencies(summonMenuCanvas.transform);
            return true;
        }

        private IEnumerator<float> SetSummonOptions(GameObject summonButton)
        {
            availableSummons = database.GetEquippedSummons().ToArray();
            summonButtonGO = summonButton;
            summonButtonGO.GetComponent<Button>().onClick.AddListener(OpenSubMenu);
            
            yield return Timing.WaitUntilTrue(InstantiateGO);
            yield return Timing.WaitForOneFrame;

            for (var i = 0; i < availableSummons.Length; i++)
            {
                var button = summonOptionButtons[i];
                var summon = availableSummons[i];
                var param = summonAbility.GetParameters(elias.abilities.IndexOf(summonAbility));
                var index = i;

                button.GetComponentInChildren<TextMeshProUGUI>().text = summon.characterName;
                button.GetComponent<Button>().onClick.AddListener(delegate
                {
                    ((BattleOptionsPanel) elias.battleOptionsPanel).GetCommandInformation(param);
                    elias.Unit.currentAbility = summonAbility;
                    currentSummon = summon;
                    summonIndex = index;
                    elias.Unit.isAbility = true;
                });
                
                button.SetActive(true);
                SetCommandOptions(summon, i);
            }
        }

        private void SetCommandOptions(UnitBase summon, int index)
        {
            var menu = commandMenus[index];
            var children = new GameObject[4];
            
            var j = 0;
            foreach (Transform child in menu.transform)
            {
                children[j] = child.gameObject;
                j++;
            }
            
            for (var i = 0; i < summon.abilities.Count; i++)
            {
                children[i].GetComponentInChildren<TextMeshProUGUI>().text = summon.abilities[i].name;
                children[i].transform.Find("Icon").GetComponent<Image>().sprite = summon.abilities[i].icon;
                children[i].SetActive(true);
                
                var param = summon.abilities[i].GetParameters(i);
                var ability = summon.abilities[i];
                
                children[i].GetComponent<Button>().onClick.AddListener(delegate
                {
                    ((BattleOptionsPanel) elias.battleOptionsPanel).GetCommandInformation(param);
                    BattleInput._controls.Enable();
                    elias.Unit.currentAbility = ability;
                    elias.Unit.isAbility = true;
                });

                if (summon.abilities[i].isMultiTarget)
                    children[i].GetComponent<Button>().onClick.AddListener(delegate
                    {
                        ChooseTarget._isMultiTarget = true;
                    });
            }

            children[summon.abilities.Count].GetComponentInChildren<TextMeshProUGUI>().text = "Dismiss";
            children[summon.abilities.Count].SetActive(true);
            children[summon.abilities.Count].GetComponent<Button>().onClick.AddListener(Dismiss);
        }

        private void OpenSubMenu()
        {
            BattleInput._controls.Disable();
            abilityMenuLastSelected = EventSystem.current.currentSelectedGameObject;
            
            var position = summonButtonGO.transform.localPosition;
            var newPosition = new Vector3(position.x + 200, position.y, position.z);
            summonMenuCanvas.transform.localPosition = newPosition;
            summonMenuCanvas.SetActive(true);
            
            switch (summonState)
            {
                case SummonState.Summon:
                    activeMenu = summonsMenu;
                    activeMenu.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(summonsMenuFirstSelected);
                    break;
                case SummonState.Command:
                    activeMenu = commandMenus[summonIndex];
                    activeMenu.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(commandMenuFirstSelected);
                    break;
            }
        }

        private void CloseSubMenu()
        {
            if (BattleEngine.Instance.activeUnit != elias) return;
            if (!activeMenu.activeSelf) return;
            
            activeMenu.SetActive(false);
            summonMenuCanvas.SetActive(false);
            BattleInput._controls.Enable();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(abilityMenuLastSelected);
        }

        private void Dismiss()
        {
            // TODO: Play command animation
            // TODO: Show cool effect
            
            summonState = SummonState.Summon;
        }

        [UsedImplicitly] public void Summon()
        {
            // TODO: Use object pooling to instantiate once
            
            summonState = SummonState.Command;
            commandMenuFirstSelected = commandMenus[summonIndex].transform.GetChild(0).gameObject;
        }

        public void OnEventRaised(GameObject value)
        {
            if (!elias.abilities.Contains(summonAbility)) return;
            Timing.RunCoroutine(SetSummonOptions(value));
        }
    }
}