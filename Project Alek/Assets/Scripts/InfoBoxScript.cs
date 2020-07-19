using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class InfoBoxScript : MonoBehaviour, ISelectHandler
{
    [TextArea(5,15)] public string  information;
    [HideInInspector] [SerializeField]
    private TextMeshProUGUI infoText;

    private void Awake() => infoText = GameObject.FindGameObjectWithTag("Canvas").transform.
        Find("Info Box").GetComponentInChildren<TextMeshProUGUI>();

    public void OnSelect(BaseEventData eventData) => infoText.text = information;
}