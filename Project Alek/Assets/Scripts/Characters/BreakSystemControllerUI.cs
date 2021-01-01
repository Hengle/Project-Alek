using System;
using TMPro;
using UnityEngine;

namespace Characters
{
    public class BreakSystemControllerUI : MonoBehaviour
    {
        public Enemy enemy;
        [SerializeField] private GameObject shieldPrefab;
        [SerializeField] private TextMeshPro shieldCount;
        [SerializeField] private TextMeshPro enemyName;

        public void Initialize()
        {
            shieldCount.text = enemy.maxShieldCount.ToString();
            enemyName.text = enemy.characterName;
            enemyName.gameObject.SetActive(false);
            
            enemy.onShieldValueChanged += UpdateShieldCountUI;
            enemy.Unit.onSelect += ShowName;
            enemy.Unit.onDeselect += HideName;
            enemy.onDeath += OnDeath;
        }

        private void UpdateShieldCountUI(int count)
        {
            shieldCount.text = count.ToString();
        }

        private void ShowName()
        {
            enemyName.gameObject.SetActive(true);
        }

        private void HideName()
        {
            enemyName.gameObject.SetActive(false);
        }

        private void OnDeath(UnitBase enemy) => gameObject.SetActive(false);

        private void OnDisable()
        {
            enemy.onShieldValueChanged -= UpdateShieldCountUI;
            enemy.Unit.onSelect -= ShowName;
            enemy.Unit.onDeselect -= HideName;
            enemy.onDeath -= OnDeath;
        }
    }
}
