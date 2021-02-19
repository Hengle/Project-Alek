using System.Collections.Generic;
using MEC;
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
        private bool showingNotification;

        private void Awake()
        {
            infoText = GameObject.FindGameObjectWithTag("Canvas").transform.Find("Info Box")
                .GetComponentInChildren<TextMeshProUGUI>();

            parent = infoText.gameObject.transform.parent.gameObject;
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (showingNotification) return;
            infoText.text = information;
            parent.SetActive(information != "");
        }

        public void ShowNotification(string message, float displayTime = 1.5f)
        {
            Timing.RunCoroutine(DisplayNotification(message, displayTime));
        }

        private IEnumerator<float> DisplayNotification(string message, float displayTime)
        {
            showingNotification = true;
            infoText.text = message;
            parent.SetActive(true);
            yield return Timing.WaitForSeconds(displayTime);
            parent.SetActive(false);
            showingNotification = false;
        }
    }
}