using Characters;
using Characters.Animations;
using Characters.PartyMembers;
using UnityEngine;

namespace BattleSystem
{
    [CreateAssetMenu(fileName = "Battle Options Panel")]
    public class BattleOptionsPanel : ScriptableObject
    {
        public GameObject battlePanel;
        public PartyMember character;
        
        public void ShowBattlePanel()
        {
            BattleManager.Instance.choosingOption = true;

            // This is triggered at the start of a party member's turn
            if (!character.battlePanel.activeSelf)
            {
                character.battlePanel.SetActive(true);
            }
            
            // This is triggered when going back to main menu from the ability menu
            else if (BattleManager.Instance.choosingAbility) 
            {
                character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.AbilityMenu);
                BattleManager.Instance.choosingAbility = false;
            }
            
            // This is triggered when going back while choosing a target from the ability menu
            else if (BattleManager.Instance.choosingTarget && BattleManager.Instance.choosingAbility) 
            {
                character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
                BattleManager.Instance.choosingTarget = false;
            }
            
            // This is triggered when going back while choosing a target from the main menu (attack button)
            else 
            {
                character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
                BattleManager.Instance.choosingTarget = false;
            }
        }

        // Parameters are separated by comma in order of: Action Name, Action Option, Target Options, and Action Cost
        public void GetCommandInformation(string parameters)
        {
            var splitParams = parameters.Split(',');
            
            var commandActionName = splitParams[0];
            var commandActionOptionString = splitParams[1];
            var commandTargetOptionsString = splitParams[2];
            var commandCostString = splitParams[3];
            
            var commandActionOption = int.Parse(commandActionOptionString);
            var commandTargetOptions = int.Parse(commandTargetOptionsString);
            var commandCost = int.Parse(commandCostString);
            commandCost += character.Unit.conversionLevel;
            
            var notEnoughAP = character.CurrentAP - commandCost < 0;
            if (notEnoughAP)
            {
                var amountToBorrow = commandCost - character.CurrentAP;
                if (Unit.CanBorrow(amountToBorrow)) character.Unit.borrowAP(amountToBorrow);
                else return;
            }
            
            ChooseTarget._targetOptions = commandTargetOptions;
            ChooseTarget.GetCurrentCommand(commandActionName, commandActionOption);
            character.Unit.actionCost = commandCost;
            
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            if (!BattleManager.Instance.choosingOption) BattleManager.Instance.choosingAbility = false;
            else BattleManager.Instance.choosingOption = false;
        }

        public void OnAbilityMenuButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.AbilityMenu);
            BattleManager.Instance.choosingAbility = true;
            BattleManager.Instance.choosingOption = false;
        }

        public void OnEndTurnButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            BattleManager.Instance.endThisMembersTurn = true;
            BattleManager.Instance.choosingOption = false;
        }
    }
}
