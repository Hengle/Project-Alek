using System.Collections.Generic;
using System.Linq;
using Characters.PartyMembers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "Party Manager", menuName = "Singleton SO/Party Manager")]
    public class PartyManager : ScriptableObjectSingleton<PartyManager>
    {
        [InlineEditor(InlineEditorModes.FullEditor)]
        [SerializeField] private List<PartyMember> partyMembers = new List<PartyMember>();

        public static List<PartyMember> Members => Instance.partyMembers;

        [SerializeField] private PartyMember currentLeader;

        public static PartyMember CurrentLeader
        {
            get => Instance.currentLeader;
            set => Instance.currentLeader = value;
        }

        public static void Order()
        {
            Instance.partyMembers = Instance.partyMembers.OrderBy(i => i.positionInParty).ToList();
            for (var i = 0; i < Instance.partyMembers.Count; i++)
            {
                if (Instance.partyMembers[i].positionInParty - 1 != i)
                    Instance.partyMembers[i].positionInParty = i + 1;
            }

            Instance.currentLeader = Instance.partyMembers[0];
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}