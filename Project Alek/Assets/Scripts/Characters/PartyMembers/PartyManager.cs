using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters.PartyMembers
{
    public class PartyManager : MonoBehaviour
    {
        public List<PartyMember> partyMembers = new List<PartyMember>();

        public static PartyManager _instance;

        private void Awake()
        {
            if (_instance == null) {
                DontDestroyOnLoad(gameObject);
                _instance = this;
            }
            
            else if (_instance != this) Destroy(gameObject);
            
            partyMembers = partyMembers.OrderBy(i => i.positionInParty).ToList();
        }
    }
}
