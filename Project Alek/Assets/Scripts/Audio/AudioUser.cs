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
    }
}