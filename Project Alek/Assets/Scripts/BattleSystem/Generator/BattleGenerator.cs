using System.Linq;
using Characters;
using UnityEngine;
using UnityEngine.UI;
using Characters.PartyMembers;
using Characters.StatusEffects;
using MoreMountains.InventoryEngine;
using TMPro;
using Random = UnityEngine.Random;

namespace BattleSystem.Generator
{
    public class BattleGenerator : MonoBehaviour
    {
        private BattleGeneratorDatabase database;

        private int offset;
        private int enemyOffset;

        private void Awake() => database = GetComponent<BattleGeneratorDatabase>();
        
        #region Setup

        public void SetupBattle()
        {
            if (PartyManager._instance.partyMembers.Count == 1) offset = 2;
            else if (PartyManager._instance.partyMembers.Count == 4) offset = 0;
            else offset = 1;
            
            if (database.enemies.Count == 1) enemyOffset = 2;
            else if (database.enemies.Count == 4) enemyOffset = 0;
            else enemyOffset = 1;

            var inventory = GameObject.Find("Main Inventory").GetComponent<Inventory>();

            database.inventoryItems.ForEach(i => inventory.AddItem(i, 1));

            SetupParty();
            SpawnAndSetupEnemyTeam();
            SetupPartyMenuController();
        }

        private void SetupParty()
        {
            for (var i = 0; i < PartyManager._instance.partyMembers.Count; i++)
            {
                SpawnAndSetupThisMember(PartyManager._instance.partyMembers[i], i);
            }
        }

        private static void SetupPartyMenuController()
        {
            foreach (var member in BattleManager.MembersForThisBattle)
            {
                foreach (var partyMember in BattleManager.MembersForThisBattle)
                {
                    member.battlePanel.GetComponent<MenuController>().memberSelectable.Add(partyMember.Unit.gameObject);
                }
            }
        }

        private void SetupBattlePanel(PartyMember character)
        {
            var position = character.Unit.transform.position;
            var newPosition = new Vector3(position.x, position.y + 1.5f, position.z + 3);
            var rotation = database.boPanel.battlePanel.transform.rotation;
            
            character.battlePanel = Instantiate(database.boPanel.battlePanel, newPosition, rotation);
            character.battlePanel.transform.SetParent(character.Unit.transform.parent, true);
            
            character.battleOptionsPanel = Instantiate(database.boPanel);
            ((BattleOptionsPanel) character.battleOptionsPanel).character = character;
            
            var mainMenu = character.battlePanel.transform.Find("Battle Menu").transform.Find("Main Options").transform;

            mainMenu.Find("Attack Button").gameObject.GetComponent<Button>().onClick.AddListener
                ( delegate { ((BattleOptionsPanel) character.battleOptionsPanel).GetCommandInformation("UniversalAction,1,0,2"); });

            mainMenu.Find("Abilities Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { ((BattleOptionsPanel) character.battleOptionsPanel).OnAbilityMenuButton(); });
            
            mainMenu.Find("Inventory Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { BattleManager._inventoryInputManager.OpenInventory(); });

            mainMenu.Find("End Turn Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { ((BattleOptionsPanel) character.battleOptionsPanel).OnEndTurnButton(); });

            character.actionPointAnim = character.battlePanel.transform.Find("AP Box").GetComponent<Animator>();
            character.battlePanel.SetActive(false);
        }
        
        private static void SetAbilityMenuOptions(PartyMember character)
        {
            var abilityMenu = character.battlePanel.transform.Find("Battle Menu").transform.Find("Ability Menu").transform;
            var abilityListIndex = 0;
            
            while (character.abilities.Count > 5) character.abilities.Remove(character.abilities[character.abilities.Count-1]);

            for (var buttonIndex = 0; buttonIndex < character.abilities.Count; buttonIndex++)
            {
                var optionButton = abilityMenu.GetChild(buttonIndex).gameObject;
                optionButton.GetComponentInChildren<TextMeshPro>().text = character.abilities[abilityListIndex].name;
                
                optionButton.transform.Find("Icon").GetComponent<Image>().sprite = character.abilities[abilityListIndex].icon;
                optionButton.SetActive(true);
                
                var param = character.abilities[abilityListIndex].GetParameters(abilityListIndex);
                
                optionButton.GetComponent<Button>().onClick.
                    AddListener(delegate { ((BattleOptionsPanel) character.battleOptionsPanel).GetCommandInformation(param); });

                if (character.abilities[abilityListIndex].isMultiTarget) optionButton.GetComponent<Button>().onClick.
                    AddListener(delegate { ChooseTarget._isMultiTarget = true; });
                    
                optionButton.GetComponent<InfoBoxScript>().information = 
                    $"{character.abilities[abilityListIndex].description} ( {character.abilities[abilityListIndex].actionCost} AP )";
                
                abilityListIndex++;

                if (buttonIndex != character.abilities.Count - 1) continue;

                var firstOption = abilityMenu.GetChild(0).gameObject;
                var firstOpNav = firstOption.GetComponent<Selectable>().navigation;
                var nav = optionButton.GetComponent<Selectable>().navigation;

                nav.selectOnDown = firstOption.GetComponent<Button>();
                optionButton.GetComponent<Selectable>().navigation = nav;

                firstOpNav.selectOnUp = optionButton.GetComponent<Button>();
                firstOption.GetComponent<Selectable>().navigation = firstOpNav;
            }
        }

        private void SetupInventoryDisplay(PartyMember character, int i)
        {
            var inventory = GameObject.Find("Main Inventory").GetComponent<Inventory>();

            var weaponInventory = Instantiate(character.weaponInventory, character.Unit.transform, true);
            weaponInventory.name = character.weaponInventory.name;

            var armorInventory = Instantiate(character.armorInventory, character.Unit.transform, true);
            armorInventory.name = character.armorInventory.name;

            character.inventoryDisplay = database.inventoryCanvases[i];
            
            var mainDisplay = character.inventoryDisplay.transform.Find("InventoryDisplay").GetComponent<InventoryDisplay>();
            mainDisplay.TargetInventoryName = $"{inventory.name}";
            mainDisplay.SetupInventoryDisplay();

            var weaponDisplay = character.inventoryDisplay.transform.Find("Weapon Display").GetComponent<InventoryDisplay>();
            weaponDisplay.TargetInventoryName = $"{character.weaponInventory.name}";
            weaponDisplay.SetupInventoryDisplay();

            var armorDisplay = character.inventoryDisplay.transform.Find("Armor Display").GetComponent<InventoryDisplay>();
            armorDisplay.TargetInventoryName = $"{character.armorInventory.name}";
            armorDisplay.SetupInventoryDisplay();

            var details = character.inventoryDisplay.GetComponentInChildren<InventoryDetails>();
            details.TargetInventoryName = $"{inventory.name}";
        }

        private void SetupProfileBox(UnitBase character)
        {
            var parent = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Profiles");
            var profileBox = Instantiate(database.profileBox, parent, false);
            profileBox.gameObject.GetComponent<ProfileBoxManager>().SetupProfileBox(character);
        }

        #endregion

        #region Spawning

        private void SpawnAndSetupThisMember(PartyMember character, int i)
        {
            var memberGo = Instantiate(character.characterPrefab, database.characterSpawnPoints[i+offset].transform);
            memberGo.GetComponent<Unit>().Setup(character);
            
            var chooseTarget = character.Unit.gameObject.GetComponent<ChooseTarget>();
            chooseTarget.thisUnitBase = character;
            chooseTarget.enabled = true;

            SetupBattlePanel(character);
            SetAbilityMenuOptions(character);
            SetupInventoryDisplay(character, i);
            SetupProfileBox(character);

            memberGo.transform.localScale = character.scale;
            
            var panel = database.characterPanels[i + offset];
            var statusBoxController = panel.transform.GetChild(panel.transform.childCount - 3).GetComponent<StatusEffectControllerUI>();
            
            panel.GetComponent<CharacterPanelController>().member = character;
            panel.SetActive(true);
            
            statusBoxController.member = character;
            statusBoxController.Initialize();

            database.closeUpCameras[i + offset].SetActive(true);
            database.criticalCameras[i + offset].SetActive(true);

            BattleManager.MembersForThisBattle.Add(character);
        }

        private void SpawnAndSetupEnemyTeam()
        {
            var i = 0;
            foreach (var clone in database.enemies.Select(Instantiate))
            {
                clone.initiative.BaseValue = (int) Random.Range(clone.initiative.BaseValue - 2, clone.initiative.BaseValue + 2); // Temporary until i make a randomizer
                
                var enemyGo = Instantiate(clone.characterPrefab, database.enemySpawnPoints[i+enemyOffset].transform);
                enemyGo.name = clone.name;
                enemyGo.transform.localScale = clone.scale;
                
                foreach (var partyMember in BattleManager.MembersForThisBattle)
                {
                    partyMember.battlePanel.GetComponent<MenuController>().enemySelectable.Add(enemyGo);
                }
                
                enemyGo.GetComponent<Unit>().Setup(clone);
                
                var chooseTarget = clone.Unit.gameObject.GetComponent<ChooseTarget>();
                chooseTarget.thisUnitBase = clone;
                chooseTarget.enabled = true;
                
                SetupProfileBox(clone);

                var position = enemyGo.transform.position;
                var newPosition = new Vector3(position.x, position.y + 1.5f, position.z);

                var statusBox = Instantiate(database.enemyStatusBox, newPosition,
                    database.enemyStatusBox.rotation);
                
                statusBox.transform.SetParent(clone.Unit.transform);
                
                var statusBoxController = statusBox.GetComponentInChildren<StatusEffectControllerUI>();
                statusBoxController.member = clone;
                statusBoxController.Initialize();

                clone.stateMachine = new UnitStateMachine(clone, clone.shieldCount);
                
                newPosition = new Vector3(position.x, position.y - 1.5f, position.z);
                
                var requirementBox = Instantiate(database.requirementsBox, newPosition,
                    database.requirementsBox.rotation);
                
                requirementBox.transform.SetParent(clone.Unit.transform);

                // var requirementBoxController = requirementBox.GetComponentInChildren<RequirementBoxControllerUI>();
                // requirementBoxController.enemy = clone;
                // requirementBoxController.Initialize();

                BattleManager.EnemiesForThisBattle.Add(clone);
                i++;
            }
        }

        #endregion
    }
}
