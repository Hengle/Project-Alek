using System.Linq;
using BattleSystem.Mechanics;
using BattleSystem.UI;
using Characters;
using Characters.Enemies;
using UnityEngine;
using UnityEngine.UI;
using Characters.PartyMembers;
using Characters.StatusEffects;
using MoreMountains.InventoryEngine;
using SingletonScriptableObject;
using TMPro;
using Random = UnityEngine.Random;

namespace BattleSystem.Generator
{
    public class BattleGenerator : MonoBehaviour
    {
        [HideInInspector] public BattleGeneratorDatabase database;

        private int offset;
        private int enemyOffset;

        private void Awake() => database = GetComponent<BattleGeneratorDatabase>();
        
        #region Setup
        
        public void SetupBattle()
        {
            SetOffsetPositions();
            SetupMainInventory();
            SetupParty();
            SpawnAndSetupEnemyTeam();
            SetupPartyMenuController();
        }

        private void SetupMainInventory()
        {
            var inventory = GameObject.Find("Main Inventory").GetComponent<Inventory>();
            database.inventoryItems.ForEach(i => inventory.AddItem(i, 1));
        }

        private void SetOffsetPositions()
        {
            if (PartyManager.Instance.partyMembers.Count == 1) offset = 2;
            else if (PartyManager.Instance.partyMembers.Count == 4) offset = 0;
            else offset = 1;

            if (EnemyManager.Instance.enemies.Count == 1) enemyOffset = 2;
            else if (EnemyManager.Instance.enemies.Count == 4) enemyOffset = 0;
            else enemyOffset = 1;
        }

        private void SetupParty()
        {
            for (var i = 0; i < PartyManager.Instance.partyMembers.Count; i++)
            {
                var memberGo = SpawnThisMember(PartyManager.Instance.partyMembers[i], i);
                SetupThisMember(PartyManager.Instance.partyMembers[i], i, memberGo);
            }
        }

        private static void SetupPartyMenuController()
        {
            foreach (var member in BattleEngine.Instance._membersForThisBattle)
            {
                SelectableObjectManager._memberSelectable.Add(member.Selectable);
                member.MenuController = member.battlePanel.GetComponent<MenuController>();
            }
        }

        private void SetupBattlePanel(PartyMember character)
        {
            character.battlePanel = Instantiate(database.battlePanelGO, database.battlePanelSpawnPoint, false);
            character.battleOptionsPanel = Instantiate(database.boPanel);
            ((BattleOptionsPanel) character.battleOptionsPanel).character = character;

            var apConversionBox = character.battlePanel.transform.Find("AP Conversion Box").gameObject;
            apConversionBox.GetComponent<APConversionControllerUI>().unit = character.Unit;
            
            var mainMenu = character.battlePanel.transform.Find("Battle Menu").transform.Find("Mask").transform.Find("Main Options");

            mainMenu.Find("Attack Button").gameObject.GetComponent<Button>().onClick.AddListener
                ( delegate { ((BattleOptionsPanel) character.battleOptionsPanel).GetCommandInformation("UniversalAction,1,0,2"); });

            mainMenu.Find("Abilities Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { ((BattleOptionsPanel) character.battleOptionsPanel).OnAbilityMenuButton(); });
            
            mainMenu.Find("Spells Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { ((BattleOptionsPanel) character.battleOptionsPanel).OnSpellMenuButton(); });
            
            mainMenu.Find("Inventory Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { BattleEngine.Instance.inventoryInputManager.OpenInventory(); });

            mainMenu.Find("End Turn Button").gameObject.GetComponent<Button>().onClick.AddListener
                (delegate { ((BattleOptionsPanel) character.battleOptionsPanel).OnEndTurnButton(); });

            character.BattlePanelAnim = character.battlePanel.GetComponent<Animator>();
            
            character.battlePanel.SetActive(false);
        }
        
        private static void SetAbilityMenuOptions(PartyMember character)
        {
            var abilityMenu = character.battlePanel.transform.Find("Battle Menu").
                transform.Find("Mask").transform.Find("Ability Menu").transform;
            var abilityListIndex = 0;
            
            while (character.abilities.Count > 5) character.abilities.Remove(character.abilities[character.abilities.Count-1]);

            for (var buttonIndex = 0; buttonIndex < character.abilities.Count; buttonIndex++)
            {
                var optionButton = abilityMenu.GetChild(buttonIndex).gameObject;
                optionButton.GetComponentInChildren<TextMeshProUGUI>().text = character.abilities[abilityListIndex].name;
                
                optionButton.transform.Find("Icon").GetComponent<Image>().sprite = character.abilities[abilityListIndex].icon;
                optionButton.SetActive(true);
                
                var param = character.abilities[abilityListIndex].GetParameters(abilityListIndex);
                var ability = character.abilities[abilityListIndex];
                
                optionButton.GetComponent<Button>().onClick.
                    AddListener(delegate { ((BattleOptionsPanel) character.battleOptionsPanel).GetCommandInformation(param);
                        character.Unit.currentAbility = ability; character.Unit.isAbility = true; });

                if (character.abilities[abilityListIndex].isMultiTarget) optionButton.GetComponent<Button>().onClick.
                    AddListener(delegate { ChooseTarget._isMultiTarget = true; });
                    
                optionButton.GetComponent<InfoBoxUI>().information =
                    $"{character.abilities[abilityListIndex].description}\n" +
                    $"({character.abilities[abilityListIndex].actionCost} AP)";
                
                ability.attackState = abilityListIndex+1;
                character.Unit.animOverride[$"Ability {abilityListIndex+1}"] = ability.animation;

                abilityListIndex++;

                if (buttonIndex != character.abilities.Count - 1) continue;
                
                var specialAttackIndex = character.abilities.Count;
                var specialAttackButton = abilityMenu.GetChild(specialAttackIndex).gameObject;
                specialAttackButton.GetComponentInChildren<TextMeshProUGUI>().text = character.specialAttack.name;
                
                specialAttackButton.transform.Find("Icon").GetComponent<Image>().sprite = character.specialAttack.icon;
                specialAttackButton.AddComponent<SpecialAttackButtonUI>().member = character;
                specialAttackButton.SetActive(true);

                specialAttackButton.GetComponent<Button>().onClick.AddListener
                    ( delegate { ((BattleOptionsPanel) character.battleOptionsPanel).
                    GetCommandInformation($"UniversalAction,2,0,{character.CurrentAP}");
                    character.Unit.specialAttackAP = character.Unit.currentAP;
                    character.Unit.currentAbility = character.specialAttack; });
                
                specialAttackButton.GetComponent<InfoBoxUI>().information = $"{character.specialAttack.description}";

                if (character.abilities.Count == 5) return;
                
                var firstOption = abilityMenu.GetChild(0).gameObject;
                var firstOpNav = firstOption.GetComponent<Selectable>().navigation;
                var nav = specialAttackButton.GetComponent<Selectable>().navigation;

                nav.selectOnDown = firstOption.GetComponent<Button>();
                specialAttackButton.GetComponent<Selectable>().navigation = nav;
                
                firstOpNav.selectOnUp = specialAttackButton.GetComponent<Button>();
                firstOption.GetComponent<Selectable>().navigation = firstOpNav;
            }

            character.Unit.anim.runtimeAnimatorController = character.Unit.animOverride;
        }
        
        private static void SetSpellMenuOptions(PartyMember character)
        {
            var spellMenu = character.battlePanel.transform.Find("Battle Menu").
                transform.Find("Mask").transform.Find("Spell Menu").transform;
            var spellListIndex = 0;
            
            while (character.spells.Count > 5) character.abilities.Remove(character.spells[character.spells.Count-1]);

            for (var buttonIndex = 0; buttonIndex < character.spells.Count; buttonIndex++)
            {
                var optionButton = spellMenu.GetChild(buttonIndex).gameObject;
                optionButton.GetComponentInChildren<TextMeshProUGUI>().text = character.spells[spellListIndex].name;
                
                optionButton.transform.Find("Icon").GetComponent<Image>().sprite = character.spells[spellListIndex].icon;
                optionButton.SetActive(true);
                
                var param = character.spells[spellListIndex].GetParameters(spellListIndex);
                var spell = character.spells[spellListIndex];
                
                optionButton.GetComponent<Button>().onClick.
                    AddListener(delegate { ((BattleOptionsPanel) character.battleOptionsPanel).GetCommandInformation(param);
                        character.Unit.currentAbility = spell; character.Unit.isAbility = true; });

                if (character.spells[spellListIndex].isMultiTarget) optionButton.GetComponent<Button>().onClick.
                    AddListener(delegate { ChooseTarget._isMultiTarget = true; });
                    
                optionButton.GetComponent<InfoBoxUI>().information =
                    $"{character.spells[spellListIndex].description}\n" +
                    $"({character.spells[spellListIndex].actionCost} AP)";
                
                spell.attackState = spellListIndex+1;
                //character.Unit.animOverride[$"Spell {spellListIndex+1}"] = spell.animation;

                spellListIndex++;

                if (buttonIndex != character.spells.Count - 1) continue;

                var firstOption = spellMenu.GetChild(0).gameObject;
                var firstOpNav = firstOption.GetComponent<Selectable>().navigation;
                var nav = optionButton.GetComponent<Selectable>().navigation;

                nav.selectOnDown = firstOption.GetComponent<Button>();
                optionButton.GetComponent<Selectable>().navigation = nav;
                
                firstOpNav.selectOnUp = optionButton.GetComponent<Button>();
                firstOption.GetComponent<Selectable>().navigation = firstOpNav;
            }

            //character.Unit.anim.runtimeAnimatorController = character.Unit.animOverride;
        }

        private void SetupInventoryDisplay(PartyMember character, int i)
        {
            var inventory = GameObject.Find("Main Inventory").GetComponent<Inventory>();

            var weaponInventory = Instantiate(character.weaponInventory, character.Unit.transform, true);
            weaponInventory.name = character.weaponInventory.name;

            var armorInventory = Instantiate(character.armorInventory, character.Unit.transform, true);
            armorInventory.name = character.armorInventory.name;

            character.inventoryDisplay = database.inventoryCanvases[i];
            
            var mainDisplay = character.inventoryDisplay.transform.Find("InventoryDisplay").GetComponent<InventoryDisplay>();
            mainDisplay.TargetInventoryName = $"{inventory.name}";
            mainDisplay.SetupInventoryDisplay();

            var weaponDisplay = character.inventoryDisplay.transform.Find("Weapon Display").GetComponent<InventoryDisplay>();
            weaponDisplay.TargetInventoryName = $"{character.weaponInventory.name}";
            weaponDisplay.SetupInventoryDisplay();

            var armorDisplay = character.inventoryDisplay.transform.Find("Armor Display").GetComponent<InventoryDisplay>();
            armorDisplay.TargetInventoryName = $"{character.armorInventory.name}";
            armorDisplay.SetupInventoryDisplay();

            var details = character.inventoryDisplay.GetComponentInChildren<InventoryDetails>();
            details.TargetInventoryName = $"{inventory.name}";
            
            if (character.equippedWeapon == null) Debug.LogError($"{character.characterName} has no weapon!");
            character.equippedWeapon.partyMember = character;
        }

        private void SetupProfileBox(UnitBase character)
        {
            var parent = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Profiles");
            var profileBox = Instantiate(database.profileBox, parent, false);
            profileBox.gameObject.GetComponent<ProfileBoxManagerUI>().SetupProfileBox(character);
        }

        private void SetupCharacterPanel(PartyMember character, int i)
        {
            var panel = database.characterPanels[i + offset];
            var statusBoxController = panel.transform.Find("Status Effects").GetComponent<StatusEffectControllerUI>();
            
            panel.GetComponent<CharacterPanelControllerUI>().member = character;
            panel.SetActive(true);
            
            statusBoxController.member = character;
            statusBoxController.Initialize();
        }

        private static void SetupChooseTargetScript(UnitBase character)
        {
            var chooseTarget = character.Unit.gameObject.GetComponent<ChooseTarget>();
            chooseTarget.thisUnitBase = character;
            chooseTarget.enabled = true;
        }

        private void SetupEnemyStatusBox(UnitBase clone, GameObject enemyGo)
        {
            var position = enemyGo.transform.position;
            var newPosition = new Vector3(position.x, position.y + 2.25f, position.z);

            var statusBox = Instantiate(database.enemyStatusBox, newPosition,
                database.enemyStatusBox.rotation);
                
            statusBox.transform.SetParent(clone.Unit.transform);
                
            var statusBoxController = statusBox.GetComponentInChildren<StatusEffectControllerUI>();
            statusBoxController.member = clone;
            statusBoxController.Initialize();
        }

        private void SetupEnemyShield(Enemy clone, GameObject enemyGo)
        {
            var position = enemyGo.transform.position;
            var newPosition = new Vector3(position.x, position.y - 0.5f, position.z);
                
            var shieldTransform = Instantiate(database.shieldTransform, newPosition,
                database.shieldTransform.rotation);
                
            //shieldTransform.transform.SetParent(clone.Unit.transform);
            shieldTransform.transform.SetParent(database.worldSpaceCanvas.transform);

            var shieldController = shieldTransform.GetComponent<BreakSystemControllerUI>();
            shieldController.enemy = clone;
            shieldController.Initialize();
        }
        
        private static void SetupUnit(UnitBase character, GameObject unitGo)
        {
            var unit = unitGo.GetComponent<Unit>();
            character.Unit = unit;
            unit.parent = character;
            unit.name = character.characterName;
            unit.status = Status.Normal;
            
            // TODO: Would need to update UI slider if i want to be able to modify max health
            unit.maxHealthRef = (int) unit.parent.health.Value;
            unit.currentHP = (int) unit.parent.health.Value;
     
            unit.currentAP = UnitBase.MaxAP;
            unit.outline.color = character.Color;
        }

        private static void SetupSpecialAttackSystem(PartyMember character, GameObject unitGo)
        {
            var system = unitGo.GetComponent<SpecialAttackSystem>();
            system.member = character;
            system.SpecialBarVal = system.member.specialAttackBarVal;
        }

        private void SetupMemberCameras(int i)
        {
            database.closeUpCameras[i + offset].SetActive(true);
            database.criticalCameras[i + offset].SetActive(true);
        }

        private static void SetupEnemyAbilities(UnitBase enemy)
        {
            for (var a = 0; a < enemy.abilities.Count; a++)
            {
                enemy.abilities[a].attackState = a+1;
                enemy.Unit.animOverride[$"Ability {a+1}"] = enemy.abilities[a].animation;
            }
            enemy.Unit.anim.runtimeAnimatorController = enemy.Unit.animOverride;
        }

        #endregion

        #region Spawning

        private void SetupThisMember(PartyMember character, int i, GameObject memberGo)
        {
            SetupUnit(character, memberGo);
            SetupChooseTargetScript(character);
            SetupBattlePanel(character);
            SetAbilityMenuOptions(character);
            SetSpellMenuOptions(character);
            SetupInventoryDisplay(character, i);
            SetupProfileBox(character);
            SetupCharacterPanel(character, i);
            SetupSpecialAttackSystem(character, memberGo);
            SetupMemberCameras(i);

            character.Selectable = character.Unit.gameObject.GetComponent<Selectable>();
            
            BattleEngine.Instance._membersForThisBattle.Add(character);
        }

        private GameObject SpawnThisMember(UnitBase character, int i)
        {
            var memberGo = Instantiate(character.characterPrefab, database.characterSpawnPoints[i + offset].transform);
            memberGo.transform.localScale = character.scale;
            return memberGo;
        }
        
        private GameObject SpawnThisEnemy(UnitBase character, int i)
        {
            var enemyGo = Instantiate(character.characterPrefab, database.enemySpawnPoints[i+enemyOffset].transform);
            enemyGo.name = character.name;
            enemyGo.transform.localScale = character.scale;
            return enemyGo;
        }

        private void SpawnAndSetupEnemyTeam()
        {
            var i = 0;
            foreach (var clone in EnemyManager.Instance.enemies.Select(Instantiate))
            {
                //TODO: Make randomizer for enemy stats
                clone.initiative.BaseValue = (int) Random.Range(clone.initiative.BaseValue - 2, clone.initiative.BaseValue + 2);

                var enemyGo = SpawnThisEnemy(clone, i);
                SetupUnit(clone, enemyGo);
                
                SetupEnemyAbilities(clone);
                
                clone.Selectable = clone.Unit.gameObject.GetComponent<Selectable>();
                SelectableObjectManager._enemySelectable.Add(clone.Selectable);

                SetupChooseTargetScript(clone);
                SetupProfileBox(clone);
                SetupEnemyStatusBox(clone, enemyGo);
                SetupEnemyShield(clone, enemyGo);
                
                enemyGo.GetComponent<BreakSystem>().Init(clone, clone.maxShieldCount);
                enemyGo.GetComponent<WeakAndResUnlockSystem>().Initialize(clone);

                BattleEngine.Instance._enemiesForThisBattle.Add(clone);
                BattleEngine.Instance._expGivers.Add(clone);
                i++;
            }
        }

        #endregion
    }
}
