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
            AudioController.Instance.PlayAudio(themes[1]);
        }
    }
}