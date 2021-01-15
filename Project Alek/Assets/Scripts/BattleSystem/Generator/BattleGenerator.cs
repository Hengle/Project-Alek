using System.Linq;
using BattleSystem.Mechanics;
using BattleSystem.UI;
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
        public BattleGeneratorDatabase database;

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
            foreach (var member in BattleManager.Instance._membersForThisBattle)
            {
                SelectableObjectManager._memberSelectable.Add(member.Selectable);
                member.MenuController = member.battlePanel.GetComponent<MenuController>();
            }
        }

        private void SetupBattlePanel(PartyMember character, int i)
        {
            character.battlePanel = database.battlePanels[i];
            character.battleOptionsPanel = Instantiate(database.boPanel);
            ((BattleOptionsPanel) character.battleOptionsPanel).character = character;

            var apConversionBox = character.battlePanel.transform.Find("AP Conversion Box").gameObject;
            apConversionBox.GetComponent<APConversionControllerUI>().unit = character.Unit;
            
            var mainMenu = character.battlePanel.transform.Find("Mask").transform.Find("Battle Menu").transform.Find("Main Options");

            mainMenu.Find("Attack Button").gameObject.GetComponent<Button>().onClick.AddListener
                ( delegate { ((BattleOptionsPanel) character.battleOptionsPanel).GetCommandInformation("UniversalAction,1,0,2"); });

            mainMenu.Find("Abilities Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { ((BattleOptionsPanel) character.battleOptionsPanel).OnAbilityMenuButton(); });
            
            mainMenu.Find("Inventory Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { BattleManager.Instance.inventoryInputManager.OpenInventory(); });

            mainMenu.Find("End Turn Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { ((BattleOptionsPanel) character.battleOptionsPanel).OnEndTurnButton(); });

            character.BattlePanelAnim = character.battlePanel.GetComponent<Animator>();
            
            character.battlePanel.SetActive(false);
        }
        
        private static void SetAbilityMenuOptions(PartyMember character)
        {
            var abilityMenu = character.battlePanel.transform.Find("Mask").transform.Find("Battle Menu").transform.Find("Ability Menu").transform;
            var abilityListIndex = 0;
            
            while (character.abilities.Count > 5) character.abilities.Remove(character.abilities[character.abilities.Count-1]);

            for (var buttonIndex = 0; buttonIndex < character.abilities.Count; buttonIndex++)
            {
                var optionButton = abilityMenu.GetChild(buttonIndex).gameObject;
                optionButton.GetComponentInChildren<TextMeshProUGUI>().text = character.abilities[abilityListIndex].name;
                
                optionButton.transform.Find("Icon").GetComponent<Image>().sprite = character.abilities[abilityListIndex].icon;
                optionButton.SetActive(true);
                
                var param = character.abilities[abilityListIndex].GetParameters(abilityListIndex);
                var ability = character.abilities[abilityListIndex];
                
                optionButton.GetComponent<Button>().onClick.
                    AddListener(delegate { ((BattleOptionsPanel) character.battleOptionsPanel).GetCommandInformation(param);
                        character.Unit.currentAbility = ability; character.Unit.isAbility = true; });

                if (character.abilities[abilityListIndex].isMultiTarget) optionButton.GetComponent<Button>().onClick.
                    AddListener(delegate { ChooseTarget._isMultiTarget = true; });
                    
                optionButton.GetComponent<InfoBoxUI>().information = 
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

            if (character.equippedWeapon == null) Debug.LogError($"{character.characterName} has no weapon!");
            character.equippedWeapon.partyMember = character;
            weaponInventory.AddItem(character.equippedWeapon, 1);
            weaponInventory.Content[0].Equip();
        }

        private void SetupProfileBox(UnitBase character)
        {
            var parent = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Profiles");
            var profileBox = Instantiate(database.profileBox, parent, false);
            profileBox.gameObject.GetComponent<ProfileBoxManagerUI>().SetupProfileBox(character);
        }

        private void SetupCharacterPanel(PartyMember character, int i)
        {
            var panel = database.characterPanels[i + offset];
            var statusBoxController = panel.transform.Find("Status Effects").GetComponent<StatusEffectControllerUI>();
            
            panel.GetComponent<CharacterPanelControllerUI>().member = character;
            panel.SetActive(true);
            
            statusBoxController.member = character;
            statusBoxController.Initialize();
        }

        private static void SetupChooseTargetScript(UnitBase character)
        {
            var chooseTarget = character.Unit.gameObject.GetComponent<ChooseTarget>();
            chooseTarget.thisUnitBase = character;
            chooseTarget.enabled = true;
        }

        private void SetupEnemyStatusBox(UnitBase clone, GameObject enemyGo)
        {
            var position = enemyGo.transform.position;
            var newPosition = new Vector3(position.x, position.y + 1.5f, position.z);

            var statusBox = Instantiate(database.enemyStatusBox, newPosition,
                database.enemyStatusBox.rotation);
                
            statusBox.transform.SetParent(clone.Unit.transform);
                
            var statusBoxController = statusBox.GetComponentInChildren<StatusEffectControllerUI>();
            statusBoxController.member = clone;
            statusBoxController.Initialize();
        }

        private void SetupEnemyShield(Enemy clone, GameObject enemyGo)
        {
            var position = enemyGo.transform.position;
            var newPosition = new Vector3(position.x, position.y - 1.5f, position.z);
                
            var shieldTransform = Instantiate(database.shieldTransform, newPosition,
                database.shieldTransform.rotation);
                
            shieldTransform.transform.SetParent(clone.Unit.transform);

            var shieldController = shieldTransform.GetComponent<BreakSystemControllerUI>();
            shieldController.enemy = clone;
            shieldController.Initialize();
        }

        #endregion

        #region Spawning

        private void SpawnAndSetupThisMember(PartyMember character, int i)
        {
            var memberGo = Instantiate(character.characterPrefab, database.characterSpawnPoints[i+offset].transform);
            memberGo.transform.localScale = character.scale;
            memberGo.GetComponent<Unit>().Setup(character);

            SetupChooseTargetScript(character);
            SetupBattlePanel(character, i);
            SetAbilityMenuOptions(character);
            SetupInventoryDisplay(character, i);
            SetupProfileBox(character);
            SetupCharacterPanel(character, i);

            database.closeUpCameras[i + offset].SetActive(true);
            database.criticalCameras[i + offset].SetActive(true);

            character.Selectable = character.Unit.gameObject.GetComponent<Selectable>();
            
            BattleManager.Instance._membersForThisBattle.Add(character);
        }

        private void SpawnAndSetupEnemyTeam()
        {
            var i = 0;
            foreach (var clone in database.enemies.Select(Instantiate))
            {
                //TODO: Make randomizer for enemy stats
                clone.initiative.BaseValue = (int) Random.Range(clone.initiative.BaseValue - 2, clone.initiative.BaseValue + 2);
                
                var enemyGo = Instantiate(clone.characterPrefab, database.enemySpawnPoints[i+enemyOffset].transform);
                enemyGo.name = clone.name;
                enemyGo.transform.localScale = clone.scale;

                enemyGo.GetComponent<Unit>().Setup(clone);
                
                clone.Selectable = clone.Unit.gameObject.GetComponent<Selectable>();
                SelectableObjectManager._enemySelectable.Add(clone.Selectable);

                SetupChooseTargetScript(clone);
                SetupProfileBox(clone);
                SetupEnemyStatusBox(clone, enemyGo);
                SetupEnemyShield(clone, enemyGo);

                clone.BreakSystem = new BreakSystem(clone, clone.maxShieldCount);

                enemyGo.GetComponent<WeakAndResUnlockSystem>().Initialize(clone);

                BattleManager.Instance._enemiesForThisBattle.Add(clone);
                i++;
            }
        }

        #endregion
    }
}
