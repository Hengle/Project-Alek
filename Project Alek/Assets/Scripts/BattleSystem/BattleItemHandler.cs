﻿using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;

namespace BattleSystem
{
    public class BattleItemHandler : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        private void Start() => MMEventManager.AddListener(this);
        
        private void OnDisable() => MMEventManager.RemoveListener(this);
        
        public void OnMMEvent(MMInventoryEvent eventType)
        {
            switch (eventType.InventoryEventType)
            {
                case MMInventoryEventType.PickTarget:
                    ChooseTarget._targetOptions = eventType.EventItem.targetOptions;
                    ChooseTarget.GetItemCommand();

                    BattleEngine.Instance.usingItem = true;
                    BattleEngine.Instance.choosingOption = false;
                    break;
                case MMInventoryEventType.Select:
                    ChooseTarget._currentlySelectedItem = eventType.EventItem;
                    break;
            }
        }
    }
}