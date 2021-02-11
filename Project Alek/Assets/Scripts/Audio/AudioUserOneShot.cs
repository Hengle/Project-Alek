
namespace Audio
{
    public class AudioUserOneShot : AudioUser
    {
        private void OnDestroy()
        {
            _audioController.RemoveTrack(useProfile ? profile.track : track);
        }
    }
}