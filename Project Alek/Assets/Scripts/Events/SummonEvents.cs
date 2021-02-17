using Characters.PartyMembers;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace Events
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "SummonEvent.asset", menuName = SOArchitecture_Utility.GAME_EVENT + "Summon Event")]
    public class SummonEvents : GameEventBase<SummonEvent>
    {
    }
}