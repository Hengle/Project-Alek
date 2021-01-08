﻿using UnityEngine;

[CreateAssetMenu(fileName = "Global Variables")]
public class GlobalVariables : ScriptableObject
{
    [Range(0.1f,0.5f)] public float damageBoostIncrement = 0.3f;
    [Range(0.1f,0.5f)] public float defenseBoostIncrement = 0.3f;

    public int maxLoanAmount = 4;

    [Range(1.0f, 2.0f)] public float criticalDamageFactor = 1.25f;
    [Range(0,1)] public float healthItemChance = 0.40f;
    [Range(3,7)] public int maxHealthItemAmount;
        
    [Range(1,3)] public float conversionFactorLvl1 = 1.2f;
    [Range(1,3)] public float conversionFactorLvl2 = 1.4f;
    [Range(1,3)] public float conversionFactorLvl3 = 1.8f;
    [Range(1,3)] public float conversionFactorLvl4 = 2.25f;
    
    public const float SlightBuff = 0.05f;
    public const float SlightDebuff = -0.05f;

    public const float ModerateBuff = 0.10f;
    public const float ModerateDebuff = -0.10f;

    public const float SignificantBuff = 0.20f;
    public const float SignificantDebuff = -0.20f;
    
    public Color originalAPColor;
    public Color overexertedAPColor;

    public AudioClip battleMusic;
}