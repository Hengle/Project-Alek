using JetBrains.Annotations;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Overworld
{
    public class OnDialogueStartAndEnd : MonoBehaviour
    {
        private ProximitySelector proximitySelector;
        private Transform dialogueUI;
        private Transform playerSpeechBubble;
        private Transform npcSpeechBubble;

        private void Awake()
        {
            proximitySelector = GetComponent<ProximitySelector>();
        }

        private void Start()
        {
            dialogueUI = FindObjectOfType<StandardDialogueUI>().transform;
            npcSpeechBubble = dialogueUI.Find("Dialogue Panel").transform.Find("NPC Subtitle Panel").transform;
            playerSpeechBubble = dialogueUI.Find("Dialogue Panel").transform.Find("PC Subtitle Panel (1)").transform;
        }

        [UsedImplicitly] private void OnConversationStart()
        {
            DisableMovement();
            SetSpeechBubblePositions();
        }

        [UsedImplicitly] private void OnConversationEnd()
        {
            EnableMovement();
        }

        private void SetSpeechBubblePositions()
        {
            var targetTransformPos = proximitySelector.CurrentUsable.transform.position;
            var playerTransformPos = transform.position;
            
            var targetOffset = new Vector3(targetTransformPos.x, targetTransformPos.y + 5, targetTransformPos.z);
            var playerOffset = new Vector3(playerTransformPos.x, playerTransformPos.y + 5, playerTransformPos.z);
            
            var worldToScreenPositionTarget = RectTransformUtility.WorldToScreenPoint(Camera.main, targetOffset);
            var worldToScreenPositionPlayer = RectTransformUtility.WorldToScreenPoint(Camera.main, playerOffset);
            
            npcSpeechBubble.position = new Vector3(worldToScreenPositionTarget.x, worldToScreenPositionTarget.y, 0);
            playerSpeechBubble.position = new Vector3(worldToScreenPositionPlayer.x, worldToScreenPositionPlayer.y, 0);
        }

        private static void DisableMovement()
        {
            PlayerMovement._controls.Overworld.Move.Disable();
        }

        private static void EnableMovement()
        {
            PlayerMovement._controls.Overworld.Move.Enable();
        }
    }
}