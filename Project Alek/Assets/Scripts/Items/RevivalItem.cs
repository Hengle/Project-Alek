using System;
using Characters;
using UnityEngine;
using UnityEngine.EventSystems;

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
            var target = EventSystem.current.currentSelectedGameObject.GetComponent<Unit>().parent;
            target.Revive(revivalHealthPercentage, revivalAPAmount);
            return true;
        }
    }
}