using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class BattleResultsUI : MonoBehaviour
    {
        public List<GameObject> memberPanels = new List<GameObject>();
        public List<Image> progressBars = new List<Image>();
        public List<Image> classProgressBars = new List<Image>();
        public List<TextMeshProUGUI> expText = new List<TextMeshProUGUI>();
        public TextMeshProUGUI itemsObtainedText;
        public TextMeshProUGUI challengesCompletedText;
        public TextMeshProUGUI expBonusText;

        public void Init()
        {
            for (var i = 0; i < BattleEngine.Instance._membersForThisBattle.Count; i++)
            {
                var member = BattleEngine.Instance._membersForThisBattle[i];
                memberPanels[i].transform.Find("Icon").gameObject.GetComponent<Image>().sprite = member.icon;
                progressBars[i].fillAmount = (float) member.CurrentExperience / member.ExperienceToNextLevel;
                classProgressBars[i].fillAmount = (float) member.currentClass.CurrentExperience / member.currentClass.ExperienceToNextLevel;
                
                memberPanels[i].SetActive(true);
            }
            gameObject.SetActive(false);
        }

        private void Update()
        {
            for (var i = 0; i < BattleEngine.Instance._membersForThisBattle.Count; i++)
            {
                var member = BattleEngine.Instance._membersForThisBattle[i];

                expText[i].text = $"+ {member.BattleExpReceived}";
                progressBars[i].fillAmount = (float) member.CurrentExperience / member.ExperienceToNextLevel;
                classProgressBars[i].fillAmount = (float) member.currentClass.CurrentExperience / member.currentClass.ExperienceToNextLevel;
            }
        }
    }
}