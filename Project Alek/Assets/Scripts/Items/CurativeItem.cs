﻿using System;
using System.Collections.Generic;
using Audio;
using BattleSystem;
using Characters;
using Characters.PartyMembers;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    [CreateAssetMenu(fileName = "Curative Item", menuName = "MoreMountains/InventoryEngine/CurativeItem", order = 4)]
    [Serializable]
    public class CurativeItem : InventoryItem
    {
        [SerializeField] private List<StatusEffect> ailmentsToCure = new List<StatusEffect>();

        public override bool Use()
        {
            base.Use();
            var target = Battle.Engine.activeUnit.CurrentTarget;
            target.CureAilments(ailmentsToCure);
            AudioController.PlayAudio(CommonAudioTypes.Heal);
            MainInventory.RemoveItem(this);
            return true;
        }
    }
}