﻿using System;
using UnityEngine;
using Characters;
using Characters.PartyMembers;
using Type = Characters.Type;

namespace BattleSystem
{
    public class ChooseTarget : MonoBehaviour
    {
        public static int targetOptions = 0; // 1 = party member, 0 = enemy, 2 = all
        public static Type targetOptionsType;
        private static int classOption;
        private static string className;
        private static bool isSwapOption;
        public static bool isMultiTarget;
        private static MenuController menuController;
        private static Unit memberCurrentlyChoosingTarget;
        private static PartyMember character;
        private Unit thisUnit;

        private void Start() => thisUnit = GetComponent<Unit>();

        private void Update()
        {
            if (!BattleHandler.choosingTarget || !isMultiTarget || thisUnit.id != targetOptionsType) return;
            
            thisUnit.outline.enabled = true;
            thisUnit.button.interactable = false;

            if (BattleHandler.inputModule.cancel.action.triggered) {
                //thisUnit.outline.enabled = false;
                thisUnit.button.interactable = true;
                isMultiTarget = false;
                return;
            }
            
            if (!BattleHandler.inputModule.submit.action.triggered) return;
                
            AddMultiHitCommand();
            thisUnit.outline.enabled = false;
            thisUnit.button.interactable = true;
            isMultiTarget = false;
        }

        public static void ForThisMember(PartyMember member)
        {
            BattleHandler.choosingTarget = true;
            character = member;
            memberCurrentlyChoosingTarget = member.unit;
            
            menuController = character.battlePanel.GetComponent<MenuController>();
            menuController.SetTargetFirstSelected();
        }

        public static void GetCurrentCommand(string name, int option, bool isSwap)
        {
            className = name;
            classOption = option;
            isSwapOption = isSwap;
            
            switch (targetOptions)
            {
                case 0: targetOptionsType = Type.Enemy;
                    break;
                case 1: targetOptionsType = Type.PartyMember;
                    break;
            }
        }
        
        private void OnMouseOver()
        {
            if (!BattleHandler.choosingTarget) return;
            
            switch (targetOptions)
            {
                case 0 when thisUnit.id == Type.Enemy && thisUnit.status != Status.Dead:
                    thisUnit.outline.enabled = true;
                    if (Input.GetMouseButtonUp(0)) AddCommand();
                    break;
                
                case 1 when thisUnit.id == Type.PartyMember && thisUnit != memberCurrentlyChoosingTarget:
                    thisUnit.outline.enabled = true;
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (isSwapOption) AddSwapCommand();
                        else AddCommand();
                    }
                    break;
                
                case 2 when thisUnit != memberCurrentlyChoosingTarget && thisUnit.status != Status.Dead:
                    thisUnit.outline.enabled = true;
                    if (Input.GetMouseButtonUp(0)) AddCommand();
                    break;
            }
        }

        public void AddCommand()
        {
            if (isSwapOption && memberCurrentlyChoosingTarget != thisUnit) { AddSwapCommand(); return; }
            if (isSwapOption && memberCurrentlyChoosingTarget == thisUnit) return;
            if (thisUnit.status == Status.Dead) return;
            
            memberCurrentlyChoosingTarget.currentTarget = thisUnit;
            memberCurrentlyChoosingTarget.commandActionName = className;
            memberCurrentlyChoosingTarget.commandActionOption = classOption;
            BattleHandler.choosingTarget = false;
        }

        public void AddMultiHitCommand() {
            memberCurrentlyChoosingTarget.commandActionName = className;
            memberCurrentlyChoosingTarget.commandActionOption = classOption;
            BattleHandler.choosingTarget = false;
        }

        private void AddSwapCommand()
        {
            if (thisUnit.status == Status.Dead) return;
            isSwapOption = false;
            memberCurrentlyChoosingTarget.isSwapping = true;

            menuController.swapButton.interactable = false;
            
            BattleHandler.partyHasChosenSwap = true;
            BattleHandler.shouldGiveCommand = false;
            BattleHandler.partyMemberWhoChoseSwap = character;
            BattleHandler.partySwapTarget = thisUnit;
            BattleHandler.choosingTarget = false;
        }

        private void OnMouseExit() => thisUnit.outline.enabled = false;
    }
}