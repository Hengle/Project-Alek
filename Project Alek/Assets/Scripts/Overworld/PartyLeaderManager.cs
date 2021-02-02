using System;
using System.Collections.Generic;
using Characters.PartyMembers;
using SingletonScriptableObject;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Overworld
{
    public class PartyLeaderManager : MonoBehaviour
    {
        private Animator animator;
        private Controls controls;
        private static List<PartyMember> Members => PartyManager.Instance.partyMembers;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            controls = new Controls();
            controls.Overworld.ChangeLeader.performed += ChangePartyLeader;
        }

        private void ChangePartyLeader(InputAction.CallbackContext ctx)
        {
            if (Members.Count == 1) return;
            
            var value = ctx.ReadValue<float>();
            var currentLeader = PartyManager.Instance.currentLeader;
            PartyMember nextLeader;

            if (Math.Abs(value - 1) < 0.001)
            {
                var index = Members.IndexOf(currentLeader);
                if (index+1 < Members.Count)
                {
                    nextLeader = Members[index + 1];
                    currentLeader.positionInParty = nextLeader.positionInParty;
                    nextLeader.positionInParty = 1;
                    PartyManager.Instance.currentLeader = nextLeader;
                    animator.runtimeAnimatorController = PartyManager.Instance.currentLeader.overworldController;
                }
                else
                {
                    nextLeader = Members[0];
                    currentLeader.positionInParty = nextLeader.positionInParty;
                    nextLeader.positionInParty = 1;
                    PartyManager.Instance.currentLeader = nextLeader;
                    animator.runtimeAnimatorController = PartyManager.Instance.currentLeader.overworldController;
                }
            }
            else if (Math.Abs(value - (-1)) < 0.001)
            {
                var index = Members.IndexOf(currentLeader);
                if (index-1 >= 0)
                {
                    nextLeader = Members[index - 1];
                    currentLeader.positionInParty = nextLeader.positionInParty;
                    nextLeader.positionInParty = 1;
                    PartyManager.Instance.currentLeader = nextLeader;
                    animator.runtimeAnimatorController = PartyManager.Instance.currentLeader.overworldController;
                }
                else
                {
                    nextLeader = Members[Members.Count-1];
                    currentLeader.positionInParty = nextLeader.positionInParty;
                    nextLeader.positionInParty = 1;
                    PartyManager.Instance.currentLeader = nextLeader;
                    PartyManager.Instance.currentLeader = nextLeader;
                    animator.runtimeAnimatorController = PartyManager.Instance.currentLeader.overworldController;
                }
            }
            
        }
        
        private void OnEnable() => controls.Enable();
        private void OnDisable() => controls.Disable();
    }
}