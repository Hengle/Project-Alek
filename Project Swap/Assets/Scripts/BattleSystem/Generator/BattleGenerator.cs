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
        private Slider slider;
        private TextMeshProUGUI healthText;
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
                SetupPartyMemberPanel(character, i);
                SpawnThisMember(character, i);
                i++;
            }
        }

        private void SetupPartyMenuController()
        {
            foreach (var member in BattleHandler.membersForThisBattle) {
                foreach (var partyMember in BattleHandler.membersForThisBattle)
                    member.battlePanel.GetComponent<MenuController>().memberSelectable.Add(partyMember.unit.gameObject);
            }
        }

        private void SetupPartyMemberPanel(PartyMember character, int i)
        {
            battleGeneratorDatabase.characterPanels[i+offset].gameObject.SetActive(true);

            var iconImage = battleGeneratorDatabase.characterPanels[i+offset].transform.GetChild(0).GetComponent<Image>();
            iconImage.sprite = character.icon;

            var nameUgui = battleGeneratorDatabase.characterPanels[i+offset].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            nameUgui.text = character.characterName.ToUpper();

            slider = battleGeneratorDatabase.characterPanels[i+offset].transform.GetChild(2).GetComponent<Slider>();

            healthText = battleGeneratorDatabase.characterPanels[i+offset].transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            healthText.text = "HP: " + character.health;
        }

        private void SetupBattlePanel(PartyMember character)
        {
            var position = character.unit.transform.position;
            var newPosition = new Vector3(position.x, position.y + 1.5f, position.z + 3);
            var rotation = battleGeneratorDatabase.boPanel.battlePanel.transform.rotation;
            
            character.battlePanel = Instantiate
                (battleGeneratorDatabase.boPanel.battlePanel, newPosition, rotation);

            character.battlePanel.transform.position = newPosition;
            character.battlePanel.transform.SetParent(character.unit.transform.parent, true);

            character.SetAbilityMenuOptions(battleGeneratorDatabase.boPanel);
            
            character.unit.battlePanelRef = character.battlePanel;
            character.unit.battlePanelIsSet = true;
            character.battlePanel.SetActive(false);
            
            character.unit.actionPointAnim =
                character.unit.battlePanelRef.transform.Find("AP Box").GetComponent<Animator>();
        }

        private void SpawnThisMember(PartyMember character, int i)
        {
            var memberGo = Instantiate(character.characterPrefab, battleGeneratorDatabase.characterSpawnPoints[i+offset].transform);

            character.SetUnitGO(memberGo, slider, healthText);
            SetupBattlePanel(character);
            character.SetupUnit(character);

            memberGo.transform.localScale = character.scale;

            character.unit.characterPanelRef = battleGeneratorDatabase.characterPanels[i+offset].transform;
            character.unit.statusBox =
                character.unit.characterPanelRef.GetChild(character.unit.characterPanelRef.childCount - 1);
            
            character.unit.spriteParentObject = battleGeneratorDatabase.characterSpawnPoints[i+offset];

            character.SetCameras();
            
            BattleHandler.membersForThisBattle.Add(character);
        }

        private void SpawnEnemyTeam()
        {
            // Spawning normal encounters needs to draw from a random list of enemies based on the overworld enemy that initiates battle

            var i = 0;
            foreach (Enemy enemy in battleGeneratorDatabase.enemies)
            {
                var enemyGo = Instantiate(enemy.characterPrefab, battleGeneratorDatabase.enemySpawnPoints[i+enemyOffset].transform);
                enemyGo.name = enemy.name;
                enemyGo.transform.localScale = enemy.scale;

                foreach (var partyMember in BattleHandler.membersForThisBattle) 
                    partyMember.battlePanel.GetComponent<MenuController>().enemySelectable.Add(enemyGo);

                enemy.SetUnitGO(enemyGo);
                enemy.SetupUnit(enemy);

                enemy.unit.spriteParentObject = battleGeneratorDatabase.enemySpawnPoints[i+enemyOffset];

                var position = enemyGo.transform.position;
                var newPosition = new Vector3(position.x, position.y + 1.5f, position.z);
                
                enemy.unit.statusBox = Instantiate(battleGeneratorDatabase.statusBox, newPosition, battleGeneratorDatabase.statusBox.rotation);
                enemy.unit.statusBox.transform.SetParent(enemy.unit.transform);
                enemy.unit.statusBox.GetComponent<CanvasGroup>().alpha = 0;


                BattleHandler.enemiesForThisBattle.Add(enemy);
                i++;
            }
        }
    }
}
