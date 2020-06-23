using Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class Setup : MonoBehaviour
    {
        public static Setup instance;

        private void Awake()
        {
            if (instance == null)
            {
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        /*public void ThisPartyMember(PartyMember member, BattleGeneratorDatabase database, int index, GameObject memberGO)
        {
            
        }

        private void SetupPartyMemberPanel(PartyMember member, BattleGeneratorDatabase database, int index)
        {
            database.characterPanels[index].gameObject.SetActive(true);

            var iconImage = database.characterPanels[index].transform.GetChild(0).GetComponent<Image>();
            iconImage.sprite = member.icon;

            var nameUgui = database.characterPanels[index].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            nameUgui.text = member.characterName;

            var slider = database.characterPanels[index].transform.GetChild(2).GetComponent<Slider>();

            var healthText = database.characterPanels[index].transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            healthText.text = "HP: " + member.health;

            var level = database.characterPanels[index].transform.GetChild(4).GetComponent<TextMeshProUGUI>();
            level.text = "LV\n" + member.level;
        }

        private void SetupThisPartyMemberBattleMenu(PartyMember member, BattleGeneratorDatabase database, int index)
        {
            var position = member.unit.transform.position;
            var newPosition = new Vector3(position.x, position.y + 1.5f, position.z + 3);
            var rotation = database.boPanel.battlePanel.transform.rotation;
            
            member.battlePanel = Instantiate
                (database.boPanel.battlePanel, newPosition, rotation);

            member.battlePanel.transform.position = newPosition;
            member.battlePanel.transform.SetParent(member.GetUnit().transform.parent, true);

            member.SetAbilityMenuOptions(database.boPanel);
            
            member.unit.battlePanelRef = member.battlePanel;
            member.unit.battlePanelIsSet = true;
            member.battlePanel.SetActive(false);
            
            member.unit.actionPointAnim =
                member.unit.battlePanelRef.transform.GetChild(1).GetComponent<Animator>();
        }
        public static void ThisEnemy()
        {
            
        }*/
    }
}