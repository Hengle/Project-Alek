﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BattleSystem.DamagePrefab
{
    public class DamagePrefabManager : MonoBehaviour
    {
        public static DamagePrefabManager Instance
        {
            get
            {
                if (instance == null) 
                    Debug.LogError("DamagePrefabManager is null");
                
                return instance;
            }
        }

        private static DamagePrefabManager instance;

        [SerializeField] private GameObject damagePrefab;
        [SerializeField] private GameObject prefabParent;

        [SerializeField] private List<GameObject> damagePool = new List<GameObject>();
        private readonly List<TextMeshPro> damageTextList = new List<TextMeshPro>();

        [SerializeField] private Color damageTextColor;
        [SerializeField] private Color criticalTextColor;

        private void Awake() => instance = this;

        private void Start() => damagePool = GeneratePrefabs(15);

        private List<GameObject> GeneratePrefabs(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var damageGO = Instantiate(damagePrefab, prefabParent.transform, true);
                damageGO.SetActive(false);
                
                damagePool.Add(damageGO);
                damageTextList.Add(damageGO.GetComponentInChildren<TextMeshPro>());
                damageTextList[i].color = damageTextColor;
            }

            return damagePool;
        }

        public GameObject ShowDamage(int dmg, bool isCrit)
        {
            for (var i = 0; i < damagePool.Count; i++)
            {
                if (damagePool[i].activeInHierarchy) continue;
                
                damageTextList[i].color = isCrit ? criticalTextColor : damageTextColor;
                damageTextList[i].text = dmg.ToString();
                damageTextColor = Color.white;
                damagePool[i].SetActive(true);
                    
                return damagePool[i];
            }
            
            // This should only be reached if the number of damage prefabs passes 15 at once, which is ridiculous;
            damageTextList[0].color = isCrit ? criticalTextColor : damageTextColor;
            damageTextList[0].text = dmg.ToString();
            damageTextColor = Color.white;
            damagePool[0].SetActive(true);
            
            return damagePool[0];
        }

        public void SetColor(Color colour) => damageTextColor = colour;
    }
}
