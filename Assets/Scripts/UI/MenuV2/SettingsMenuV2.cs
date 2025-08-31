using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsMenuV2 : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void LoadSettingsAtLaunch()
    {
        if (File.Exists(SettingsPath))
        {
            settings = JsonUtility.FromJson<Settings>(File.ReadAllText(SettingsPath));
            settings.updated = true;
            Debug.Log($"Settings file exists at startup\nLocated at{SettingsPath}");
        }
    }

    public bool isSingleton;

    internal static string SettingsPath => (!Application.isEditor ? Application.dataPath : Application.persistentDataPath) + "/settings.json";
    public static Settings settings;

    public UniversalRenderer urpRenderer;
    public UniversalRendererData urpData;
    
    public UniversalRenderPipelineAsset urpAsset, overrideAsset;

    Resolution[] resolutions;
    List<Resolution> filteredResolutions;

    [System.Serializable]
    public struct Settings
    {
        [System.NonSerialized]
        public bool updated;

        public float sensitivity, gameVolume, uiVolume, renderScale;
        public Resolution resolution;

        public int frameLimit;
        public bool vsync, useFrameLimit, showFrames;
        public bool fullscreen;
    }

    public TMP_Dropdown resolutionDropdown;
    public Toggle vsyncToggle, frameCapToggle, showFrameToggle, fullscreenToggle;
    public Slider sensitivitySlide, gameVolumeSlide, uiVolumeSlide, frameSlide, renderScaleSlide;
    public TMP_Text fpsCapDisplay, renderScaleDisplay, gameAudioDisplay, sensDisplay, uiAudioDisplay;

    public AudioMixerGroup gameMixer, uiMixer;
    int currentResolutionIndex;

    private void Start()
    {
        //Use the updated flag to make sure we don't do erroneous operations on the settings.
        //We'll have two different instances of the settings menu in the game.
        //One is on the game manager, the other is on the main menu. Both need to be updated accordingly.
        if (!settings.updated)
        {
            if (File.Exists(SettingsPath))
            {
                LoadFile();
            }
            else
            {
                CreateNewSettings();
            }
            settings.updated = true;
        }
        BuildResolutionDropdown();
        AddListeners();
        ApplySettings();
    }
    void ApplySettings()
    {
        Screen.fullScreenMode = settings.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        if (filteredResolutions[currentResolutionIndex].height != Screen.height || filteredResolutions[currentResolutionIndex].height != Screen.height)
        {
            Screen.SetResolution(filteredResolutions[currentResolutionIndex].width, filteredResolutions[currentResolutionIndex].height, settings.fullscreen);
        }
        QualitySettings.vSyncCount = settings.vsync ? 1 : 0;
        if(GameManager.instance != null)
        {
            GameManager.instance.SetFrameCounter(settings.showFrames);
        }
        if (settings.useFrameLimit)
        {
            Application.targetFrameRate = settings.frameLimit;
        }
        else
        {
            Application.targetFrameRate = 500;
        }
        urpAsset.renderScale = settings.renderScale;
    }
    void AddListeners()
    {
        //Toggles
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        frameCapToggle.onValueChanged.AddListener(SetFrameCapOn);
        vsyncToggle.onValueChanged.AddListener(SetVsync);
        showFrameToggle.onValueChanged.AddListener(SetShowFrames);
        //Dropdown
        resolutionDropdown.onValueChanged.AddListener(SetResolutionIndex);
        //Sliders;
        frameSlide.onValueChanged.AddListener(SetFPSCap);
        gameVolumeSlide.onValueChanged.AddListener(SetGameAudio);
        uiVolumeSlide.onValueChanged.AddListener(SetUIAudio);
        renderScaleSlide.onValueChanged.AddListener(SetRenderScale);
    }
    private void OnDestroy()
    {
        if (isSingleton)
        {
            SaveFile();
            urpAsset.renderScale = 1;
        }
    }

    private void OnEnable()
    {
        vsyncToggle.isOn = settings.vsync;
        showFrameToggle.isOn = settings.showFrames;
        frameCapToggle.isOn = settings.useFrameLimit;
        fullscreenToggle.isOn = settings.fullscreen;

        frameSlide.value = settings.frameLimit;
        fpsCapDisplay.text = $"{frameSlide.value} FPS";
        sensitivitySlide.value = settings.sensitivity;
        sensDisplay.text = $"{sensitivitySlide.value}";
        gameVolumeSlide.value = settings.gameVolume;
        gameAudioDisplay.text = $"{(settings.gameVolume * 100):0.0}%";
        renderScaleSlide.value = settings.renderScale;
        renderScaleDisplay.text = $"x{settings.renderScale:0.0}";
        uiVolumeSlide.value = settings.uiVolume;
        uiAudioDisplay.text = $"{(settings.uiVolume * 100):0.0}%";
    }
    public void SetFullscreen(bool value)
    {
        settings.fullscreen = value;
        ApplySettings();
    }
    public void SetVsync(bool value)
    {
        settings.vsync = value;
        ApplySettings();
    }
    public void SetFrameCapOn(bool value)
    {
        settings.useFrameLimit = value;
        ApplySettings();
    }
    public void SetShowFrames(bool value)
    {
        settings.showFrames = value;
        ApplySettings();
    }
    public void SetFPSCap(float value)
    {
        settings.frameLimit = Mathf.RoundToInt(value);
        fpsCapDisplay.text = $"{value} FPS";
        ApplySettings();
    }
    public void SetResolutionIndex(int index)
    {
        currentResolutionIndex = index;
        ApplySettings();
    }
    public void SetRenderScale(float value)
    {
        settings.renderScale = value;
        renderScaleDisplay.text = $"x{settings.renderScale}";
        ApplySettings();
    }
    public void SetGameAudio(float value)
    {
        settings.gameVolume = value;
        gameAudioDisplay.text = $"{settings.gameVolume * 100}%";
        ApplySettings();
    }
    public void SetUIAudio(float value)
    {
        settings.uiVolume = value;
        uiAudioDisplay.text = $"{settings.uiVolume * 100}%";
        ApplySettings();
    }

    void BuildResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();
        resolutionDropdown.ClearOptions();
        float currentRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;
        filteredResolutions.AddRange(resolutions.Where(v => v.refreshRateRatio.value == currentRefreshRate));
        List<string> options = new();
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string option = $"{filteredResolutions[i].width}x{filteredResolutions[i].height}";
            options.Add(option);
            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void SaveFile()
    {
        //Then we can flip this updated flag at the end to make sure we only write the settings ONCE
        if (settings.updated)
        {
            if (!File.Exists(SettingsPath))
            {
                Debug.Log($"Created save file at {SettingsPath}");
                File.Create(SettingsPath).Close();
            }
            File.WriteAllText(SettingsPath, JsonUtility.ToJson(settings, true));
            Debug.Log("Wrote settings to file at end.");
            settings.updated = false;
        }
    }
    void CreateNewSettings()
    {
        settings = new()
        {
            resolution = Screen.currentResolution,
            frameLimit = 60,
            vsync = false,
            useFrameLimit = false,
            showFrames = false,
            sensitivity = 10,
            gameVolume = 1,
            fullscreen = false,
            renderScale = 1,
            updated = false
        };
        Debug.Log("Created new settings!");
        
    }
    void LoadFile()
    {
        if (File.Exists(SettingsPath))
        {
            settings = JsonUtility.FromJson<Settings>(File.ReadAllText(SettingsPath));
            Debug.Log($"Loaded settings from {SettingsPath}");
        }
        else
        {
            Debug.LogWarning("Failed to load file: settings Does Not Exist!");
        }
    }
}
