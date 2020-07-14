using BattleSystem;
using Characters.Animations;
using UnityEngine;

namespace Characters.PartyMembers
{
    [CreateAssetMenu(fileName = "Battle Options Panel")]
    public class BattleOptionsPanel : ScriptableObject
    {
        public GameObject battlePanel;
        public PartyMember character;
        
        public void ShowBattlePanel()
        {
            BattleManager._choosingOption = true;
            character.actionPointAnim.SetInteger(AnimationHandler.APVal, character.CurrentAP);
            
            // This is triggered at the start of a party member's turn
            if (!character.battlePanel.activeSelf)
            {
                character.battlePanel.SetActive(true);
            }
            
            // This is triggered when going back to main menu from the ability menu
            else if (BattleManager._choosingAbility) 
            {
                character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.AbilityMenu);
                BattleManager._choosingAbility = false;
            }
            
            // This is triggered when going back while choosing a target from the ability menu
            else if (BattleManager._choosingTarget && BattleManager._choosingAbility) 
            {
                character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
                BattleManager._choosingTarget = false;
            }
            
            // This is triggered when going back while choosing a target from the main menu (attack button)
            else 
            {
                character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
                BattleManager._choosingTarget = false;
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
            
            var notEnoughAP = character.CurrentAP - commandCost < 0;
            if (notEnoughAP) return;
            
            ChooseTarget._targetOptions = commandTargetOptions;
            ChooseTarget.GetCurrentCommand(commandActionName, commandActionOption);
            character.Unit.actionCost = commandCost;
            
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            if (!BattleManager._choosingOption) BattleManager._choosingAbility = false;
            else BattleManager._choosingOption = false;
        }

        public void OnAbilityMenuButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.AbilityMenu);
            BattleManager._choosingAbility = true;
            BattleManager._choosingOption = false;
        }

        public void OnEndTurnButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            BattleManager._endThisMembersTurn = true;
            BattleManager._choosingOption = false;
        }
    }
}
