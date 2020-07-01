using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;

namespace BattleSystem
{
    public class BattleSystemInputManager : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        private void Awake() => MMEventManager.AddListener(this);

        public void OnMMEvent(MMInventoryEvent eventType)
        {
            return;
            switch (eventType.InventoryEventType)
            {
                case MMInventoryEventType.InventoryOpens:
                    BattleManager.inputModule.enabled = true;
                    break;
                case MMInventoryEventType.InventoryCloses:
                    BattleManager.inputModule.enabled = true;
                    break;
            }
        }
    }
}