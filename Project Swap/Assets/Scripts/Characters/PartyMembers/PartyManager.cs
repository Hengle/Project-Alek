using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.PartyMembers
{
    public class PartyManager : MonoBehaviour
    {
        // Current list of all party members
        [FormerlySerializedAs("PartyMembers")] public List<PartyMember> partyMembers = new List<PartyMember>();

        public static PartyManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            // Sort the list by the order the party is currently in
            partyMembers = partyMembers.OrderBy(i => i.positionInParty).ToList();
        }
    }
}
