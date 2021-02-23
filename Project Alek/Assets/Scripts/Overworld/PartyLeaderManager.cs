using System;
using System.Collections.Generic;
using Characters.Animations;
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
        private static List<PartyMember> Members => PartyManager.Members;

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
            var currentLeader = PartyManager.CurrentLeader;
            var index = Members.IndexOf(currentLeader);

            var getNextMember = Math.Abs(value - 1) < 0.001;
            var getPrevMember = Math.Abs(value - -1) < 0.001;
            
            var nextLeader = currentLeader;

            if (getNextMember) nextLeader = Members[index + 1 < Members.Count ? index + 1 : 0];
            else if (getPrevMember) nextLeader = Members[index - 1 >= 0 ? index - 1 : Members.Count - 1];

            currentLeader.positionInParty = nextLeader.positionInParty;
            nextLeader.positionInParty = 1;
                
            PartyManager.CurrentLeader = nextLeader;
            animator.runtimeAnimatorController = PartyManager.CurrentLeader.overworldController;
            animator.SetInteger(AnimationHandler.AnimState, 0);
            
        }
        
        private void OnEnable() => controls.Enable();
        private void OnDisable() => controls.Disable();
    }
}