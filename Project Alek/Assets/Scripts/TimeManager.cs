using System.Collections.Generic;
using UnityEngine;
using MEC;

public static class TimeManager
{
    public static void PauseTime()
    {
        Time.timeScale = 0.0f;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
    }

    public static void ResumeTime()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
    }

    public static void SlowTime(float timeScale)
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
    }

    public static void SlowMotionSequence(float timeScale, float sequenceLength) =>
        Timing.RunCoroutine(SlowMotionCoroutine(timeScale, sequenceLength));

    private static IEnumerator<float> SlowMotionCoroutine(float timesScale, float sequenceLength)
    {
        SlowTime(timesScale);
        yield return Timing.WaitForSeconds(sequenceLength);
        ResumeTime();
    }
}