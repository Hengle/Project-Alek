﻿using System;
using System.Collections.Generic;
using Audio;
using BattleSystem;
using Characters.StatusEffects;
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
            var target = BattleEngine.Instance.activeUnit.CurrentTarget;
            target.CureAilments(ailmentsToCure);
            AudioController.Instance.PlayAudio(CommonAudioTypes.Instance.heal);
            return true;
        }
    }
}