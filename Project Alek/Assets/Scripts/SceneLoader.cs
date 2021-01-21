using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Volume volume;
    private LensDistortion lensDistortion;
    private AsyncOperation operation;

    private static SceneLoader instance;
    
    public static SceneLoader Instance {
        get { if (!instance) Debug.LogError("Scene Loader is null");
            return instance; }
    }

    private void Awake() => instance = this;

    private void Start()
    {
        if (!volume.profile.TryGet(out lensDistortion))
        {
            Debug.LogError("Lens distortion component could not be found!");
        }

        lensDistortion.intensity.overrideState = true;
    }

    public void LoadBattle()
    {
        operation = SceneManager.LoadSceneAsync("Scenes/Battle");
        operation.allowSceneActivation = false;
        Timing.RunCoroutine(DistortLens().Append(() => operation.allowSceneActivation = true));
    }

    private IEnumerator<float> DistortLens()
    {
        while (Math.Abs(lensDistortion.intensity.value - lensDistortion.intensity.min) > 0.01f)
        {
            lensDistortion.intensity.value -= 0.01f;
            yield return Timing.WaitForOneFrame;
        }
    }

    public IEnumerator<float> ResetLens()
    {
        while (Math.Abs(lensDistortion.intensity.value - 0) > 0.01f)
        {
            lensDistortion.intensity.value += 0.01f;
            yield return Timing.WaitForOneFrame;
        }
    }
}
