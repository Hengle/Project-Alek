using Animations;
using BattleSystem;
using UnityEngine;

namespace Characters.PartyMembers
{
    /*
     * Consider making this inherit from a base SO so each character's battle panel can already have
     * all of their available actions and not rely on generating them at start of battle
     */
    [CreateAssetMenu(fileName = "Battle Options Panel")]
    public class BattleOptionsPanel : ScriptableObject
    {
        public GameObject battlePanel;
        private PartyMember character; // This will be used to store a reference to the active party member
        
        public void ShowBattlePanel(PartyMember thisCharacter)
        {
            character = thisCharacter;
            BattleHandler.choosingOption = true;
            thisCharacter.unit.actionPointAnim.SetInteger(AnimationHandler.APVal, thisCharacter.unit.currentAP);
            
            // This is triggered at the start of a party member's turn
            if (!thisCharacter.battlePanel.activeSelf) { thisCharacter.battlePanel.SetActive(true); }
            
            // This is triggered when going back to main menu from the ability menu
            else if (BattleHandler.choosingAbility) {
                character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.AbilityMenu);
                BattleHandler.choosingAbility = false;
            }
            
            // This is triggered when going back while choosing a target from the ability menu
            else if (BattleHandler.choosingTarget && BattleHandler.choosingAbility) {
                BattleHandler.choosingTarget = false;
                character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            }
            
            // This is triggered when going back while choosing a target from the main menu (attack button)
            else
            {
                BattleHandler.choosingTarget = false;
                character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            }
        }

        // Parameters are separated by comma in order of: Action Name, Action Option, Target Options, and Action Cost
        // Could add extra parameter that serves as the ID for each ability (that comes from a dictionary)
        public void GetCommandInformation(string parameters)
        {
            // Split the parameters into a list
            string[] splitParams = parameters.Split(',');
        
            // Store each parameter into separate variables
            var commandActionName = splitParams[0];
            var commandActionOptionString = splitParams[1];
            var commandTargetOptionsString = splitParams[2];
            var commandCostString = splitParams[3];
        
            // Convert numbers to integers
            var commandActionOption = int.Parse(commandActionOptionString);
            var commandTargetOptions = int.Parse(commandTargetOptionsString);
            var commandCost = int.Parse(commandCostString);

            // Check to see if the action costs more than current AP
            var notEnoughAP = character.unit.currentAP - commandCost < 0;
            if (notEnoughAP) return;
        
            // Store the information
            ChooseTarget.GetCurrentCommand(commandActionName, commandActionOption, false);
            ChooseTarget.targetOptions = commandTargetOptions;
            character.unit.actionCost = commandCost;

            // Close the panel
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            if (!BattleHandler.choosingOption) BattleHandler.choosingAbility = false;
            else BattleHandler.choosingOption = false;
        }

        public void OnAbilityMenuButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.AbilityMenu);
            BattleHandler.choosingAbility = true;
            BattleHandler.choosingOption = false;
        }
        
        public void OnSwapButton()
        {
            // First 2 parameters don't matter
            ChooseTarget.GetCurrentCommand("UniversalAction", 2, true);
            ChooseTarget.targetOptions = 1;
            character.unit.actionCost = 1;
            
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            if (!BattleHandler.choosingOption) BattleHandler.choosingAbility = false;
            BattleHandler.choosingOption = false;
        }

        public void OnEndTurnButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            BattleHandler.endThisMembersTurn = true;
            BattleHandler.choosingOption = false;
        }
    }
}
