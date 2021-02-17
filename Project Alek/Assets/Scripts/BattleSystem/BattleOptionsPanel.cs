using Characters;
using Characters.Animations;
using Characters.PartyMembers;
using MEC;
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
                BattleEngine.Instance.choosingOption = true;
                character.battlePanel.SetActive(true);
            }
            
            // This is triggered when going back to main menu from the ability menu
            else if (BattleEngine.Instance.choosingAbility) 
            {
                character.BattlePanelAnim.SetTrigger(AnimationHandler.AbilityMenu);
                BattleEngine.Instance.choosingOption = true;
                BattleEngine.Instance.choosingAbility = false;
            }
            
            // This is triggered when going back to main menu from the spell menu
            else if (BattleEngine.Instance.choosingSpell)
            {
                character.BattlePanelAnim.SetTrigger(AnimationHandler.SpellMenu);
                BattleEngine.Instance.choosingOption = true;
                BattleEngine.Instance.choosingSpell = false;
            }
            
            // This is triggered when going back while choosing a target from the ability menu
            else if (BattleEngine.Instance.choosingTarget && BattleEngine.Instance.abilityMenuLast) 
            {
                character.BattlePanelAnim.SetTrigger(AnimationHandler.AbilityMenu);
                BattleEngine.Instance.choosingTarget = false;
                BattleEngine.Instance.choosingAbility = true;
                BattleEngine.Instance.abilityMenuLast = false;
            }
            
            // This is triggered when going back while choosing a target from the spell menu
            else if (BattleEngine.Instance.choosingTarget && BattleEngine.Instance.spellMenuLast) 
            {
                character.BattlePanelAnim.SetTrigger(AnimationHandler.SpellMenu);
                BattleEngine.Instance.choosingTarget = false;
                BattleEngine.Instance.choosingSpell = true;
                BattleEngine.Instance.spellMenuLast = false;
            }
            
            // This is triggered when going back while choosing a target from the main menu (attack button)
            else 
            {
                character.BattlePanelAnim.SetTrigger(AnimationHandler.Panel);
                BattleEngine.Instance.choosingOption = true;
                BattleEngine.Instance.choosingTarget = false;
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

            if (skipChooseTarget) BattleEngine.Instance.skipChooseTarget = true;
            else
            {
                ChooseTarget._targetOptions = commandTargetOptions;
                ChooseTarget.GetCurrentCommand(commandActionName, commandActionOption);
            }

            character.Unit.actionCost = commandCost;

            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            if (BattleEngine.Instance.choosingAbility) BattleEngine.Instance.choosingAbility = false;
            else if (BattleEngine.Instance.choosingSpell) BattleEngine.Instance.choosingSpell = false;
            else BattleEngine.Instance.choosingOption = false;
        }

        public void OnAbilityMenuButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.AbilityMenu);
            BattleEngine.Instance.choosingAbility = true;
            BattleEngine.Instance.choosingOption = false;
        }

        public void OnSpellMenuButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.SpellMenu);
            BattleEngine.Instance.choosingSpell = true;
            BattleEngine.Instance.choosingOption = false;
        }

        public void OnEndTurnButton()
        {
            character.battlePanel.GetComponent<Animator>().SetTrigger(AnimationHandler.Panel);
            BattleEngine.Instance.endThisMembersTurn = true;
            BattleEngine.Instance.choosingOption = false;
        }

        public void OnFleeButton()
        {
            Timing.RunCoroutine(BattleEngine.Instance.FleeBattleSequence());
        }
    }
}
