using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Audio
{
    //TODO: Don't use this in animations functions, call from code directly
    public class SoundEffects : MonoBehaviour
    {
        public List<AudioClip> audioClips = new List<AudioClip>();
        private AudioSource audioSource;
        private void Awake() => audioSource = gameObject.AddComponent<AudioSource>();

        [UsedImplicitly] public void PlayAudioClip(int id)
        {
            if (audioClips.Contains(audioClips[id])) audioSource.PlayOneShot(audioClips[id]);
        }
    }
}