using System;
using Audio;
using BattleSystem;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    [CreateAssetMenu(fileName = "RevivalItem", menuName = "MoreMountains/InventoryEngine/RevivalItem", order = 3)]
    [Serializable]
    public class RevivalItem : InventoryItem
    {
        [Range(0,1)] public float revivalHealthPercentage;

        [Range(0,6)] public int revivalAPAmount;
        
        public override bool Use()
        {
            base.Use();
            var target = BattleEngine.Instance.activeUnit.CurrentTarget;
            target.Revive(revivalHealthPercentage, revivalAPAmount);
            AudioController.PlayAudio(CommonAudioTypes.Revive);
            return true;
        }
    }
}