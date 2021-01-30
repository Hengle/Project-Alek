using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    private AsyncOperation operation;
    
    private static LoadSceneManager instance;
    
    public static LoadSceneManager Instance {
        get { if (!instance) Debug.LogError("Scene Loader is null");
            return instance; }
    }

    private void Awake() => instance = this;

    public void LoadBattle()
    {
        operation = SceneManager.LoadSceneAsync("Scenes/Battle");
        operation.allowSceneActivation = false;
        Timing.RunCoroutine(SceneTransitionManager.Instance.BattleTransition().
            Append(() => operation.allowSceneActivation = true));
    }

    public void LoadOverworld()
    {
        operation = SceneManager.LoadSceneAsync("Overworld Demo");
        operation.allowSceneActivation = false;
        Timing.RunCoroutine(SceneTransitionManager.Instance.OverworldTransition(1, true).
            Append(() => operation.allowSceneActivation = true));
    }
}
