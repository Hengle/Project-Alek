using JetBrains.Annotations;
using UnityEngine;

namespace Audio
{
    public class AudioUser : MonoBehaviour
    {
        public AudioController.AudioTrack track;
        private AudioController audioController;

        private void Start()
        {
            audioController = FindObjectOfType<AudioController>();
            audioController.AddNewTrack(track);
        }

        [UsedImplicitly] private void PlayAudio(int index) =>
            audioController.PlayAudio(track.audio[index].type);
    }
}