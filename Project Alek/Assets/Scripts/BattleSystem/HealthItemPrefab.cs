using UnityEngine;

namespace BattleSystem
{
    public class HealthItemPrefab : MonoBehaviour
    {
        private void OnEnable() => Invoke(nameof(Hide), 1.5f);
        private void Hide() => gameObject.SetActive(false);
    }
}
