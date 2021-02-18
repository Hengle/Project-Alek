using Sirenix.OdinInspector;
using UnityEngine;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Global Variables", menuName = "Singleton SO/Global Variables")]
    public class GlobalVariables : ScriptableObjectSingleton<GlobalVariables>
    {
        #region BoostIncrements

        [TabGroup("Tabs/Battle System","Boost System")]
        [Range(0.1f,0.5f)] public float damageBoostIncrement = 0.3f;
        [TabGroup("Tabs/Battle System","Boost System")]
        [Range(0.1f,0.5f)] public float defenseBoostIncrement = 0.3f;

        #endregion

        #region Loan

        [TabGroup("Tabs/Battle System","Overexertion System")]
        [Range(0,6)] public int maxLoanAmount = 4;
    
        #endregion

        #region CriticalDamageValue
        
        [TabGroup("Tabs/Battle System","Critical Damage")]
        [Range(1.0f, 2.0f)] public float criticalDamageFactor = 1.25f;
    
        #endregion

        #region BattleItemValues
        
        [TabGroup("Tabs/Battle System","Battle Item Values")]
        [Range(0,1)] public float healthItemChance = 0.40f;
        [TabGroup("Tabs/Battle System","Battle Item Values")]
        [Range(3,7)] public int maxHealthItemAmount;
    
        #endregion

        #region ConversionValues
        
        [TabGroup("Tabs/Battle System","AP Conversion System")]
        [Range(1,3)] public float conversionFactorLvl1 = 1.2f;
        [TabGroup("Tabs/Battle System","AP Conversion System")]
        [Range(1,3)] public float conversionFactorLvl2 = 1.4f;
        [TabGroup("Tabs/Battle System","AP Conversion System")]
        [Range(1,3)] public float conversionFactorLvl3 = 1.8f;
        [TabGroup("Tabs/Battle System","AP Conversion System")]
        [Range(1,3)] public float conversionFactorLvl4 = 2.25f;
    
        #endregion

        #region BuffAndDebuffValues
        
        [TabGroup("Tabs/Battle System","Buff & Debuff System")]
        [Range(0,1)] public float slightBuff = 0.05f;
        [TabGroup("Tabs/Battle System","Buff & Debuff System")]
        [Range(-1,0)] public float slightDebuff = -0.05f;

        [Space] [TabGroup("Tabs/Battle System","Buff & Debuff System")]
        [Range(0,1)] public float moderateBuff = 0.10f;
        [TabGroup("Tabs/Battle System","Buff & Debuff System")]
        [Range(-1,0)] public float moderateDebuff = -0.10f;

        [Space] [TabGroup("Tabs/Battle System","Buff & Debuff System")]
        [Range(0,1)] public float significantBuff = 0.20f;
        [TabGroup("Tabs/Battle System","Buff & Debuff System")]
        [Range(-1,0)] public float significantDebuff = -0.20f;
    
        #endregion

        #region TimedButtonValues
        
        [TabGroup("Tabs/Battle System","Button Timing System")]
        [Range(1,1.5f)] public float timedAttackBonus = 1.10f;
        [TabGroup("Tabs/Battle System","Button Timing System")]
        [Range(0.5f,1)] public float timedDefenseBonus = 0.90f;
    
        #endregion

        #region Colors
        
        [TabGroup("Tabs", "Colors")] public Color originalAPColor;
        [TabGroup("Tabs", "Colors")] public Color overexertedAPColor;
    
        #endregion

        #region Objects
        
        [TabGroup("Tabs", "Other")] public Sprite mysteryIcon;
        [TabGroup("Tabs", "Other")] public Material flashMaterial;
    
        #endregion
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}