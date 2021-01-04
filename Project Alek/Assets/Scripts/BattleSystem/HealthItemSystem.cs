using Characters;
using MoreMountains.InventoryEngine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BattleSystem
{
    public class HealthItemSystem : MonoBehaviour
    {
        private Unit unit;
        private Inventory inventory;

        void Start()
        {
            inventory = GameObject.Find("Main Inventory").GetComponent<Inventory>();
            unit = GetComponent<Unit>();

            unit.onTimedAttack += GetHealthItemChance;
        }
        
        private void GetHealthItemChance(bool success)
        {
            if (!success) return;
            if (unit.HasMissedAllTargets) return;

            var randomValue = Random.value;
            if (randomValue < 0.40f) AddItems();
        }
        
        private void AddItems()
        {
            var count = Random.Range(1, 3);
            
            Logger.Log($"Adding {count} item(s) to inventory!");
            
            for (var i = 0; i < count; i++)
            {
                var id = Random.Range(0, HealthItemPrefabManager.Instance.healthItems.Count - 1);
                inventory.AddItem(HealthItemPrefabManager.Instance.healthItems[id], 1);

                if (!unit.isAbility || !unit.currentAbility.isMultiTarget)
                {
                    var position = unit.currentTarget.Unit.gameObject.transform.position;
                    var newPosition = new Vector3(position.x + count, position.y, position.z);
                        
                    var item = HealthItemPrefabManager.Instance.ShowItem(id);
                    item.transform.position = newPosition;
                }
                else
                {
                    var randomTarget = Random.Range(0, unit.multiHitTargets.Count - 1);
                        
                    var position = unit.multiHitTargets[randomTarget].Unit.gameObject.transform.position;
                    var newPosition = new Vector3(position.x, position.y, position.z);
                        
                    var item = HealthItemPrefabManager.Instance.ShowItem(id);
                    item.transform.position = newPosition;
                }
            }
        }

        private void OnDisable() => unit.onTimedAttack -= GetHealthItemChance;
    }
}
