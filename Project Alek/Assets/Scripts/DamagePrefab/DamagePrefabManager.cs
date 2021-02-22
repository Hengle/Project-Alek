using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
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
        
        [SerializeField] private List<KeyValuePair<Transform, Queue<GameObject>>> battleUnits =
            new List<KeyValuePair<Transform, Queue<GameObject>>>();
        
        public Color DamageTextColor { set => damageTextColor = value; }

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

        private KeyValuePair<Transform, Queue<GameObject>> GetKeyValuePair(Transform battleUnit = null, GameObject prefab = null)
        {
            return prefab ? battleUnits.SingleOrDefault(u => u.Value.Contains(prefab)) :
                battleUnits.SingleOrDefault(u => u.Key == battleUnit);
        }

        public void RegisterTransforms(List<Transform> transforms)
        {
            foreach (var obj in transforms)
            {
                battleUnits.Add(new KeyValuePair<Transform, Queue<GameObject>>(obj, new Queue<GameObject>()));
            }
        }

        public GameObject ShowDamage(int dmg, bool isCrit, Vector3 position, Transform battleUnit)
        {
            var count = GetKeyValuePair(battleUnit).Value.Count;
            var stackOffset = count > 0 ? count * 0.5f : 0;
            
            for (var i = 0; i < damagePool.Count; i++)
            {
                if (damagePool[i].activeInHierarchy) continue;

                damageTextList[i].color = isCrit ? criticalTextColor : damageTextColor;
                damageTextList[i].text = dmg != -1 ? dmg.ToString() : "MISS";
                damageTextList[i].fontSize = normalFontSize;
                damageTextColor = Color.white;
                damagePool[i].transform.position = new Vector3(position.x, position.y + stackOffset, position.z);
                
                Enqueue(damagePool[i], battleUnit);
                damagePool[i].SetActive(true);
                    
                return damagePool[i];
            }
            
            damageTextList[0].color = isCrit ? criticalTextColor : damageTextColor;
            damageTextList[0].text = dmg.ToString();
            damageTextColor = Color.white;
            damagePool[0].transform.position = new Vector3(position.x, position.y + stackOffset, position.z);
            
            Enqueue(damagePool[0], battleUnit);
            damagePool[0].SetActive(true);
            
            return damagePool[0];
        }

        public GameObject ShowText(string message, Vector3 position, Transform battleUnit, float fontSize = 0f)
        {
            var count = GetKeyValuePair(battleUnit).Value.Count;
            var stackOffset = count > 0 ? count * 0.5f : 0;
            
            for (var i = 0; i < damagePool.Count; i++)
            {
                if (damagePool[i].activeInHierarchy) continue;

                damageTextList[i].color = damageTextColor;
                damageTextList[i].text = message;
                damageTextList[i].fontSize = Math.Abs(fontSize) > 0.1f ? fontSize : normalFontSize;
                damageTextColor = Color.white;
                damagePool[i].transform.position = new Vector3(position.x, position.y + stackOffset, position.z);
                
                Enqueue(damagePool[i], battleUnit);
                damagePool[i].SetActive(true);
                    
                return damagePool[i];
            }
            
            damageTextList[0].color = damageTextColor;
            damageTextList[0].text = message;
            damageTextColor = Color.white;
            damagePool[0].transform.position = new Vector3(position.x, position.y + stackOffset, position.z);
            
            Enqueue(damagePool[0], battleUnit);
            damagePool[0].SetActive(true);
            
            return damagePool[0];
        }

        private void Enqueue(GameObject prefab, Transform battleUnit)
        {
            var pair = GetKeyValuePair(battleUnit);
            if (pair.Key) pair.Value.Enqueue(prefab);
        }

        public void Dequeue(GameObject prefab)
        {
            var pair = GetKeyValuePair(prefab: prefab);
            if (pair.Key) pair.Value.Dequeue();
        }
    }
}
