using Characters;
using Characters.Abilities;
using Characters.ElementalTypes;
using Characters.Enemies;
using Characters.PartyMembers;
using Characters.StatusEffects;
using JetBrains.Annotations;
using MoreMountains.InventoryEngine;
using SingletonScriptableObject;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
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
    private CreateNewClass createNewClass;
    private CreateNewWeapon createNewWeapon;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (createNewEnemy != null) DestroyImmediate(createNewEnemy._enemy);
        if (createNewPhysicalAttack != null) DestroyImmediate(createNewPhysicalAttack._ability);
        if (createNewRangedAttack != null) DestroyImmediate(createNewRangedAttack._ability);
        if (createNewNonAttack != null) DestroyImmediate(createNewNonAttack._ability);
        if (createNewDotEffect != null) DestroyImmediate(createNewDotEffect._statusEffect);
        if (createNewElementalType != null) DestroyImmediate(createNewElementalType._elementalType);
        if (createNewClass != null) DestroyImmediate(createNewClass._class);
        if (createNewWeapon != null) DestroyImmediate(createNewWeapon._weaponItem);
    }
    
    protected override OdinMenuTree BuildMenuTree()
    {
        createNewEnemy = new CreateNewEnemy();
        createNewPhysicalAttack = new CreateNewPhysicalAttack();
        createNewRangedAttack = new CreateNewRangedAttack();
        createNewNonAttack = new CreateNewNonAttack();
        createNewDotEffect = new CreateNewDotEffect();
        createNewElementalType = new CreateNewElementalType();
        createNewClass = new CreateNewClass();
        createNewWeapon = new CreateNewWeapon();

        var tree = new OdinMenuTree (supportsMultiSelect: true)
        {
            {"Create New/Enemy", createNewEnemy},
            {"Create New/Ability/Physical Attack", createNewPhysicalAttack},
            {"Create New/Ability/Ranged Attack", createNewRangedAttack},
            {"Create New/Ability/Non-Attack", createNewNonAttack},
            {"Create New/Status Effect/Damage Over Time", createNewDotEffect},
            {"Create New/Elemental Type", createNewElementalType},
            {"Create New/Class", createNewClass},
            {"Create New/Weapon", createNewWeapon}
        };
        
        tree.AddAllAssetsAtPath("Party Members", "Scriptable Objects/Characters/Party Members", typeof(PartyMember));
        tree.AddAllAssetsAtPath("Enemies", "Scriptable Objects/Characters/Enemies", typeof(Enemy));
        tree.AddAllAssetsAtPath("Classes/Alek", "Scriptable Objects/Classes/Alek Ezana", typeof(Class));
        tree.AddAllAssetsAtPath("Classes/Leandra", "Scriptable Objects/Classes/Leandra Valentina", typeof(Class));
        tree.AddAllAssetsAtPath("Classes/Lilith", "Scriptable Objects/Classes/Lilith Morana", typeof(Class));
        tree.AddAllAssetsAtPath("Classes/Elias", "Scriptable Objects/Classes/Elias Adwin", typeof(Class));
        tree.AddAllAssetsAtPath("Abilities/Physical Attacks","Scriptable Objects/Abilities/Physical", typeof(Ability));
        tree.AddAllAssetsAtPath("Abilities/Ranged Attacks","Scriptable Objects/Abilities/Ranged", typeof(Ability));
        tree.AddAllAssetsAtPath("Abilities/Non-Attacks","Scriptable Objects/Abilities/Non-attacks", typeof(Ability));
        tree.AddAllAssetsAtPath("Abilities/Special Attacks","Scriptable Objects/Abilities/Special Attacks", typeof(Ability));
        tree.AddAllAssetsAtPath("Status Effects/Damage Over Time", "Scriptable Objects/Status Effects/DOT", typeof(StatusEffect));
        tree.AddAllAssetsAtPath("Status Effects/Stat Change", "Scriptable Objects/Status Effects/StatChange/Buffs", typeof(StatusEffect));
        tree.AddAllAssetsAtPath("Status Effects/Stat Change", "Scriptable Objects/Status Effects/StatChange/Debuffs", typeof(StatusEffect));
        tree.AddAllAssetsAtPath("Status Effects/Inhibiting", "Scriptable Objects/Status Effects/InhibitingEffect", typeof(StatusEffect));
        tree.AddAllAssetsAtPath("Status Effects/AI Effect", "Scriptable Objects/Status Effects/AIEffect", typeof(StatusEffect));
        tree.AddAllAssetsAtPath("Elemental Types", "Scriptable Objects/ElementalTypes", typeof(ElementalType));
        tree.AddAllAssetsAtPath("Weapons", "Scriptable Objects/Weapons", typeof(WeaponItem));
        tree.AddAllAssetsAtPath("", "Resources", typeof(PartyManager));
        tree.AddAllAssetsAtPath("", "Resources", typeof(EnemyManager));
        tree.AddAllAssetsAtPath("", "Resources", typeof(GlobalVariables)).AddIcon(EditorIcons.SettingsCog);
        
        return tree;
    }

    #region Create
    
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
            AssetDatabase.CreateAsset(_enemy, "Assets/Scriptable Objects/Characters/Enemies/" + _enemy.characterName + ".asset");
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
            AssetDatabase.CreateAsset(_ability, "Assets/Scriptable Objects/Abilities/Physical/" + _name + ".asset");
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
            AssetDatabase.CreateAsset(_ability, "Assets/Scriptable Objects/Abilities/Ranged/" + _name + ".asset");
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
            AssetDatabase.CreateAsset(_ability, "Assets/Scriptable Objects/Abilities/Non-attacks/" + _name + ".asset");
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
            AssetDatabase.CreateAsset(_statusEffect, "Assets/Scriptable Objects/StatusEffects/DOT/" + _name + ".asset");
            AssetDatabase.SaveAssets();
            
            _statusEffect = CreateInstance<DamageOverTime>();
        }
    }
    
    public class CreateNewStatChangeEffect
    {
        [UsedImplicitly] public string _name;
        [UsedImplicitly] public string _buffOrDebuff;

        public CreateNewStatChangeEffect()
        {
            _statusEffect = CreateInstance<StatChange>();
            _statusEffect.name = _name;
            _buffOrDebuff = _statusEffect.buffs ? "Buffs" : "Debuffs";
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public StatChange _statusEffect;

        [Button("Add New Stat Change Effect")]
        private void CreateNewData()
        {
            AssetDatabase.CreateAsset(_statusEffect, $"Assets/Scriptable Objects/StatusEffects/StatChange/{_buffOrDebuff}/" + _name + ".asset");
            AssetDatabase.SaveAssets();
            
            _statusEffect = CreateInstance<StatChange>();
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
            AssetDatabase.CreateAsset(_elementalType, "Assets/Scriptable Objects/ElementalTypes/" + _name + ".asset");
            AssetDatabase.SaveAssets();
            
            _elementalType = CreateInstance<ElementalType>();
        }
    }

    public class CreateNewClass
    {
        [UsedImplicitly] public string _name;

        public CreateNewClass()
        {
            _class = CreateInstance<Class>();
            _class.name = _name;
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public Class _class;

        [Button("Add New Class")]
        private void CreateNewData()
        {
            AssetDatabase.CreateAsset(_class, $"Assets/Scriptable Objects/Classes/{_class.partyMember.characterName}/" + _name + ".asset");
            AssetDatabase.SaveAssets();
            
            _class = CreateInstance<Class>();
        }
    }
    
    public class CreateNewWeapon
    {
        [UsedImplicitly] public string _name;

        public CreateNewWeapon()
        {
            _weaponItem = CreateInstance<WeaponItem>();
            _weaponItem.name = _name;
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public WeaponItem _weaponItem;

        [Button("Add New Weapon")]
        private void CreateNewData()
        {
            AssetDatabase.CreateAsset(_weaponItem, $"Assets/Scriptable Objects/Weapons/" + _name + ".asset");
            AssetDatabase.SaveAssets();
            
            _weaponItem = CreateInstance<WeaponItem>();
        }
    }
    
    #endregion
}
