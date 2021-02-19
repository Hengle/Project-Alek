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
        
        private void Start()
        {
            if (useProfile && !GameObject.Find(profile.source.gameObject.name))
                InstantiateProfileAudioSource();
            else if (!useProfile) track.source.transform.SetParent(AudioController.Instance.transform);
            
            AudioController.Instance.AddNewTrack(useProfile ? profile.track : track);
        }

        private void InstantiateProfileAudioSource()
        {
            var source = Instantiate(profile.source, AudioController.Instance.transform);
            source.name = profile.source.gameObject.name;
            profile.track.source = source;
        }

        private void OnDestroy()
        {
            if (useProfile) return;
            AudioController.Instance.RemoveTrack(track);
        }

        [UsedImplicitly] public void PlayAudio(int index) => AudioController.Instance.PlayAudio
            (useProfile ? profile.track.audio[index].type : track.audio[index].type);
    }
}