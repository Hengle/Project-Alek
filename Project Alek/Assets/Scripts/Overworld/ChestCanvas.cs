using System;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.InventoryEngine;
using SingletonScriptableObject;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;

namespace Overworld
{
    public class ChestCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private List<TextMeshProUGUI> itemsText;

        private static ChestCanvas instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this) Destroy(gameObject);
        }

        public void Activate(List<InventoryItem> items)
        {
            itemsText.ForEach(i => i.gameObject.SetActive(false));
            var uniqueItems = items.Distinct().ToList();

            for (var i = 0; i < uniqueItems.Count; i++)
            {
                var count = items.Count(item => item == uniqueItems[i]);
                itemsText[i].text = $"{uniqueItems[i].ItemName} x{count}";
                itemsText[i].gameObject.SetActive(true);
            }
            
            items.ForEach(MainInventory.AddItem);
            panel.SetActive(true);
            SetSelected();
        }

        private void SetSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(panel);
        }
    }
}