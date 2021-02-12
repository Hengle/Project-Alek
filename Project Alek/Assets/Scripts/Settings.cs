using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private RenderPipelineAsset[] qualityLevels;
    [SerializeField] private TMP_Dropdown qualityPresetDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider slider;
    [SerializeField] private Toggle fullscreenToggle;
    private Resolution[] resolutions;

    private void Start()
    {
        InitializeQualityLevel();
        SetResolutionOptions();
        InitializeVolumeValue();
        InitializeFullscreen();
    }
    
    private void InitializeQualityLevel() => 
        qualityPresetDropdown.value = QualitySettings.GetQualityLevel();
    
    private void InitializeFullscreen() => fullscreenToggle.isOn = Screen.fullScreen;

    private void InitializeVolumeValue()
    {
        audioMixer.GetFloat("MasterVolume", out var volume);
        slider.value = volume;
    }

    private void SetResolutionOptions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        
        var options = new List<string>();
        var resolutionIndex = 0;
        for (var i = 0; i < resolutions.Length; i++)
        {
            var option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);

            if (resolutions[i].width == Screen.width &&
                resolutions[i].height == Screen.height)
            {
                resolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = resolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int index)
    {
        var resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        CameraViewportController.Instance.SetAspect();
    }

    public void SetQualityLevel(int level)
    {
        QualitySettings.SetQualityLevel(level);
        QualitySettings.renderPipeline = qualityLevels[level];
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}