
namespace Audio
{
    public class AudioUserOneShot : AudioUser
    {
        private void OnDestroy()
        {
            AudioController.RemoveTrack(useProfile ? profile.track : track);
        }
    }
}