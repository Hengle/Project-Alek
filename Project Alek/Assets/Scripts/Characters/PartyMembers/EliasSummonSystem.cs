using System.Collections.Generic;
using System.Linq;
using BattleSystem;
using BattleSystem.Mechanics;
using BattleSystem.UI;
using Characters.Abilities;
using Characters.Animations;
using Characters.Enemies;
using DG.Tweening;
using JetBrains.Annotations;
using MEC;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Characters.PartyMembers
{
    public enum SummonState { Summon, Command }
    public class EliasSummonSystem : MonoBehaviour, IGameEventListener<GameObject>, IGameEventListener<UnitBase, CharacterGameEvent>
    {
        [SerializeField] private PartyMember elias;
        [SerializeField] private Ability summonAbility;
        [SerializeField] private Ability analyzeAbility;
        [SerializeField] private SummonsDatabase database;
        [SerializeField] private Material summonMaterial;
        [SerializeField] private GameObject summonsMenuGO;
        [SerializeField] private GameObject chargeEffect;
        [SerializeField] private Transform summonSpawnPoint;
        [SerializeField] private AnimationClip commandAnimation;

        [SerializeField] [ReadOnly] private Enemy enemyToRegister;
        [SerializeField] [ReadOnly] private Enemy currentSummon;
        [SerializeField] [ReadOnly] private GameObject currentSummonGO;
        [SerializeField] [ReadOnly] private GameObject activeMenu;
        [SerializeField] [ReadOnly] private GameObject commandMenuFirstSelected;
        [SerializeField] [ReadOnly] private GameObject abilityMenuLastSelected;
        [SerializeField] [ReadOnly] private SummonState summonState;
        [SerializeField] [ReadOnly] private int summonIndex;
        [SerializeField] [ReadOnly] private Enemy[] availableSummons;

        private InfoBoxUI notificationHandler;
        private Transform runTimeSpawnPoint;
        private GameObject summonMenuCanvas;
        private GameObject summonsMenu;
        private GameObject[] summonOptionButtons;
        private GameObject[] commandMenus;
        private GameObject summonsMenuFirstSelected;
        private GameObject summonButtonGO;
        private Controls controls;

        [SerializeField] [ReadOnly] private bool isAnalyzing;
        [SerializeField] [ReadOnly] private float hpValueAtStartOfAnalyze;

        private void Awake()
        {
            if (!elias.abilities.Contains(summonAbility)) return;

            notificationHandler = GameObject.FindWithTag("Notification Handler").GetComponent<InfoBoxUI>();
            
            DOTween.Init();
            
            controls = new Controls();
            controls.Enable();
            controls.Battle.Back.performed += ctx => CloseSubMenu();
            
            InstantiateRuntimeSpawnPoint();
            
            BattleEvents.Instance.overrideButtonEvent.AddListener(this);
            BattleEvents.Instance.characterAttackEvent.AddListener(this);
            BattleEvents.Instance.endOfTurnEvent.AddListener(this);
            BattleEvents.Instance.characterTurnEvent.AddListener(this);
        }

        private void OnDisable()
        {
            if (!elias.abilities.Contains(summonAbility)) return;
            BattleEvents.Instance.overrideButtonEvent.RemoveListener(this);
            BattleEvents.Instance.characterAttackEvent.RemoveListener(this);
            BattleEvents.Instance.endOfTurnEvent.RemoveListener(this);
            BattleEvents.Instance.characterTurnEvent.RemoveListener(this);
            
            if (isAnalyzing) elias.onHpValueChanged -= StopAnalyzeIfDamaged;
        }

        private void InstantiateRuntimeSpawnPoint()
        {
            var transform1 = summonSpawnPoint.transform;
            var position = transform1.position;
            var newPosition = new Vector3(position.x - 0.5f, position.y, position.z + 0.75f);
            
            runTimeSpawnPoint = Instantiate(summonSpawnPoint, newPosition, transform1.rotation);
        }

        private void FindDependencies(Transform summonMenuGO)
        {
            summonsMenu = summonMenuGO.Find("Summon Options Menu").gameObject;
            var commandMenuTransform = summonMenuGO.Find("Command Menus");
            
            summonOptionButtons = new GameObject[3];
            commandMenus = new GameObject[3];

            var i = 0;
            foreach (Transform child in summonsMenu.transform)
            {
                summonOptionButtons[i] = child.gameObject;
                i++;
            }

            i = 0;
            foreach (Transform child in commandMenuTransform)
            {
                commandMenus[i] = child.gameObject;
                i++;
            }

            summonsMenuFirstSelected = summonOptionButtons[0];
            summonMenuCanvas.SetActive(false);
        }

        private bool InstantiateGO()
        {
            summonMenuCanvas = Instantiate(summonsMenuGO, elias.battlePanel.transform);
            FindDependencies(summonMenuCanvas.transform);
            return true;
        }

        private IEnumerator<float> SetSummonOptions(GameObject summonButton)
        {
            var summons = database.GetEquippedSummons().ToArray();
            availableSummons = new Enemy[summons.Length];
            
            for (var i = 0; i < summons.Length; i++)
            {
                var s = Instantiate(summons[i]);
                var enemyGo = Instantiate(s.characterPrefab, runTimeSpawnPoint);
                enemyGo.transform.localScale = new Vector3(5,5,1);

                SetupSummon(s, enemyGo, i);
                enemyGo.SetActive(false);
            }
            
            summonButtonGO = summonButton;
            summonButtonGO.GetComponent<Button>().onClick.AddListener(OpenSubMenu);
            
            yield return Timing.WaitUntilTrue(InstantiateGO);
            yield return Timing.WaitForOneFrame;

            for (var i = 0; i < availableSummons.Length; i++)
            {
                var button = summonOptionButtons[i];
                var summon = availableSummons[i];
                var param = summonAbility.GetParameters(elias.abilities.IndexOf(summonAbility));
                var index = i;

                button.GetComponentInChildren<TextMeshProUGUI>().text = summon.characterName;
                button.GetComponent<Button>().onClick.AddListener(delegate
                {
                    ((BattleOptionsPanel) elias.battleOptionsPanel).GetCommandInformation(param, true);
                    summonButtonGO.GetComponentInChildren<TextMeshProUGUI>().text = SummonState.Command.ToString();
                    CloseSubMenu();
                    elias.Unit.currentAbility = summonAbility;
                    currentSummon = summon;
                    summonIndex = index;
                    elias.Unit.isAbility = true;
                });
                
                button.SetActive(true);
                SetCommandOptions(summon, i);
            }
        }

        private void SetupSummon(Enemy enemy, GameObject enemyGo, int i)
        {
            var spriteRenderer = enemyGo.GetComponent<SpriteRenderer>();
            spriteRenderer.material = summonMaterial;
            spriteRenderer.sortingOrder = -1;
            
            enemyGo.GetComponent<UnitAnimatorFunctions>().OverrideUnit(elias.Unit, summonMaterial);
            enemyGo.transform.rotation = summonSpawnPoint.rotation;
            enemyGo.name = enemy.name;

            enemy.id = CharacterType.PartyMember;
            enemy.Unit = elias.Unit;
            enemy.Unit.parent = elias;
            enemy.isSummon = true;
            enemy.master = elias;
            enemy.summonParent = runTimeSpawnPoint;
            enemy.summonHandler = enemyGo.GetComponent<AnimationHandler>();
            enemy.summonGO = enemyGo;
            availableSummons[i] = enemy;
            RemoveUnnecessaryComponents(enemyGo);
        }

        private static void RemoveUnnecessaryComponents(GameObject enemyGO)
        {
            Destroy(enemyGO.GetComponent<Unit>());
            Destroy(enemyGO.GetComponent<ChooseTarget>());
            Destroy(enemyGO.GetComponent<SpriteOutline>());
            Destroy(enemyGO.GetComponent<Button>());
            Destroy(enemyGO.GetComponent<WeaknessIndicatorUI>());
            Destroy(enemyGO.GetComponent<WeakAndResUnlockSystem>());
            Destroy(enemyGO.GetComponent<BreakSystem>());
        }

        private void SetCommandOptions(Enemy summon, int index)
        {
            var menu = commandMenus[index];
            var children = new GameObject[4];
            
            var j = 0;
            foreach (Transform child in menu.transform)
            {
                children[j] = child.gameObject;
                j++;
            }

            summon.summonAnim = summon.summonGO.GetComponent<Animator>();
            summon.animOverride = (AnimatorOverrideController) summon.summonAnim.runtimeAnimatorController;

            for (var i = 0; i < summon.abilities.Count; i++)
            {
                children[i].GetComponentInChildren<TextMeshProUGUI>().text = summon.abilities[i].name;
                children[i].transform.Find("Icon").GetComponent<Image>().sprite = summon.abilities[i].icon;
                children[i].GetComponent<InfoBoxUI>().information =
                    $"{summon.abilities[i].description}\n" +
                    $"({summon.abilities[i].actionCost} AP)";
                children[i].SetActive(true);
                
                var param = summon.abilities[i].GetParameters(i);
                var ability = summon.abilities[i];
                
                children[i].GetComponent<Button>().onClick.AddListener(delegate
                {
                    ((BattleOptionsPanel) elias.battleOptionsPanel).GetCommandInformation(param);
                    CloseSubMenu();
                    BattleInput._controls.Enable();
                    elias.Unit.currentAbility = ability;
                    elias.Unit.isAbility = true;
                });

                if (summon.abilities[i].isMultiTarget)
                    children[i].GetComponent<Button>().onClick.AddListener(delegate
                    {
                        ChooseTarget._isMultiTarget = true;
                    });
                
                ability.attackState = i+1;
                summon.animOverride[$"Ability {i+1}"] = ability.animation;
            }
            
            summon.summonAnim.runtimeAnimatorController = summon.animOverride;

            children[summon.abilities.Count].GetComponentInChildren<TextMeshProUGUI>().text = "Dismiss";
            children[summon.abilities.Count].SetActive(true);
            children[summon.abilities.Count].GetComponent<Button>().onClick.AddListener(delegate
            {
                summonButtonGO.GetComponentInChildren<TextMeshProUGUI>().text = SummonState.Summon.ToString();
                Timing.RunCoroutine(Dismiss());
            });
        }

        private void OpenSubMenu()
        {
            BattleInput._controls.Disable();
            abilityMenuLastSelected = EventSystem.current.currentSelectedGameObject;
            
            var position = summonButtonGO.transform.localPosition;
            var newPosition = new Vector3(position.x + 200, position.y, position.z);
            summonMenuCanvas.transform.localPosition = newPosition;
            summonMenuCanvas.SetActive(true);
            
            switch (summonState)
            {
                case SummonState.Summon:
                    activeMenu = summonsMenu;
                    activeMenu.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(summonsMenuFirstSelected);
                    break;
                case SummonState.Command:
                    activeMenu = commandMenus[summonIndex];
                    activeMenu.SetActive(true);
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(commandMenuFirstSelected);
                    break;
            }
        }

        private void CloseSubMenu()
        {
            if (BattleEngine.Instance.activeUnit != elias) return;
            if (!activeMenu || !activeMenu.activeSelf) return;
            
            activeMenu.SetActive(false);
            summonMenuCanvas.SetActive(false);
            BattleInput._controls.Enable();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(abilityMenuLastSelected);
        }

        private IEnumerator<float> Dismiss()
        {
            BattleInput._controls.Disable();
            elias.Unit.anim.Play($"Ability {summonAbility.attackState}", 0);
            ShowChargeEffect();

            yield return Timing.WaitForSeconds(0.75f);
            currentSummonGO.SetActive(false);
            
            chargeEffect.transform.DOScale(0.1f, 0.5f);
            chargeEffect.SetActive(false);
            
            CloseSubMenu();
            elias.Unit.hasSummon = false;
            summonState = SummonState.Summon;
            
            Invoke(nameof(SetSummonAnimation), 1);
            BattleInput._controls.Enable();
        }

        private IEnumerator<float> SetCommandAnimation()
        {
            yield return Timing.WaitUntilFalse(() => elias.AnimationHandler.performingAction);
            elias.Unit.animOverride[$"Ability {summonAbility.attackState}"] = commandAnimation;
            elias.Unit.anim.runtimeAnimatorController = elias.Unit.animOverride;
        }

        private void SetSummonAnimation()
        {
            elias.Unit.animOverride[$"Ability {summonAbility.attackState}"] = summonAbility.animation;
            elias.Unit.anim.runtimeAnimatorController = elias.Unit.animOverride;
        }

        private void BeginAnalyze(Enemy enemy)
        {
            isAnalyzing = true;
            enemyToRegister = enemy;
        }

        private void EndAnalyze()
        {
            if (!isAnalyzing) return;
            if (database.RegisterSummon(enemyToRegister, true))
            {
                notificationHandler.ShowNotification($"Successfully registered {enemyToRegister}");
            }
            elias.Unit.anim.SetBool(AnimationHandler.DontTranToIdle, false);
        }

        private void StopAnalyzeIfDamaged(float hp)
        {
            if (!(hp < hpValueAtStartOfAnalyze)) return;
            isAnalyzing = false;
            elias.Unit.anim.SetBool(AnimationHandler.DontTranToIdle, false);
            elias.onHpValueChanged -= StopAnalyzeIfDamaged;
            notificationHandler.ShowNotification("Analysis failed");
        }

        private void SetListenerForHpLowered()
        {
            hpValueAtStartOfAnalyze = elias.CurrentHP;
            elias.onHpValueChanged += StopAnalyzeIfDamaged;
        }

        [UsedImplicitly] public void Summon()
        {
            currentSummonGO = currentSummon.summonGO;
            currentSummonGO.SetActive(true);

            elias.Unit.hasSummon = true;
            elias.Unit.currentSummon = currentSummon;
            summonState = SummonState.Command;
            commandMenuFirstSelected = commandMenus[summonIndex].transform.GetChild(0).gameObject;

            chargeEffect.transform.DOScale(0.1f, 0.5f);
            chargeEffect.SetActive(false);

            Timing.RunCoroutine(SetCommandAnimation());
        }

        [UsedImplicitly] public void ShowChargeEffect()
        {
            chargeEffect.SetActive(true);
            chargeEffect.transform.DOScale(1, 0.5f);
        }

        public void OnEventRaised(GameObject value)
        {
            if (!elias.abilities.Contains(summonAbility)) return;
            Timing.RunCoroutine(SetSummonOptions(value));
        }

        public void OnEventRaised(UnitBase value1, CharacterGameEvent value2)
        {
            if (value1 == currentSummon && ((Enemy) value1).isSummon)
            {
                elias.Unit.anim.Play($"Ability {summonAbility.attackState}", 0);
            }

            if (value1 == elias && value2 == BattleEvents.Instance.characterAttackEvent &&
                elias.CurrentAbility == analyzeAbility && !elias.CurrentTarget.Unit.isCountered)
            {
                notificationHandler.ShowNotification($"Beginning analysis of {value1.characterName}");
                BeginAnalyze((Enemy)elias.CurrentTarget);
            }

            if (value1 == elias && value2 == BattleEvents.Instance.endOfTurnEvent && isAnalyzing)
            {
                SetListenerForHpLowered();
            }

            if (value1 == elias && value2 == BattleEvents.Instance.characterTurnEvent && isAnalyzing)
            {
                EndAnalyze();
            }
        }
    }
}