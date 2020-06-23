using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BattleSystem
{
    public class InfoBoxScript : MonoBehaviour, ISelectHandler
    {
        [TextArea(5,15)] public string  information;
        [SerializeField] private TextMeshProUGUI infoText;

        private void Awake()
        {
            infoText = GameObject.FindGameObjectWithTag("Canvas").transform.GetChild(3).GetComponentInChildren<TextMeshProUGUI>();
        }

        public void OnSelect(BaseEventData eventData) => infoText.text = information;
    }
}