using Sirenix.OdinInspector;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "Audio User Profile", menuName = "Audio/Audio User Profile")]
    public class AudioUserProfile : ScriptableObject
    {
        [InfoBox("Place the source here instead of in the track")]
        [SerializeField] public AudioSource source;
        [SerializeField] public AudioController.AudioTrack track;
    }
}