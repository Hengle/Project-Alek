using System.Linq;
using Cinemachine;
using ScriptableObjectArchitecture;
using SingletonScriptableObject;
using UnityEngine;

namespace Overworld
{
    public class PlayerSpawnManager : MonoBehaviour
    {
        [SerializeField] private GameEvent playerSpawnEvent;
        [SerializeField] private CinemachineVirtualCamera vCam;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerTransform;
        
        private void Awake()
        {
            PartyManager.Instance.Order();
            var member = PartyManager.Instance.partyMembers.Single(m => m.positionInParty == 1);
            
            GameObject memberGo;
            
            if (PlayerPositionManager.Instance.Position != Vector3.zero)
            {
                memberGo = Instantiate(playerPrefab, PlayerPositionManager.Instance.Position, playerPrefab.transform.rotation);
                memberGo.GetComponent<Animator>().runtimeAnimatorController = member.overworldController;
                PlayerPositionManager.Instance.Position = Vector3.zero;
                playerTransform = memberGo.transform;
                vCam.Follow = memberGo.transform;
            }
            
            else
            {
                var playerSpawners = FindObjectsOfType<PlayerSpawnArea>();
                var spawnPoint = playerSpawners.SingleOrDefault(s =>
                    s.Id == PlayerPositionManager.Instance.SpawnId)?.transform;

                if (spawnPoint != null)
                {
                    memberGo = Instantiate(playerPrefab, spawnPoint.position, playerPrefab.transform.rotation);
                    memberGo.GetComponent<Animator>().runtimeAnimatorController = member.overworldController;
                    playerTransform = memberGo.transform;
                    vCam.Follow = memberGo.transform;
                }
                else Debug.LogError("Could not locate a spawn point with the specified ID!");
            }

            playerSpawnEvent.Raise();
        }

        private void OnDisable()
        {
            if (Time.timeScale < 1) return;
            PlayerPositionManager.Instance.Position = playerTransform.position;
        }
    }
}