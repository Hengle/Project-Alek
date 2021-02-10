using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Audio
{
    public class AudioUser : MonoBehaviour
    {
        public bool useProfile;
        
        [SerializeField] [ShowIf(nameof(useProfile))]
        public AudioUserProfile profile;
        
        [SerializeField] [HideIf(nameof(useProfile))]
        public AudioController.AudioTrack track;
        
        private AudioController audioController;

        private void Start()
        {
            audioController = FindObjectOfType<AudioController>();
            
            if (useProfile && !GameObject.Find(profile.source.gameObject.name))
                InstantiateProfileAudioSource();
            
            audioController.AddNewTrack(useProfile ? profile.track : track);
        }

        private void InstantiateProfileAudioSource()
        {
            var source = Instantiate(profile.track.source, audioController.transform);
            source.name = profile.source.gameObject.name;
            profile.track.source = source;
        }
        
        [UsedImplicitly] public void PlayAudio(int index) => audioController.PlayAudio
            (useProfile ? profile.track.audio[index].type : track.audio[index].type);
    }
}