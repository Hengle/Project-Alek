using System.Collections.Generic;
using System.Linq;
using Characters.PartyMembers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SingletonScriptableObject
{
    [CreateAssetMenu(fileName = "Party Manager")]
    public class PartyManager : SingletonScriptableObject<PartyManager>
    {
        [InlineEditor(InlineEditorModes.FullEditor)]
        public List<PartyMember> partyMembers = new List<PartyMember>();
        
        public void Order() => partyMembers = partyMembers.OrderBy
            (i => i.positionInParty).ToList();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() => Init();
    }
}