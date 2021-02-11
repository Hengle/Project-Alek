using Audio;
using UnityEngine;
using AudioType = Audio.AudioType;

namespace Overworld
{
    public class OverworldAudioManager : MonoBehaviour
    {
        private AudioController audioController;
        [SerializeField] private AudioType[] themes;

        private void Start()
        {
            audioController = FindObjectOfType<AudioController>();
            audioController.PlayAudio(themes[1]);
        }
    }
}