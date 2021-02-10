using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MEC;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Audio
{
    public class AudioController : MonoBehaviour
    {
            private static AudioController instance;

            public bool debug;
            public List<AudioTrack> tracks = new List<AudioTrack>();

            private Hashtable mAudioTable; // relationship of audio types (key) and tracks (value)
            private Hashtable mJobTable;   // relationship between audio types (key) and jobs (value)

            private enum AudioAction { Start, Stop, Restart }

            [Serializable] public class AudioObject
            {
                public AudioType type;
                public AudioClip clip;
            }

            [Serializable] public class AudioTrack
            {
                public AudioSource source;
                public AudioObject[] audio;

                public AudioTrack(AudioSource audioSource, AudioObject[] audioObjects)
                {
                    source = audioSource;
                    audio = audioObjects;
                }
            }

            private class AudioJob
            {
                public readonly AudioAction _action;
                public readonly AudioType _type;
                public readonly bool _fade;
                public readonly float _fadeDuration;
                public readonly float _delay;

                public AudioJob(AudioAction action, AudioType type, bool fade, float fadeDuration, float delay)
                {
                    _action = action;
                    _type = type;
                    _fade = fade;
                    _fadeDuration = fadeDuration;
                    _delay = delay;
                }
            }

#region Unity Functions

            private void Awake() { if (!instance) Configure(); }

            private void OnDisable() => Dispose();
            
#endregion

#region Public Functions

            public void PlayAudio(AudioType type, bool fade=false, float fadeDuration = 1.0f, float delay=0.0F) =>
                AddJob(new AudioJob(AudioAction.Start, type, fade, fadeDuration, delay));
            
            public void StopAudio(AudioType type, bool fade=false, float fadeDuration = 1.0f, float delay=0.0F) =>
                AddJob(new AudioJob(AudioAction.Stop, type, fade, fadeDuration, delay));
            
            public void RestartAudio(AudioType type, bool fade=false, float fadeDuration = 1.0f, float delay=0.0F) =>
                AddJob(new AudioJob(AudioAction.Restart, type, fade, fadeDuration, delay));

            public void AddNewTrack(AudioTrack track)
            {
                if (tracks.Exists(t =>
                    t.source.name == track.source.name)) return;
                
                tracks.Add(track);
                UpdateAudioTable(track);
            }
            
#endregion

#region Private Functions
            private void Configure() 
            {
                instance = this;
                mAudioTable = new Hashtable();
                mJobTable = new Hashtable();
                GenerateAudioTable();
            }

            private void Dispose()
            {
                // cancel all jobs in progress
                foreach (var job in from DictionaryEntry kvp
                    in mJobTable select (IEnumerator<float>)kvp.Value)
                {
                    StopCoroutine(job);
                }
            }

            private void AddJob(AudioJob job) 
            {
                // cancel any job that might be using this job's audio source
                RemoveConflictingJobs(job._type);

                var jobRunner = RunAudioJob(job);
                mJobTable.Add(job._type, jobRunner);
                Timing.RunCoroutine(jobRunner);
                Log("Starting job on ["+job._type+"] with operation: "+job._action);
            }

            private void RemoveJob(AudioType type) {
                if (!mJobTable.ContainsKey(type))
                {
                    Log("Trying to stop a job ["+type+"] that is not running.");
                    return;
                }
                var runningJob = (IEnumerator<float>)mJobTable[type];
                StopCoroutine(runningJob);
                mJobTable.Remove(type);
            }

            private void RemoveConflictingJobs(AudioType type) {
                // cancel the job if one exists with the same type
                if (mJobTable.ContainsKey(type)) RemoveJob(type);

                // cancel jobs that share the same audio track
                AudioType conflictAudio = null;
                foreach (DictionaryEntry entry in mJobTable)
                {
                    var audioType = (AudioType)entry.Key;
                    var audioTrackInUse = GetAudioTrack(audioType, "Get Audio Track In Use");
                    var audioTrackNeeded = GetAudioTrack(type, "Get Audio Track Needed");
                    if (audioTrackInUse.source == audioTrackNeeded.source) conflictAudio = audioType;
                }

                if (conflictAudio != null) RemoveJob(conflictAudio); 
            }

            private IEnumerator<float> RunAudioJob(AudioJob job)
            {
                yield return Timing.WaitForSeconds(job._delay);

                var track = GetAudioTrack(job._type); // track existence should be verified by now
                track.source.clip = GetAudioClipFromAudioTrack(job._type, track);

                switch (job._action)
                {
                    case AudioAction.Start: track.source.Play();
                        break;
                    case AudioAction.Stop: if (!job._fade) track.source.Stop();
                        break;
                    case AudioAction.Restart:
                        track.source.Stop();
                        track.source.Play();
                        break;
                }
                
                if (job._fade)
                {
                    float initial = job._action == AudioAction.Start || job._action == AudioAction.Restart ? 0 : 1;
                    float target = Math.Abs(initial) < 0.0001f ? 1 : 0;
                    var duration = job._fadeDuration;
                    var timer = 0.0f;

                    while (timer < duration)
                    {
                        track.source.volume = Mathf.Lerp(initial, target, timer / duration);
                        timer += Time.deltaTime;
                        yield return Timing.WaitForOneFrame;
                    }

                    if (job._action == AudioAction.Stop) track.source.Stop();
                }

                mJobTable.Remove(job._type);
                Log("Job count: "+mJobTable.Count);
            }

            private void GenerateAudioTable()
            {
                foreach(var track in tracks)
                {
                    foreach(var obj in track.audio)
                    {
                        if (mAudioTable.ContainsKey(obj.type))
                        {
                            LogWarning("You are trying to register audio ["+obj.type+"] that has already been registered.");
                        } 
                        else
                        {
                            mAudioTable.Add(obj.type, track);
                            Log("Registering audio ["+obj.type+"]");
                        }
                    }
                }
            }

            private void UpdateAudioTable(AudioTrack track)
            {
                foreach (var obj in track.audio)
                {
                    if (mAudioTable.ContainsKey(obj.type))
                    {
                        LogWarning("You are trying to register audio ["+obj.type+"] that has already been registered.");
                    } 
                    else
                    {
                        mAudioTable.Add(obj.type, track);
                        Log("Registering audio ["+obj.type+"]");
                    }
                }
            }

            private AudioTrack GetAudioTrack(AudioType type, string job="")
            {
                if (mAudioTable.ContainsKey(type)) return (AudioTrack) mAudioTable[type];
                LogWarning("You are trying to <color=#fff>"+job+"</color> for ["+type+"] but no track was found supporting this audio type.");
                return null;
            }

            private static AudioClip GetAudioClipFromAudioTrack(AudioType type, AudioTrack track)
            {
                return (from obj in track.audio where obj.type == type select obj.clip).FirstOrDefault();
            }

            private void Log(string msg)
            {
                if (!debug) return;
                Debug.Log("[Audio Controller]: "+msg);
            }
            
            private void LogWarning(string msg)
            {
                if (!debug) return;
                Debug.LogWarning("[Audio Controller]: "+msg);
            }
#endregion
    }
}