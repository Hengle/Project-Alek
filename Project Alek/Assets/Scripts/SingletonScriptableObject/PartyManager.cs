using System.Collections.Generic;
using System.Linq;
using Characters.PartyMembers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Party Manager", menuName = "Singleton SO/Party Manager")]
    public class PartyManager : SingletonScriptableObject<PartyManager>
    {
        [InlineEditor(InlineEditorModes.FullEditor)]
        public List<PartyMember> partyMembers = new List<PartyMember>();

        public PartyMember currentLeader;

        public void Order()
        {
            partyMembers = partyMembers.OrderBy(i => i.positionInParty).ToList();
            for (var i = 0; i < partyMembers.Count; i++)
            {
                if (partyMembers[i].positionInParty - 1 != i)
                    partyMembers[i].positionInParty = i + 1;
            }

            currentLeader = partyMembers[0];
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}