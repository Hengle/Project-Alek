using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleSystem.UI
{
    public class InfoBoxUI : MonoBehaviour, ISelectHandler
    {
        [TextArea(5,15)] public string  information;
        [HideInInspector] [SerializeField]
        private TextMeshProUGUI infoText;

        private GameObject parent;

        private void Awake()
        {
            infoText = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Info Box")
                .GetComponentInChildren<TextMeshProUGUI>();

            parent = infoText.gameObject.transform.parent.gameObject;
        }

        public void OnSelect(BaseEventData eventData)
        {
            infoText.text = information;
            parent.SetActive(information != "");
        }
    }
}