using UnityEngine;

namespace DamagePrefab
{
    public class DamagePrefab : MonoBehaviour
    {
        private void OnEnable() => Invoke(nameof(Hide), 2);
        
        private void Hide()
        {
            DamagePrefabManager.Instance.Dequeue(gameObject);
            gameObject.SetActive(false);
        }
    }
}