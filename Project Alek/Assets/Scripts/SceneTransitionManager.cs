using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private Image screen;
    [SerializeField] private Volume volume;
    private LensDistortion lensDistortion;
    
    private static SceneTransitionManager instance;
    
    public static SceneTransitionManager Instance {
        get { if (!instance) Debug.LogError("SceneTransitionManager is null");
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

        if (SceneManager.GetActiveScene().name == "Battle") Timing.RunCoroutine(ResetLens());
        else if (SceneManager.GetActiveScene().name == "Overworld Demo")
            Timing.RunCoroutine(OverworldTransition(0.01f, false));
    }

    public IEnumerator<float> OverworldTransition(float alpha, bool fromZero)
    {
        if (fromZero)
        {
            var fixedColor = screen.color;
            fixedColor.a = 1;
            screen.color = fixedColor;
            screen.CrossFadeAlpha(0f, 0f, true);
        }

        screen.CrossFadeAlpha(alpha, 2, false);
        yield return Timing.WaitForSeconds(2f);
    }
    
    public IEnumerator<float> BattleTransition()
    {
        while (Math.Abs(lensDistortion.intensity.value - lensDistortion.intensity.min) > 0.01f)
        {
            lensDistortion.intensity.value -= 0.01f;
            yield return Timing.WaitForOneFrame;
        }
    }

    private IEnumerator<float> ResetLens()
    {
        while (Math.Abs(lensDistortion.intensity.value - 0) > 0.01f)
        {
            lensDistortion.intensity.value += 0.01f;
            yield return Timing.WaitForOneFrame;
        }
    }
}