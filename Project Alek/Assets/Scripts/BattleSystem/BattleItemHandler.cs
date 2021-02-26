using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using SingletonScriptableObject;
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
                    
                    Battle.Engine.usingItem = true;
                    Battle.Engine.choosingOption = false;
                    break;
                case MMInventoryEventType.Select:
                    ChooseTarget._currentlySelectedItem = eventType.EventItem;
                    break;
            }
        }
    }
}