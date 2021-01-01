using UnityEngine;

namespace BattleSystem
{
    public class HealthItemPrefab : MonoBehaviour
    {
        private void OnEnable() => Invoke(nameof(Hide), 2);
        private void Hide() => gameObject.SetActive(false);
    }
}
