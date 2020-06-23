using UnityEngine;
using UnityEngine.UI;
using Characters;
using TMPro;

/*
 * Note that this could be changed later to inherit from a base Script Object that has the functions to call.
 * This way I can make new script objects for different battle generators based on the area you are in or boss battles
 */
namespace BattleSystem
{
    public class BattleGenerator : MonoBehaviour
    {
        [SerializeField] private BattleGeneratorDatabase battleGeneratorDatabase;
        private Slider slider;
        private TextMeshProUGUI healthText;
        
        public bool SetupBattle()
        {
            SetupParty();
            SpawnEnemyTeam();
            SetupPartyMenuController();
            return false;
        }

        private void SetupPartyMenuController()
        {
            foreach (var member in BattleHandler.membersForThisBattle)
            {
                foreach (var partyMember in BattleHandler.membersForThisBattle) 
                    member.battlePanel.GetComponent<MenuController>().memberSelectable.Add(partyMember.unit.gameObject);
            }
        }

        private void SetupParty()
        {
            var i = 0;
            foreach (PartyMember character in PartyManager.instance.partyMembers)
            {
                SetupPartyMemberPanel(character, i);
                SpawnThisMember(character, i);
                i++;
            }
        }

        private void SetupPartyMemberPanel(PartyMember character, int i)
        {
            battleGeneratorDatabase.characterPanels[i].gameObject.SetActive(true);

            var iconImage = battleGeneratorDatabase.characterPanels[i].transform.GetChild(0).GetComponent<Image>();
            iconImage.sprite = character.icon;

            var nameUgui = battleGeneratorDatabase.characterPanels[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            nameUgui.text = character.characterName.ToUpper();

            slider = battleGeneratorDatabase.characterPanels[i].transform.GetChild(2).GetComponent<Slider>();

            healthText = battleGeneratorDatabase.characterPanels[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            healthText.text = "HP: " + character.health;
        }

        private void SetupBattlePanel(PartyMember character, int i)
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
                character.unit.battlePanelRef.transform.GetChild(1).GetComponent<Animator>();
        }

        private void SpawnThisMember(PartyMember character, int i)
        {
            var memberGo = Instantiate(character.characterPrefab, battleGeneratorDatabase.characterSpawnPoints[i].transform);

            character.SetUnitGO(memberGo, slider, healthText, battleGeneratorDatabase.showDamageSO);
            SetupBattlePanel(character, i);
            character.SetupUnit(character);

            InstantiateUnitPrefabs(character.unit);
            
            memberGo.transform.localScale = character.scale;

            character.unit.characterPanelRef = battleGeneratorDatabase.characterPanels[i].transform;
            character.unit.spriteParentObject = battleGeneratorDatabase.characterSpawnPoints[i];
            character.unit.statusEffectGO = battleGeneratorDatabase.characterPanels[i].transform.GetChild(4).gameObject;
            
            character.SetCameras();
            
            BattleHandler.membersForThisBattle.Add(character);
        }

        private void SpawnEnemyTeam()
        {
            // Spawning normal encounters needs to draw from a random list of enemies based on the overworld enemy that initiates battle

            var i = 0;
            foreach (Enemy enemy in battleGeneratorDatabase.enemies)
            {
                var enemyGo = Instantiate(enemy.characterPrefab, battleGeneratorDatabase.enemySpawnPoints[i].transform);
                enemyGo.name = enemy.name;
                enemyGo.transform.localScale = enemy.scale;

                foreach (var partyMember in BattleHandler.membersForThisBattle) 
                    partyMember.battlePanel.GetComponent<MenuController>().enemySelectable.Add(enemyGo);

                enemy.SetUnitGO(enemyGo);
                enemy.SetupUnit(enemy);
                
                InstantiateUnitPrefabs(enemy.unit);

                enemy.unit.spriteParentObject = battleGeneratorDatabase.enemySpawnPoints[i];

                BattleHandler.enemiesForThisBattle.Add(enemy);
                i++;
            }
        }

        private void InstantiateUnitPrefabs(Unit characterUnit)
        {
            var position = characterUnit.gameObject.transform.position;
            var newPosition = new Vector3(position.x, position.y + 2, position.z);
            var parent = characterUnit.transform.parent;

            characterUnit.nameText = Instantiate
                (characterUnit.showDamageSO.nameTextWithDmgColor, newPosition, characterUnit.nameText.transform.rotation);
            
            characterUnit.nameText.transform.SetParent(parent);
            characterUnit.nameText.text = characterUnit.unitName.ToUpper();

            characterUnit.damagePrefab = Instantiate
                (characterUnit.showDamageSO.damagePrefab, newPosition, characterUnit.showDamageSO.damagePrefab.transform.rotation);
            
            characterUnit.damagePrefab.transform.SetParent(parent, true);
            characterUnit.damageText = characterUnit.damagePrefab.GetComponentInChildren<TextMeshPro>();
            characterUnit.damagePrefab.SetActive(false);
            
            characterUnit.damagePrefab2 = Instantiate
                (characterUnit.showDamageSO.damagePrefab, newPosition, characterUnit.showDamageSO.damagePrefab.transform.rotation);
            
            characterUnit.damagePrefab2.transform.SetParent(parent, true);
            characterUnit.damageText2 = characterUnit.damagePrefab2.GetComponentInChildren<TextMeshPro>();
            characterUnit.damagePrefab2.SetActive(false);
            
            characterUnit.critDamagePrefab = Instantiate
                (characterUnit.showDamageSO.critDamagePrefab, newPosition, characterUnit.showDamageSO.critDamagePrefab.transform.rotation);
            
            characterUnit.critDamagePrefab.transform.SetParent(parent, true);
            characterUnit.critDamageText = characterUnit.critDamagePrefab.GetComponentInChildren<TextMeshPro>();
            characterUnit.critDamagePrefab.SetActive(false);
            
            characterUnit.critDamagePrefab2 = Instantiate
                (characterUnit.showDamageSO.critDamagePrefab, newPosition, characterUnit.showDamageSO.critDamagePrefab.transform.rotation);
            
            characterUnit.critDamagePrefab2.transform.SetParent(parent, true);
            characterUnit.critDamageText2 = characterUnit.critDamagePrefab2.GetComponentInChildren<TextMeshPro>();
            characterUnit.critDamagePrefab2.SetActive(false);
        }
    }
}
