using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Global Variables")]
public class GlobalVariables : ScriptableObject
{
    public bool showDamageBoostIncrements;
    [ShowIf(nameof(showDamageBoostIncrements))]
    [Range(0.1f,0.5f)] public float damageBoostIncrement = 0.3f;
    [ShowIf(nameof(showDamageBoostIncrements))]
    [Range(0.1f,0.5f)] public float defenseBoostIncrement = 0.3f;
    
    [Space(10)] [Range(0,6)] public int maxLoanAmount = 4;

    [Space(10)] [Range(1.0f, 2.0f)] public float criticalDamageFactor = 1.25f;
    [Space(10)] [Range(0,1)] public float healthItemChance = 0.40f;
    [Range(3,7)] public int maxHealthItemAmount;

    [Space(10)] public bool showConversionFactors;
    [ShowIf(nameof(showConversionFactors))]
    [Range(1,3)] public float conversionFactorLvl1 = 1.2f;
    [ShowIf(nameof(showConversionFactors))]
    [Range(1,3)] public float conversionFactorLvl2 = 1.4f;
    [ShowIf(nameof(showConversionFactors))]
    [Range(1,3)] public float conversionFactorLvl3 = 1.8f;
    [ShowIf(nameof(showConversionFactors))]
    [Range(1,3)] public float conversionFactorLvl4 = 2.25f;

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

    public bool showAPColors;
    [Space(10)] [ShowIf(nameof(showAPColors))] public Color originalAPColor;
    [ShowIf(nameof(showAPColors))] public Color overexertedAPColor;
}