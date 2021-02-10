using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "Audio User Profile", menuName = "Audio/Audio User Profile")]
    public class AudioUserProfile : ScriptableObject
    {
        [SerializeField] public AudioSource source;
        [SerializeField] public AudioController.AudioTrack track;
    }
}