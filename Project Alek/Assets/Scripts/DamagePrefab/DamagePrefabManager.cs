using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace DamagePrefab
{
    public class DamagePrefabManager : MonoBehaviour
    {
        private static DamagePrefabManager instance;

        private readonly List<TextMeshPro> damageTextList = new List<TextMeshPro>();
        
        [SerializeField] private int poolCount = 15;

        [SerializeField] [ReadOnly]
        private List<GameObject> damagePool = new List<GameObject>();

        [SerializeField] private GameObject damagePrefab;
        [SerializeField] private GameObject prefabParent;
        
        [SerializeField] private Color damageTextColor;
        [SerializeField] private Color criticalTextColor;

        public static DamagePrefabManager Instance {
            get { if (instance == null) 
                    Debug.LogError("DamagePrefabManager is null");
                return instance; }
        }

        public Color DamageTextColor { set => damageTextColor = value; }

        private void Awake() => instance = this;

        private void Start() => damagePool = GeneratePrefabs(poolCount);

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
                damageTextList[i].text = dmg != -1 ? dmg.ToString() : "MISS";
                damageTextColor = Color.white;
                damagePool[i].SetActive(true);
                    
                return damagePool[i];
            }
            
            damageTextList[0].color = isCrit ? criticalTextColor : damageTextColor;
            damageTextList[0].text = dmg.ToString();
            damageTextColor = Color.white;
            damagePool[0].SetActive(true);
            
            return damagePool[0];
        }
    }
}
