using Characters;
using Characters.Abilities;
using Characters.ElementalTypes;
using Characters.PartyMembers;
using Characters.StatusEffects;
using JetBrains.Annotations;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

public class ScriptableObjectEditor : OdinMenuEditorWindow
{
    [MenuItem("Tools/Scriptable Object Editor")]
    private static void OpenWindow()
    {
        GetWindow<ScriptableObjectEditor>().Show();
    }

    private CreateNewEnemy createNewEnemy;
    private CreateNewPhysicalAttack createNewPhysicalAttack;
    private CreateNewRangedAttack createNewRangedAttack;
    private CreateNewNonAttack createNewNonAttack;
    private CreateNewDotEffect createNewDotEffect;
    private CreateNewElementalType createNewElementalType;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (createNewEnemy != null) DestroyImmediate(createNewEnemy._enemy);
        if (createNewPhysicalAttack != null) DestroyImmediate(createNewPhysicalAttack._ability);
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        createNewEnemy = new CreateNewEnemy();
        createNewPhysicalAttack = new CreateNewPhysicalAttack();
        createNewRangedAttack = new CreateNewRangedAttack();
        createNewNonAttack = new CreateNewNonAttack();
        createNewDotEffect = new CreateNewDotEffect();
        createNewElementalType = new CreateNewElementalType();

        var tree = new OdinMenuTree
        {
            {"Create New/Enemy", createNewEnemy},
            {"Create New/Ability/Physical Attack", createNewPhysicalAttack},
            {"Create New/Ability/Ranged Attack", createNewRangedAttack},
            {"Create New/Ability/Non-Attack", createNewNonAttack},
            {"Create New/Status Effect/Damage Over Time", createNewDotEffect},
            {"Create New/Elemental Type", createNewElementalType}
        };
        
        tree.AddAllAssetsAtPath("Party Members", "Scriptable Objects/Characters/Party Members", typeof(PartyMember));
        tree.AddAllAssetsAtPath("Enemies", "Scriptable Objects/Characters/Enemies", typeof(Enemy));
        tree.AddAllAssetsAtPath("Abilities/Physical Attacks","Scripts/Characters/Abilities/Physical Attacks", typeof(Ability));
        tree.AddAllAssetsAtPath("Abilities/Ranged Attacks","Scripts/Characters/Abilities/Ranged Attacks", typeof(Ability));
        tree.AddAllAssetsAtPath("Abilities/Non-Attacks","Scripts/Characters/Abilities/Non-attacks", typeof(Ability));
        tree.AddAllAssetsAtPath("Status Effects/Damage Over Time", "Scripts/Characters/StatusEffects/DOT", typeof(StatusEffect));
        tree.AddAllAssetsAtPath("Status Effects/Stat Change", "Scripts/Characters/StatusEffects/StatChange/Buffs", typeof(StatusEffect));
        tree.AddAllAssetsAtPath("Status Effects/Stat Change", "Scripts/Characters/StatusEffects/StatChange/Debuffs", typeof(StatusEffect));
        tree.AddAllAssetsAtPath("Status Effects/Inhibiting", "Scripts/Characters/StatusEffects/InhibitingEffect", typeof(StatusEffect));
        tree.AddAllAssetsAtPath("Status Effects/AI Effect", "Scripts/Characters/StatusEffects/AIEffect", typeof(StatusEffect));
        tree.AddAllAssetsAtPath("Elemental Types", "Assets/Scripts/Characters/ElementalTypes", typeof(ElementalType));
        tree.AddAllAssetsAtPath("Weapons", "Scriptable Objects/Weapons", typeof(WeaponItem));
        tree.AddAllAssetsAtPath("", "Scriptable Objects", typeof(GlobalVariables));

        return tree;
    }
    
    public class CreateNewEnemy
    {
        public CreateNewEnemy()
        {
            _enemy = CreateInstance<Enemy>();
            _enemy.name = "New Enemy";
        }
        
        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public Enemy _enemy;

        [Button("Add New Enemy")]
        private void CreateNewData()
        {
            AssetDatabase.CreateAsset(_enemy, "Assets/Characters/Enemies/" + _enemy.characterName + ".asset");
            AssetDatabase.SaveAssets();
            
            _enemy = CreateInstance<Enemy>();
            _enemy.name = "New Enemy";
        }
    }
    
    public class CreateNewPhysicalAttack
    {
        [UsedImplicitly] public string _name;

        public CreateNewPhysicalAttack()
        {
            _ability = CreateInstance<PhysicalAttack>();
            _ability.name = _name;
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public PhysicalAttack _ability;

        [Button("Add New Physical Attack")]
        private void CreateNewData()
        {
            AssetDatabase.CreateAsset(_ability, "Assets/Scripts/Characters/Abilities/Physical Attacks/" + _name + ".asset");
            AssetDatabase.SaveAssets();
            
            _ability = CreateInstance<PhysicalAttack>();
        }
    }
    
    public class CreateNewRangedAttack
    {
        [UsedImplicitly] public string _name;

        public CreateNewRangedAttack()
        {
            _ability = CreateInstance<RangedAttack>();
            _ability.name = _name;
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public RangedAttack _ability;

        [Button("Add New Ranged Attack")]
        private void CreateNewData()
        {
            AssetDatabase.CreateAsset(_ability, "Assets/Scripts/Characters/Abilities/Ranged Attacks/" + _name + ".asset");
            AssetDatabase.SaveAssets();
            
            _ability = CreateInstance<RangedAttack>();
        }
    }
    
    public class CreateNewNonAttack
    {
        [UsedImplicitly] public string _name;

        public CreateNewNonAttack()
        {
            _ability = CreateInstance<NonAttack>();
            _ability.name = _name;
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public NonAttack _ability;

        [Button("Add New Non-Attack")]
        private void CreateNewData()
        {
            AssetDatabase.CreateAsset(_ability, "Assets/Scripts/Characters/Abilities/Non-attacks/" + _name + ".asset");
            AssetDatabase.SaveAssets();
            
            _ability = CreateInstance<NonAttack>();
        }
    }

    public class CreateNewDotEffect
    {
        [UsedImplicitly] public string _name;

        public CreateNewDotEffect()
        {
            _statusEffect = CreateInstance<DamageOverTime>();
            _statusEffect.name = _name;
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public DamageOverTime _statusEffect;

        [Button("Add New DOT Effect")]
        private void CreateNewData()
        {
            AssetDatabase.CreateAsset(_statusEffect, "Assets/Scripts/Characters/StatusEffects/DOT/" + _name + ".asset");
            AssetDatabase.SaveAssets();
            
            _statusEffect = CreateInstance<DamageOverTime>();
        }
    }

    public class CreateNewElementalType
    {
        [UsedImplicitly] public string _name;

        public CreateNewElementalType()
        {
            _elementalType = CreateInstance<ElementalType>();
            _elementalType.name = _name;
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public ElementalType _elementalType;

        [Button("Add New Elemental Type")]
        private void CreateNewData()
        {
            AssetDatabase.CreateAsset(_elementalType, "Assets/Scripts/Characters/ElementalTypes/" + _name + ".asset");
            AssetDatabase.SaveAssets();
            
            _elementalType = CreateInstance<ElementalType>();
        }
    }
}
