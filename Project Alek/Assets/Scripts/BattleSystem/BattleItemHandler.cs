using System;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;

namespace BattleSystem
{
    public class BattleItemHandler : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        private void Start()
        {
            MMEventManager.AddListener(this);
        }

        private void OnDisable()
        {
            MMEventManager.RemoveListener(this);
        }

        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (eventType.InventoryEventType != MMInventoryEventType.PickTarget) return;
            
            ChooseTarget._targetOptions = eventType.EventItem.targetOptions;
            ChooseTarget.GetItemCommand();
            BattleManager._usingItem = true;
            BattleManager._choosingOption = false;
        }
    }
}