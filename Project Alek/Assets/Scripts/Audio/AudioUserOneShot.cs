
namespace Audio
{
    public class AudioUserOneShot : AudioUser
    {
        private void OnDestroy()
        {
            AudioController.Instance.RemoveTrack(useProfile ? profile.track : track);
        }
    }
}