using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace DamagePrefab
{
    public class DamagePrefabManager : MonoBehaviorSingleton<DamagePrefabManager>
    {
        private readonly List<TextMeshPro> damageTextList = new List<TextMeshPro>();
        
        [SerializeField] private int poolCount = 15;

        [SerializeField] [ReadOnly]
        private List<GameObject> damagePool = new List<GameObject>();

        [SerializeField] private GameObject damagePrefab;
        [SerializeField] private GameObject prefabParent;
        
        [SerializeField] private Color damageTextColor;
        [SerializeField] private Color criticalTextColor;

        [SerializeField] private float normalFontSize;
        
        public Color DamageTextColor { set => damageTextColor = value; }
        
        public Queue<GameObject> activeObjects = new Queue<GameObject>();

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

            normalFontSize = damageTextList[0].fontSize;
            return damagePool;
        }

        public GameObject ShowDamage(int dmg, bool isCrit, Vector3 position)
        {
            var stackOffset = activeObjects.Count + 0.5f;
            
            for (var i = 0; i < damagePool.Count; i++)
            {
                if (damagePool[i].activeInHierarchy) continue;

                damageTextList[i].color = isCrit ? criticalTextColor : damageTextColor;
                damageTextList[i].text = dmg != -1 ? dmg.ToString() : "MISS";
                damageTextList[i].fontSize = normalFontSize;
                damageTextColor = Color.white;
                damagePool[i].transform.position = new Vector3(position.x, position.y + stackOffset, position.z);
                damagePool[i].SetActive(true);
                    
                return damagePool[i];
            }
            
            damageTextList[0].color = isCrit ? criticalTextColor : damageTextColor;
            damageTextList[0].text = dmg.ToString();
            damageTextColor = Color.white;
            damagePool[0].transform.position = new Vector3(position.x, position.y + stackOffset, position.z);
            damagePool[0].SetActive(true);
            
            return damagePool[0];
        }

        public GameObject ShowText(string message, Vector3 position, float fontSize = 0f)
        {
            var stackOffset = activeObjects.Count + 0.5f;
            
            for (var i = 0; i < damagePool.Count; i++)
            {
                if (damagePool[i].activeInHierarchy) continue;

                damageTextList[i].color = damageTextColor;
                damageTextList[i].text = message;
                damageTextList[i].fontSize = Math.Abs(fontSize) > 0.1f ? fontSize : normalFontSize;
                damageTextColor = Color.white;
                damagePool[i].transform.position = new Vector3(position.x, position.y + stackOffset, position.z);
                damagePool[i].SetActive(true);
                    
                return damagePool[i];
            }
            
            damageTextList[0].color = damageTextColor;
            damageTextList[0].text = message;
            damageTextColor = Color.white;
            damagePool[0].transform.position = new Vector3(position.x, position.y + stackOffset, position.z);
            damagePool[0].SetActive(true);
            
            return damagePool[0];
        }
    }
}
