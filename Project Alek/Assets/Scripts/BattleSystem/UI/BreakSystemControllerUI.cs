using System.Collections.Generic;
using Characters;
using Characters.Animations;
using Characters.Enemies;
using MEC;
using TMPro;
using UnityEngine;

namespace BattleSystem.UI
{
    public class BreakSystemControllerUI : MonoBehaviour
    {
        public Enemy enemy;
        [SerializeField] private GameObject shield;
        [SerializeField] private TextMeshPro shieldCount;
        [SerializeField] private TextMeshPro enemyName;

        private Animator shieldAnim;

        private void Awake()
        {
            shieldAnim = shield.GetComponent<Animator>();
        }

        public void Initialize()
        {
            shieldCount.text = enemy.maxShieldCount.ToString();
            enemyName.text = enemy.characterName;
            enemyName.gameObject.SetActive(false);
            shieldCount.gameObject.SetActive(false);
            shield.SetActive(false);
            Timing.RunCoroutine(ShowShield());
            
            enemy.onShieldValueChanged += UpdateShieldCountUI;
            enemy.onShieldValueChanged += PlayShieldDamagedAnimation;
            enemy.onShieldBroken += PlayShieldBrokenAnimation;
            enemy.onShieldBroken += HideShieldCount;
            enemy.onShieldRestored += ShowShieldCount;
            enemy.onShieldRestored += PlayShieldRestoredAnimation;
            enemy.Unit.onSelect += ShowName;
            enemy.Unit.onDeselect += HideName;
            enemy.onDeath += OnDeath;
        }

        private IEnumerator<float> ShowShield()
        {
            yield return Timing.WaitForSeconds(1.5f);
            shield.SetActive(true);
            shieldCount.gameObject.SetActive(true);
        }

        private void PlayShieldDamagedAnimation(int count)
        {
            if (count > 0 && count != enemy.maxShieldCount)
            {
                shieldAnim.SetTrigger(AnimationHandler.ShieldDamage);
            }
        }

        private void PlayShieldBrokenAnimation()
        {
            TimeManager.SlowMotionSequence(0.35f, 0.75f);
            //TODO: Play sound effect
            shieldAnim.SetTrigger(AnimationHandler.ShieldBreak);
        }

        private void PlayShieldRestoredAnimation() => shieldAnim.SetTrigger(AnimationHandler.ShieldRestore);

        private void UpdateShieldCountUI(int count) => shieldCount.text = count.ToString();

        private void ShowShieldCount() => shieldCount.gameObject.SetActive(true);

        private void HideShieldCount() => shieldCount.gameObject.SetActive(false);
        
        private void ShowName() => enemyName.gameObject.SetActive(true);
        
        private void HideName() => enemyName.gameObject.SetActive(false);
        
        private void OnDeath(UnitBase unit) => gameObject.SetActive(false);

        private void OnDisable()
        {
            enemy.onShieldValueChanged -= UpdateShieldCountUI;
            enemy.onShieldValueChanged -= PlayShieldDamagedAnimation;
            enemy.onShieldBroken -= PlayShieldBrokenAnimation;
            enemy.onShieldBroken -= HideShieldCount;
            enemy.onShieldRestored -= ShowShieldCount;
            enemy.onShieldRestored -= PlayShieldRestoredAnimation;
            enemy.Unit.onSelect -= ShowName;
            enemy.Unit.onDeselect -= HideName;
            enemy.onDeath -= OnDeath;
        }
    }
}
