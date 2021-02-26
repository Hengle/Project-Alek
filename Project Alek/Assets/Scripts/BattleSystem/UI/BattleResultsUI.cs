using System.Collections.Generic;
using Audio;
using Characters.PartyMembers;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class BattleResultsUI : MonoBehaviour
    {
        public List<GameObject> otherCanvases = new List<GameObject>();
        public List<GameObject> memberPanels = new List<GameObject>();
        public List<Image> progressBars = new List<Image>();
        public List<Image> classProgressBars = new List<Image>();
        public List<TextMeshProUGUI> expText = new List<TextMeshProUGUI>();
        public Queue<PartyMember> membersThatLeveledUp = new Queue<PartyMember>();
        public GameObject resultsPanel;
        public GameObject levelUpPanel;
        public TextMeshProUGUI levelUpText;
        public TextMeshProUGUI itemsObtainedText;
        public TextMeshProUGUI challengesCompletedText;
        public TextMeshProUGUI expBonusText;

        public bool showingLevelUps;

        public void Init()
        {
            for (var i = 0; i < OldBattleEngine.Instance._membersForThisBattle.Count; i++)
            {
                var member = OldBattleEngine.Instance._membersForThisBattle[i];
                memberPanels[i].transform.Find("Icon").gameObject.GetComponent<Image>().sprite = member.icon;
                progressBars[i].fillAmount = (float) member.CurrentExperience / member.ExperienceToNextLevel;
                classProgressBars[i].fillAmount = (float) member.currentClass.CurrentExperience / member.currentClass.ExperienceToNextLevel;
                
                memberPanels[i].SetActive(true);
            }
            gameObject.SetActive(false);
        }

        public void Enqueue(object member)
        {
            if (!membersThatLeveledUp.Contains((PartyMember) member))
                membersThatLeveledUp.Enqueue((PartyMember) member);
        }
        
        public void DisableUI() => otherCanvases.ForEach(c => c.SetActive(false));

        public IEnumerator<float> ShowLevelUps()
        {
            showingLevelUps = true;
            resultsPanel.SetActive(false);

            while (membersThatLeveledUp.Count > 0)
            {
                var member = membersThatLeveledUp.Dequeue();
                SuperCloseupCamController._onLevelUpCloseUp?.Invoke(member);

                var hpText = $"HP: {member.health.BaseValue} (+{member.health.amountIncreasedBy})\n";

                var strText = member.currentClass.statsToIncrease.Contains(member.strength)
                    ? $"STR: {member.strength.BaseValue} (+{member.strength.amountIncreasedBy})\n"
                    : $"STR: {member.strength.BaseValue}\n";
                
                var magText = member.currentClass.statsToIncrease.Contains(member.magic)
                    ? $"MAG: {member.magic.BaseValue} (+{member.magic.amountIncreasedBy})\n"
                    : $"MAG: {member.magic.BaseValue}\n";
                
                var accTest = member.currentClass.statsToIncrease.Contains(member.accuracy)
                    ? $"ACC: {member.accuracy.BaseValue} (+{member.accuracy.amountIncreasedBy})\n"
                    : $"ACC: {member.accuracy.BaseValue}\n";
                
                var initText = member.currentClass.statsToIncrease.Contains(member.initiative)
                    ? $"INIT: {member.initiative.BaseValue} (+{member.initiative.amountIncreasedBy})\n"
                    : $"INIT: {member.initiative.BaseValue}\n";
                
                var defText = member.currentClass.statsToIncrease.Contains(member.defense)
                    ? $"DEF: {member.defense.BaseValue} (+{member.defense.amountIncreasedBy})\n"
                    : $"DEF: {member.defense.BaseValue}\n";
                
                var resText = member.currentClass.statsToIncrease.Contains(member.resistance)
                    ? $"RES: {member.resistance.BaseValue} (+{member.resistance.amountIncreasedBy})\n"
                    : $"RES: {member.resistance.BaseValue}\n";
                
                var critText = member.currentClass.statsToIncrease.Contains(member.criticalChance)
                    ? $"CRIT: {member.criticalChance.BaseValue} (+{member.criticalChance.amountIncreasedBy})\n"
                    : $"CRIT: {member.criticalChance.BaseValue}\n";
                    
                levelUpText.text =
                    $"{member.characterName}\n" + "\n" +
                    $"Level: {member.level}\n" +
                    $"{hpText}" +
                    $"{strText}" +
                    $"{magText}" +
                    $"{accTest}" +
                    $"{initText}" +
                    $"{defText}" +
                    $"{resText}" +
                    $"{critText}";

                levelUpPanel.SetActive(true);
                
                AudioController.PlayAudio(CommonAudioTypes.LevelUp);
                
                yield return Timing.WaitForSeconds(0.5f);
                yield return Timing.WaitUntilTrue(() => BattleInput._controls.Battle.Confirm.triggered);
                
                levelUpPanel.SetActive(false);
                SuperCloseupCamController._disableCam?.Invoke(member);
            }

            showingLevelUps = false;
        }

        private void Update()
        {
            if (showingLevelUps) return;
            for (var i = 0; i < OldBattleEngine.Instance._membersForThisBattle.Count; i++)
            {
                var member = OldBattleEngine.Instance._membersForThisBattle[i];

                expText[i].text = $"+ {member.BattleExpReceived}";
                progressBars[i].fillAmount = (float) member.CurrentExperience / member.ExperienceToNextLevel;
                classProgressBars[i].fillAmount = (float) member.currentClass.CurrentExperience / member.currentClass.ExperienceToNextLevel;
            }
        }
    }
}