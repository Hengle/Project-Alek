﻿using System.Collections.Generic;
using System.Linq;
using MoreMountains.InventoryEngine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleSystem
{
    public class HealthItemPrefabManager : MonoBehaviour
    {
        private static HealthItemPrefabManager instance;

        [SerializeField] private int poolCount = 15;
        
        [SerializeField] public List<InventoryItem> healthItems;
        
        [SerializeField] [ReadOnly]
        private List<GameObject> itemPool = new List<GameObject>();
        
        [SerializeField] private GameObject prefabParent;
        
        public static HealthItemPrefabManager Instance {
            get { if (instance == null) 
                    Debug.LogError("HealthItemPrefabManager is null");
                return instance; }
        }
        
        private void Awake() => instance = this;
        
        private void Start() => itemPool = GeneratePrefabs(poolCount);

        private List<GameObject> GeneratePrefabs(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var healthGO = Instantiate(healthItems[0].Prefab, prefabParent.transform, true);
                healthGO.name = healthItems[0].Prefab.name;
                healthGO.SetActive(false);
                
                itemPool.Add(healthGO);
            }
        
            return itemPool;
        }

        public GameObject ShowItem(int id)
        {
            foreach (var t in itemPool.
                Where(t => !t.activeInHierarchy).
                Where(t => t.name == healthItems[id].Prefab.name))
            {
                t.SetActive(true);
                return t;
            }

            return itemPool[0];
        }
    }
}
