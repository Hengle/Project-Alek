using Characters;
using UnityEngine;

namespace ScriptableObjectArchitecture
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "CharacterGameEvent.asset", menuName = SOArchitecture_Utility.GAME_EVENT + "Character Event")]
    public class CharacterGameEvent : GameEventBase<UnitBase, CharacterGameEvent>
    {
    }
}