using Characters;
using Characters.Animations;
using Characters.PartyMembers;
using MEC;
using SingletonScriptableObject;
using UnityEngine;

namespace BattleSystem
{
    [CreateAssetMenu(fileName = "Battle Options Panel")]
    public class BattleOptionsPanel : ScriptableObject
    {
        public PartyMember character;
        
        public void ShowBattlePanel()
        {
            // This is triggered at the start of a party member's turn
            if (!character.battlePanel.activeSelf)
            {
                Battle.Engine.choosingOption = true;
                character.battlePanel.SetActive(true);
            }
            
            // This is triggered when going back to main menu from the ability menu
            else if (Battle.Engine.choosingAbility) 
            {
                character.BattlePanelAnim.SetTrigger(AnimationHandler.AbilityMenu);
                Battle.Engine.choosingOption = true;
                Battle.Engine.choosingAbility = false;
            }
            
            // This is triggered when going back to main menu from the spell menu
            else if (Battle.Engine.choosingSpell)
            {
                character.BattlePanelAnim.SetTrigger(AnimationHandler.SpellMenu);
                Battle.Engine.choosingOption = true;
                Battle.Engine.choosingSpell = false;
            }
            
            // This is triggered when going back while choosing a target from the ability menu
            else if (Battle.Engine.choosingTarget && Battle.Engine.abilityMenuLast) 
            {
                character.BattlePanelAnim.SetTrigger(AnimationHandler.AbilityMenu);
                Battle.Engine.choosingTarget = false;
                Battle.Engine.choosingAbility = true;
                Battle.Engine.abilityMenuLast = false;
            }
            
            // This is triggered when going back while choosing a target from the spell menu
            else if (Battle.Engine.choosingTarget && Battle.Engine.spellMenuLast) 
            {
                character.BattlePanelAnim.SetTrigger(AnimationHandler.SpellMenu);
                Battle.Engine.choosingTarget = false;
                Battle.Engine.choosingSpell = true;
                Battle.Engine.spellMenuLast = false;
            }
            
            // This is triggered when going back while choosing a target from the main menu (attack button)
            else 
            {
                character.BattlePanelAnim.SetTrigger(AnimationHandler.Panel);
                Battle.Engine.choosingOption = true;
                Battle.Engine.choosingTarget = false;
            }
        }

        // Parameters are separated by comma in order of: Action Name, Action Option, Target Options, and Action Cost
        public void GetCommandInformation(string parameters, bool skipChooseTarget = false)
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

            if (skipChooseTarget)
            {
                character.Unit.commandActionName = commandActionName;
                character.Unit.commandActionOption = commandActionOption;
                Battle.Engine.skipChooseTarget = true;
            }
            else
            {
                ChooseTarget._targetOptions = commandTargetOptions;
                ChooseTarget.GetCurrentCommand(commandActionName, commandActionOption);
            }

            character.Unit.actionCost = commandCost;

            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            if (Battle.Engine.choosingAbility) Battle.Engine.choosingAbility = false;
            else if (Battle.Engine.choosingSpell) Battle.Engine.choosingSpell = false;
            else Battle.Engine.choosingOption = false;
        }

        public void OnAbilityMenuButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.AbilityMenu);
            Battle.Engine.choosingAbility = true;
            Battle.Engine.choosingOption = false;
        }

        public void OnSpellMenuButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.SpellMenu);
            Battle.Engine.choosingSpell = true;
            Battle.Engine.choosingOption = false;
        }

        public void OnEndTurnButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            Battle.Engine.endThisMembersTurn = true;
            Battle.Engine.choosingOption = false;
        }

        public void OnFleeButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            Timing.RunCoroutine(Battle.Engine.FleeBattleSequence());
        }
    }
}
