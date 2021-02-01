using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Global Variables")]
public class GlobalVariables : SingletonScriptableObject<GlobalVariables>
{
    #region BoostIncrements
    
    [Space(10)] public bool showBoostIncrements;
    [ShowIf(nameof(showBoostIncrements))]
    [Range(0.1f,0.5f)] public float damageBoostIncrement = 0.3f;
    [ShowIf(nameof(showBoostIncrements))]
    [Range(0.1f,0.5f)] public float defenseBoostIncrement = 0.3f;

    #endregion

    #region Loan
    
    [Space(10)] public bool showMaxLoanAmount;
    [ShowIf(nameof(showMaxLoanAmount))]
    [Range(0,6)] public int maxLoanAmount = 4;
    
    #endregion

    #region CriticalDamageValue
    
    [Space(10)] public bool showCritDamageFactor;
    [ShowIf(nameof(showCritDamageFactor))]
    [Range(1.0f, 2.0f)] public float criticalDamageFactor = 1.25f;
    
    #endregion

    #region BattleItemValues
    
    [Space(10)] public bool showBattleItemValues;
    [ShowIf(nameof(showBattleItemValues))] 
    [Range(0,1)] public float healthItemChance = 0.40f;
    [ShowIf(nameof(showBattleItemValues))]
    [Range(3,7)] public int maxHealthItemAmount;
    
    #endregion

    #region ConversionValues
    
    [Space(10)] public bool showConversionFactors;
    [ShowIf(nameof(showConversionFactors))]
    [Range(1,3)] public float conversionFactorLvl1 = 1.2f;
    [ShowIf(nameof(showConversionFactors))]
    [Range(1,3)] public float conversionFactorLvl2 = 1.4f;
    [ShowIf(nameof(showConversionFactors))]
    [Range(1,3)] public float conversionFactorLvl3 = 1.8f;
    [ShowIf(nameof(showConversionFactors))]
    [Range(1,3)] public float conversionFactorLvl4 = 2.25f;
    
    #endregion

    #region BuffAndDebuffValues

    [Space(10)] public bool showBuffAndDebuffValues;
    [ShowIf(nameof(showBuffAndDebuffValues))]
    [Range(0,1)] public float slightBuff = 0.05f;
    [ShowIf(nameof(showBuffAndDebuffValues))]
    [Range(-1,0)] public float slightDebuff = -0.05f;

    [Space] [ShowIf(nameof(showBuffAndDebuffValues))]
    [Range(0,1)] public float moderateBuff = 0.10f;
    [ShowIf(nameof(showBuffAndDebuffValues))]
    [Range(-1,0)] public float moderateDebuff = -0.10f;

    [Space] [ShowIf(nameof(showBuffAndDebuffValues))]
    [Range(0,1)] public float significantBuff = 0.20f;
    [ShowIf(nameof(showBuffAndDebuffValues))]
    [Range(-1,0)] public float significantDebuff = -0.20f;
    
    #endregion

    #region TimedButtonValues
    
    [Space(10)] public bool showTimedAttackAndDefenseValues;
    [ShowIf(nameof(showTimedAttackAndDefenseValues))]
    [Range(1,1.5f)] public float timedAttackBonus = 1.10f;
    [ShowIf(nameof(showTimedAttackAndDefenseValues))]
    [Range(0.5f,1)] public float timedDefenseBonus = 0.90f;
    
    #endregion

    #region Colors

    [Space(10)] public bool showAPColors;
    [ShowIf(nameof(showAPColors))] public Color originalAPColor;
    [ShowIf(nameof(showAPColors))] public Color overexertedAPColor;
    
    #endregion

    #region Objects

    [Space(10)] public bool showMysteryIcon;
    [ShowIf(nameof(showMysteryIcon))] public Sprite mysteryIcon;
    
    #endregion
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize() => Init();
}