using System.Linq;
using Characters;
using Characters.PartyMembers;
using TMPro;
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
        //private Slider slider;
        //private TextMeshProUGUI healthText;
        private int offset;
        private int enemyOffset;
        
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
            var i = 0;
            foreach (PartyMember character in PartyManager.instance.partyMembers) {
                //SetupPartyMemberPanel(character, i);
                SpawnThisMember(character, i);
                i++;
            }
        }

        private void SetupPartyMenuController()
        {
            foreach (var member in BattleManager.membersForThisBattle) {
                foreach (var partyMember in BattleManager.membersForThisBattle)
                    member.battlePanel.GetComponent<MenuController>().memberSelectable.Add(partyMember.Unit.gameObject);
            }
        }

        // private void SetupPartyMemberPanel(PartyMember character, int i)
        // {
        //     battleGeneratorDatabase.characterPanels[i+offset].gameObject.SetActive(true);
        //
        //     var iconImage = battleGeneratorDatabase.characterPanels[i+offset].transform.GetChild(0).GetComponent<Image>();
        //     iconImage.sprite = character.icon;
        //
        //     var nameUgui = battleGeneratorDatabase.characterPanels[i+offset].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        //     nameUgui.text = character.characterName.ToUpper();
        //
        //     slider = battleGeneratorDatabase.characterPanels[i+offset].transform.GetChild(2).GetComponent<Slider>();
        //
        //     healthText = battleGeneratorDatabase.characterPanels[i+offset].transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        //     healthText.text = "HP: " + character.health;
        // }

        private void SetupBattlePanel(PartyMember character)
        {
            var position = character.Unit.transform.position;
            var newPosition = new Vector3(position.x, position.y + 1.5f, position.z + 3);
            var rotation = battleGeneratorDatabase.boPanel.battlePanel.transform.rotation;
            
            character.battlePanel = Instantiate
                (battleGeneratorDatabase.boPanel.battlePanel, newPosition, rotation);

            character.battlePanel.transform.position = newPosition;
            character.battlePanel.transform.SetParent(character.Unit.transform.parent, true);

            character.SetAbilityMenuOptions(battleGeneratorDatabase.boPanel);
            
            var mainMenu = character.battlePanel.transform.Find("Battle Menu").transform.Find("Main Options").transform;

            mainMenu.Find("Attack Button").gameObject.GetComponent<Button>().onClick.AddListener
                ( delegate { character.battleOptionsPanel.GetCommandInformation("UniversalAction,1,0,2"); });

            mainMenu.Find("Abilities Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { character.battleOptionsPanel.OnAbilityMenuButton(); });

            mainMenu.Find("End Turn Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { character.battleOptionsPanel.OnEndTurnButton(); });


            character.Unit.battlePanelRef = character.battlePanel;
            character.battlePanel.SetActive(false);
            
            character.actionPointAnim = character.Unit.battlePanelRef.transform.Find("AP Box").GetComponent<Animator>();
        }

        private void SpawnThisMember(PartyMember character, int i)
        {
            var memberGo = Instantiate(character.characterPrefab, battleGeneratorDatabase.characterSpawnPoints[i+offset].transform);

            //character.SetUnitGO(memberGo);
            character.SetupUnit(memberGo);
            SetupBattlePanel(character);

            memberGo.transform.localScale = character.scale;

            // Could add this functionality to panel controller
            var panel = battleGeneratorDatabase.characterPanels[i + offset];
            character.Unit.statusBox = panel.transform.GetChild(panel.transform.childCount - 1);

            panel.GetComponent<CharacterPanelController>().member = character;
            panel.SetActive(true);
            
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

                foreach (var partyMember in BattleManager.membersForThisBattle) 
                    partyMember.battlePanel.GetComponent<MenuController>().enemySelectable.Add(enemyGo);

                //clone.SetUnitGO(enemyGo);
                clone.SetupUnit(enemyGo);

                clone.Unit.parent = battleGeneratorDatabase.enemySpawnPoints[i+enemyOffset];

                var position = enemyGo.transform.position;
                var newPosition = new Vector3(position.x, position.y + 1.5f, position.z);
                
                clone.Unit.statusBox = Instantiate(battleGeneratorDatabase.statusBox, newPosition, battleGeneratorDatabase.statusBox.rotation);
                clone.Unit.statusBox.transform.SetParent(clone.Unit.transform);
                clone.Unit.statusBox.GetComponent<CanvasGroup>().alpha = 0;
                
                BattleManager.enemiesForThisBattle.Add(clone);
                i++;
            }
        }
    }
}
