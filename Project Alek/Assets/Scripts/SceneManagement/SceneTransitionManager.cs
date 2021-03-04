﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MEC;

namespace SceneManagement
{
    public class SceneTransitionManager : MonoBehaviorSingleton<SceneTransitionManager>
    {
        [SerializeField] private Image screen;
        [SerializeField] private Volume volume;
        private LensDistortion lensDistortion;

        private void Start()
        {
            if (volume && !volume.profile.TryGet(out lensDistortion)) 
                Debug.LogWarning("Lens distortion component could not be found!");
        
            if (lensDistortion) lensDistortion.intensity.overrideState = true;

            Timing.RunCoroutine(SceneManager.GetActiveScene().name == "Battle"
                ? ResetLens()
                : OverworldTransition(0.01f, false));
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
                lensDistortion.intensity.value -= 0.02f;
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
}