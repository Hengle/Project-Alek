using Overworld;
using UnityEngine;

namespace ScriptableObjectArchitecture
{
    [System.Serializable]
    [CreateAssetMenu(
        fileName = "SpawnEvent.asset",
        menuName = SOArchitecture_Utility.GAME_EVENT + "Spawn Event")]
    public class SpawnEvent : GameEventBase<Spawner>
    {
    }
}