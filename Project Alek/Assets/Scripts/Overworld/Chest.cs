using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.InventoryEngine;
using UnityEngine;
using Audio;

namespace Overworld
{
    public class Chest : MonoBehaviour
    {
        [SerializeField] private GameObject indicator;
        [SerializeField] private List<InventoryItem> content = new List<InventoryItem>();
        [SerializeField] private bool opened;

        private Transform chestTop;
        private ChestCanvas chestContentCanvas;
        private Controls controls;

        private void Awake()
        {
            chestContentCanvas = GameObject.FindWithTag("ChestCanvas").GetComponent<ChestCanvas>();
            chestTop = transform.GetChild(0);
            
            controls = new Controls();
            controls.Enable();
            
            DOTween.Init();
        }

        private void OpenChest()
        {
            opened = true;
            AudioController.PlayAudio(CommonAudioTypes.ChestOpen);
            indicator.SetActive(false);
            controls.Dispose();
            chestTop.DOLocalRotate(new Vector3(120f, 0, 0), 0.75f).
                OnComplete(() => chestContentCanvas.Activate(content));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.transform.CompareTag("Player") || opened) return;
            indicator.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.transform.CompareTag("Player") || opened) return;
            indicator.SetActive(false);
        }

        private void OnTriggerStay(Collider other)
        {
            if (opened) return;
            if (!other.transform.CompareTag("Player")) return;
            if (!TimeManager.AtFullSpeed) return;
            if (controls.UI.Submit.triggered) OpenChest();
        }
    }
}