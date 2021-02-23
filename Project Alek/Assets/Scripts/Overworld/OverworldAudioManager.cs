using Audio;
using UnityEngine;
using AudioType = Audio.AudioType;

namespace Overworld
{
    public class OverworldAudioManager : MonoBehaviour
    {
        [SerializeField] private AudioType[] themes;

        private void Start()
        {
            AudioController.PlayAudio(themes[1]);
        }
    }
}