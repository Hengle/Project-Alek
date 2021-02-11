using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Audio
{
    public class AudioUser : MonoBehaviour
    {
        [SerializeField] protected bool useProfile;
        
        [SerializeField] [ShowIf(nameof(useProfile))]
        public AudioUserProfile profile;
        
        [SerializeField] [HideIf(nameof(useProfile))]
        public AudioController.AudioTrack track;
        
        protected AudioController _audioController;

        private void Start()
        {
            _audioController = FindObjectOfType<AudioController>();
            
            if (useProfile && !GameObject.Find(profile.source.gameObject.name))
                InstantiateProfileAudioSource();
            
            _audioController.AddNewTrack(useProfile ? profile.track : track);
        }

        private void InstantiateProfileAudioSource()
        {
            var source = Instantiate(profile.source, _audioController.transform);
            source.name = profile.source.gameObject.name;
            profile.track.source = source;
        }
        
        [UsedImplicitly] public void PlayAudio(int index) => _audioController.PlayAudio
            (useProfile ? profile.track.audio[index].type : track.audio[index].type);
    }
}