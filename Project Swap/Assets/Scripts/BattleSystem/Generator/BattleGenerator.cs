using System.Linq;
using Characters.PartyMembers;
using MoreMountains.InventoryEngine;
using StatusEffects;
using UnityEngine;
using UnityEngine.UI;

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
            offset = PartyManager.instance.partyMembers.Count == 2 ? 1 : 0;
            enemyOffset = battleGeneratorDatabase.enemies.Count == 2 ? 1 : 0;
            
            SetupParty();
            SpawnEnemyTeam();
            SetupPartyMenuController();
            return false;
        }

        private void SetupParty()
        {
            for (var i = 0; i < PartyManager.instance.partyMembers.Count; i++)
                SpawnThisMember(PartyManager.instance.partyMembers[i], i);
        }

        private void SetupPartyMenuController()
        {
            foreach (var member in BattleManager.membersForThisBattle) {
                foreach (var partyMember in BattleManager.membersForThisBattle)
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

            mainMenu.Find("End Turn Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { character.battleOptionsPanel.OnEndTurnButton(); });
            
            character.SetAbilityMenuOptions();
            
            //SetupInventoryDisplay(character);
            
            character.Unit.battlePanelRef = character.battlePanel;
            character.battlePanel.SetActive(false);
            
            character.actionPointAnim = character.Unit.battlePanelRef.transform.Find("AP Box").GetComponent<Animator>();
        }

        private void SetupInventoryDisplay(PartyMember character)
        {
            var inventory = Instantiate(character.inventory);
            inventory.name = character.inventory.name;

            var inventoryDisplay = character.battlePanel.transform.Find
                ("Battle Menu").transform.Find("Item Inventory Display").GetComponent<InventoryDisplay>();
            
            inventoryDisplay.TargetInventoryName = character.inventory.name;
            inventoryDisplay.SetupInventoryDisplay();
        }

        #endregion

        #region Spawning

        private void SpawnThisMember(PartyMember character, int i)
        {
            var memberGo = Instantiate(character.characterPrefab, battleGeneratorDatabase.characterSpawnPoints[i+offset].transform);
            
            character.SetupUnit(memberGo);
            SetupBattlePanel(character);

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
            
            BattleManager.membersForThisBattle.Add(character);
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
                foreach (var partyMember in BattleManager.membersForThisBattle) 
                    partyMember.battlePanel.GetComponent<MenuController>().enemySelectable.Add(enemyGo);
                
                clone.SetupUnit(enemyGo);

                clone.Unit.parent = battleGeneratorDatabase.enemySpawnPoints[i+enemyOffset];

                var position = enemyGo.transform.position;
                var newPosition = new Vector3(position.x, position.y + 1.5f, position.z);

                var statusBox = Instantiate(battleGeneratorDatabase.enemyStatusBox, newPosition, battleGeneratorDatabase.enemyStatusBox.rotation);
                statusBox.transform.SetParent(clone.Unit.transform);
                
                var statusBoxController = statusBox.GetComponentInChildren<StatusEffectControllerUI>();
                statusBoxController.member = clone;
                statusBoxController.Initialize();
                
                BattleManager.enemiesForThisBattle.Add(clone);
                i++;
            }
        }

        #endregion
    }
}
