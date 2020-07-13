using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        // this will be overhauled
        public static AudioManager Instance;

        //A list of audioclips
        public List<AudioInfo> AudioClips = new List<AudioInfo>();

        Coroutine musicRoutine;
        void Awake() { if (Instance == null) { Instance = this; } }

        [System.Serializable]
        public class AudioInfo
        {
            public int id;
            public AudioClip clip;
        }

        /*
    //Getting audio clip by id
    public AudioClip findAudioClipById(int id)
    {
        foreach (AudioInfo info in AudioManager.Instance.AudioClips)
        {
            if (info.id == id)
            {
                return info.clip;
            }
        }

        return null;
    }

    Coroutine audioTransitionRoutine;

    //Changing current audio
    public void changeAudio(int audioSourceIndex, int audioId)
    {
        if (audioTransitionRoutine != null)
        {
            StopCoroutine(audioTransitionRoutine);
        }

        audioTransitionRoutine = StartCoroutine(audioTransition(audioSourceIndex, audioId));
    }

    //This variable will be used by BattleManager's musicManager to determine whether an audio transition is in progress.
    [HideInInspector] public bool audioTransitioning;

    //Audio transition enumerator
    //Allows smooth transitions between currently playing clip and clip in question.
    IEnumerator audioTransition(int audioSourceIndex, int audioId)
    {

        audioTransitioning = true;

        //Getting audiosources
        var audio = GetComponents<AudioSource>();

        //Save current source volume
        var curVol = audio[audioSourceIndex].volume;

        //Mute source gradually
        while (audio[audioSourceIndex].volume > 0)
        {
            audio[audioSourceIndex].volume -= 0.01f;
            yield return new WaitForEndOfFrame();
        }

        //stopping audio
        stopAudio(audioSourceIndex);

        //Setting and playing new clip
        setAudio(audioSourceIndex, audioId);
        playAudio(audioSourceIndex);

        //Turning volume back on
        while (audio[audioSourceIndex].volume < curVol)
        {
            audio[audioSourceIndex].volume += 0.01f;
            yield return new WaitForEndOfFrame();
        }

        audioTransitioning = false;

    }

    //Playing audio once
    public void playAudioOnce(int audioSourceIndex, int audioId)
    {

        //Getting clip
        var clip = AudioManager.Instance.findAudioClipById(audioId);

        //Getting audiosource(s)
        var audio = GetComponents<AudioSource>();

        //stopping audio
        if (audio.Length > audioSourceIndex)
        {
            audio[audioSourceIndex].PlayOneShot(clip);
        }
        else
        {
            Debug.Log("Invalid source index " + audioSourceIndex.ToString());
        }
    }

    //Setting an audio clip is required to play, resume and pause the audio further on
    public void setAudio(int audioId, int audioSourceIndex)
    {
        //Getting clip
        var clip = FunctionDB.core.findAudioClipById(audioId);

        //Checking clip's validity
        if (clip != null)
        {
            //Getting audioSource(s)
            var audio = GetComponents<AudioSource>();

            //Setting audio
            if (audio.Length > audioSourceIndex)
            {
                audio[audioSourceIndex].clip = clip;
            }
            else
            {
                Debug.Log("Invalid source index " + audioSourceIndex.ToString());
            }

        }
        else
        {
            Debug.Log("Invalid audio clip id " + audioId.ToString());
        }
    }

    //Playing set audio
    public void playAudio(int audioSourceIndex)
    {

        //Getting audiosource(s)
        var audio = GetComponents<AudioSource>();

        //Playing audio
        if (audio.Length > audioSourceIndex)
        {
            audio[audioSourceIndex].Play();
        }
        else
        {
            Debug.Log("Invalid source index " + audioSourceIndex.ToString());
        }

    }

    //Stopping audio
    public void stopAudio(int audioSourceIndex)
    {

        //Getting audiosource(s)
        var audio = GetComponents<AudioSource>();

        //stopping audio
        if (audio.Length > audioSourceIndex)
        {
            audio[audioSourceIndex].Stop();
        }
        else
        {
            Debug.Log("Invalid source index " + audioSourceIndex.ToString());
        }

    }

    public void pauseAudio(int audioSourceIndex)
    {
        //Getting audiosource(s)
        var audio = GetComponents<AudioSource>();

        //pausing audio
        if (audio.Length > audioSourceIndex)
        {
            audio[audioSourceIndex].Pause();
        }
        else
        {
            Debug.Log("Invalid source index " + audioSourceIndex.ToString());
        }
    }*/

    }
}
