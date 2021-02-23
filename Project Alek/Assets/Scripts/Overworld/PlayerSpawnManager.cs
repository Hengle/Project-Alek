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

        private void Awake()
        {
            PartyManager.Order();
            var member = PartyManager.Members.Single(m => m.positionInParty == 1);
            
            GameObject memberGo;
            
            if (PlayerPositionManager.Position != Vector3.zero)
            {
                memberGo = Instantiate(playerPrefab, PlayerPositionManager.Position, playerPrefab.transform.rotation);
                memberGo.GetComponent<Animator>().runtimeAnimatorController = member.overworldController;
                PlayerPositionManager.Position = Vector3.zero;
                vCam.Follow = memberGo.transform;
            }
            
            else
            {
                var playerSpawners = FindObjectsOfType<PlayerSpawnArea>();
                var spawnPoint = playerSpawners.SingleOrDefault(s =>
                    s.Id == PlayerPositionManager.SpawnId)?.transform;

                if (spawnPoint != null)
                {
                    memberGo = Instantiate(playerPrefab, spawnPoint.position, playerPrefab.transform.rotation);
                    memberGo.GetComponent<Animator>().runtimeAnimatorController = member.overworldController;
                    vCam.Follow = memberGo.transform;
                }
                else Debug.LogError("Could not locate a spawn point with the specified ID!");
            }

            playerSpawnEvent.Raise();
        }
    }
}