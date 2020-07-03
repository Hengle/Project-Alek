using System.Linq;
using Characters;
using UnityEngine;
using UnityEngine.UI;
using Characters.PartyMembers;
using Characters.StatusEffects;
using MoreMountains.InventoryEngine;
using Input;

/*
 * Note that this could be changed later to inherit from a base Script Object that has the functions to call.
 * This way I can make new script objects for different battle generators based on the area you are in or boss battles
 */
namespace BattleSystem.Generator
{
    public class BattleGenerator : MonoBehaviour
    {
        [SerializeField] private BattleGeneratorDatabase battleGeneratorDatabase;

        private int offset;
        private int enemyOffset;

        #region Setup

        public bool SetupBattle()
        {
            offset = PartyManager._instance.partyMembers.Count == 2? 1 : 0;
            enemyOffset = battleGeneratorDatabase.enemies.Count == 2? 1 : 0;
            
            SetupParty();
            SpawnEnemyTeam();
            SetupPartyMenuController();
            return false;
        }

        private void SetupParty()
        {
            for (var i = 0; i < PartyManager._instance.partyMembers.Count; i++)
                SpawnThisMember(PartyManager._instance.partyMembers[i], i);
        }

        private static void SetupPartyMenuController()
        {
            foreach (var member in BattleManager._membersForThisBattle) {
                foreach (var partyMember in BattleManager._membersForThisBattle)
                    member.battlePanel.GetComponent<MenuController>().memberSelectable.Add(partyMember.Unit.gameObject);
            }
        }

        private void SetupBattlePanel(PartyMember character)
        {
            // Set variables for the position the battle panel should be in when instantiated
            var position = character.Unit.transform.position;
            var newPosition = new Vector3(position.x, position.y + 1.5f, position.z + 3);
            var rotation = battleGeneratorDatabase.boPanel.battlePanel.transform.rotation;
            
            // Instantiate the battle panel gameObject
            character.battlePanel = Instantiate(battleGeneratorDatabase.boPanel.battlePanel, newPosition, rotation);
            character.battlePanel.transform.SetParent(character.Unit.transform.parent, true);

            // Create an instance of the battle panel SO
            character.battleOptionsPanel = Instantiate(battleGeneratorDatabase.boPanel);
            character.battleOptionsPanel.character = character;
            
            // Set up the menu options
            var mainMenu = character.battlePanel.transform.Find("Battle Menu").transform.Find("Main Options").transform;

            mainMenu.Find("Attack Button").gameObject.GetComponent<Button>().onClick.AddListener
                ( delegate { character.battleOptionsPanel.GetCommandInformation("UniversalAction,1,0,2"); });

            mainMenu.Find("Abilities Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { character.battleOptionsPanel.OnAbilityMenuButton(); });
            
            mainMenu.Find("Inventory Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { BattleInputManager._inventoryInputManager.OpenInventory(); });

            mainMenu.Find("End Turn Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { character.battleOptionsPanel.OnEndTurnButton(); });
            
            character.SetAbilityMenuOptions();

            character.Unit.battlePanelRef = character.battlePanel;
            character.battlePanel.SetActive(false);
            
            character.actionPointAnim = character.Unit.battlePanelRef.transform.Find("AP Box").GetComponent<Animator>();
        }

        private void SetupInventoryDisplay(PartyMember character, int i)
        {
            var inventory = Instantiate(character.inventory, character.Unit.transform, true);
            inventory.name = character.inventory.name;

            character.inventoryDisplay = battleGeneratorDatabase.inventoryCanvases[i];
            var display = character.inventoryDisplay.GetComponentInChildren<InventoryDisplay>();
            display.TargetInventoryName = $"{character.inventory.name}";

            var details = character.inventoryDisplay.GetComponentInChildren<InventoryDetails>();
            details.TargetInventoryName = $"{character.inventory.name}";
            display.SetupInventoryDisplay();
        }

        private void SetupProfileBox(UnitBase character)
        {
            var parent = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Profiles");

            var profileBox = Instantiate(battleGeneratorDatabase.profileBox, parent, false);
            profileBox.name = $"{character.characterName} profile";
            
            profileBox.gameObject.GetComponent<ProfileBoxManager>().SetupProfileBox(character);
            //var manager = profileBox.gameObject.GetComponent<ProfileBoxManager>();
            //Logger.Log(character.characterPrefab.gameObject.GetComponent<SpriteRenderer>().sprite.name);
            
            // manager.spriteImage.sprite = character.characterPrefab.GetComponent<SpriteRenderer>().sprite;
            // manager.description.text = character.description;
            // manager.background.color = character.profileBoxColor;
        }

        #endregion

        #region Spawning

        private void SpawnThisMember(PartyMember character, int i)
        {
            var memberGo = Instantiate(character.characterPrefab, battleGeneratorDatabase.characterSpawnPoints[i+offset].transform);
            
            character.SetupUnit(memberGo);
            SetupBattlePanel(character);
            SetupInventoryDisplay(character, i);
            SetupProfileBox(character);

            memberGo.transform.localScale = character.scale;
            
            // Setup the character panel and the status effect box
            var panel = battleGeneratorDatabase.characterPanels[i + offset];
            var statusBoxController = panel.transform.GetChild(panel.transform.childCount - 1).GetComponent<StatusEffectControllerUI>();
            
            panel.GetComponent<CharacterPanelController>().member = character;
            panel.SetActive(true);
            statusBoxController.member = character;
            statusBoxController.Initialize();

            character.Unit.parent = battleGeneratorDatabase.characterSpawnPoints[i+offset];

            battleGeneratorDatabase.closeUpCameras[i + offset].SetActive(true);
            battleGeneratorDatabase.criticalCameras[i + offset].SetActive(true);
            
            BattleManager._membersForThisBattle.Add(character);
        }

        private void SpawnEnemyTeam()
        {
            // Spawning normal encounters needs to draw from a random list of enemies based on the overworld enemy that initiates battle

            var i = 0;
            foreach (var clone in battleGeneratorDatabase.enemies.Select(Instantiate))
            {
                /* I could just do this, but for all stats with a slight level randomizer. The personas in P5 stay at the level
                 you encounter them (jack frost is only really at the beginning) so this could be the same way. If i want a higher
                 level version of an enemy, create a new enemy that is a variant of the original
                 */
                clone.initiative = Random.Range(clone.initiative - 2, clone.initiative + 2); // Temporary until i make a randomizer
                
                var enemyGo = Instantiate(clone.characterPrefab, battleGeneratorDatabase.enemySpawnPoints[i+enemyOffset].transform);
                enemyGo.name = clone.name;
                enemyGo.transform.localScale = clone.scale;

                // Add each enemy to every party member's list of selectable objects
                foreach (var partyMember in BattleManager._membersForThisBattle) 
                    partyMember.battlePanel.GetComponent<MenuController>().enemySelectable.Add(enemyGo);
                
                clone.SetupUnit(enemyGo);
                SetupProfileBox(clone);

                clone.Unit.parent = battleGeneratorDatabase.enemySpawnPoints[i+enemyOffset];

                var position = enemyGo.transform.position;
                var newPosition = new Vector3(position.x, position.y + 1.5f, position.z);

                var statusBox = Instantiate(battleGeneratorDatabase.enemyStatusBox, newPosition, battleGeneratorDatabase.enemyStatusBox.rotation);
                statusBox.transform.SetParent(clone.Unit.transform);
                
                var statusBoxController = statusBox.GetComponentInChildren<StatusEffectControllerUI>();
                statusBoxController.member = clone;
                statusBoxController.Initialize();
                
                BattleManager._enemiesForThisBattle.Add(clone);
                i++;
            }
        }

        #endregion
    }
}
