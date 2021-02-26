using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "Main Inventory", menuName = "Singleton SO/Main Inventory")]
    public class MainInventory : ScriptableObjectSingleton<MainInventory>
    {
        [SerializeField] private List<InventoryItem> inventoryItems = new List<InventoryItem>();

        // TODO: Make separate list(s) for items that have a set amount (like health and revival items)
        public static IEnumerable<InventoryItem> GetInventoryItems()
        {
            return Instance.inventoryItems;
        }

        public static void AddItem(InventoryItem item) => Instance.inventoryItems.Add(item);

        public static void RemoveItem(InventoryItem item) => Instance.inventoryItems.Remove(item);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}