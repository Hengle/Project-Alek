using System;
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
        
        private void Awake()
        {
            PartyManager.Instance.Order();
            var member = PartyManager.Instance.partyMembers.Single(m => m.positionInParty == 1);
            GameObject memberGo;
            
            if (PlayerPositionManager.Instance.Position != Vector3.zero)
            {
                memberGo = Instantiate(member.overworldPrefab, PlayerPositionManager.Instance.Position, member.overworldPrefab.transform.rotation);
                PlayerPositionManager.Instance.Position = Vector3.zero;
            }
            
            else
            {
                var playerSpawners = FindObjectsOfType<PlayerSpawnArea>();
                var spawnPoint = playerSpawners.Single
                    (s => s.Id == PlayerPositionManager.Instance.SpawnId).transform;
                
                memberGo = Instantiate(member.overworldPrefab, spawnPoint.position, member.overworldPrefab.transform.rotation);
                //PlayerPositionManager.Instance.SpawnId = "0";
            }

            memberGo.GetComponent<Animator>().runtimeAnimatorController = member.overworldController;
            playerSpawnEvent.Raise();
            vCam.Follow = memberGo.transform;
        }
    }
}