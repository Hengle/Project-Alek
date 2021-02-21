using UnityEngine;

namespace DamagePrefab
{
    public class DamagePrefab : MonoBehaviour
    {
        private void OnEnable()
        {
            DamagePrefabManager.Instance.activeObjects.Enqueue(gameObject);
            Invoke(nameof(Hide), 2);
        }

        private void Hide()
        {
            DamagePrefabManager.Instance.activeObjects.Dequeue();
            gameObject.SetActive(false);
        }
    }
}