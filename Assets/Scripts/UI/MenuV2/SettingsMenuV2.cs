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

    public bool isSingleton;

   

    public TMP_Dropdown resolutionDropdown;
    public Toggle vsyncToggle, frameCapToggle, showFrameToggle, fullscreenToggle;
    public Slider sensitivitySlide, gameVolumeSlide, musicVolumeSlide, frameSlide, renderScaleSlide;
    public TMP_Text fpsCapDisplay, renderScaleDisplay, gameAudioDisplay, musicAudioDisplay, sensDisplay;

    public Toggle cheatsToggle;
    public void SetCheatsEnabled(bool value)
    {
        GameManager.cheatsEnabled = value;
        StatsManager.cheatsWereEverUsed = true;
    }
    public AudioMixerGroup gameMixer, uiMixer;
    Resolution[] resolutions;

    private void Awake()
    {
        //Remove initialise from Awake, called by MonoEventListener
        //Initialise();
    }
    public void Initialise()
    {
        //Use the updated flag to make sure we don't do erroneous operations on the settings.
        //We'll have two different instances of the settings menu in the game.
        //One is on the game manager, the other is on the main menu. Both need to be updated accordingly.
        if (!SettingsController.settings.updated)
        {
            if (File.Exists(SettingsController.SettingsPath))
            {
                LoadFile();
            }
            else
            {
                CreateNewSettings();
            }
            SettingsController.settings.updated = true;
        }
        BuildResolutionDropdown();
        AddListeners();
        if(SettingsController.Instance != null)
            SettingsController.Instance.ApplySettings();
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
        musicVolumeSlide.onValueChanged.AddListener(SetMusicAudio);
        renderScaleSlide.onValueChanged.AddListener(SetRenderScale);
    }
    private void OnDestroy()
    {
        if (isSingleton)
        {
            SaveFile();
            SettingsController.Instance.urpAsset.renderScale = 1;
        }
    }

    private void OnEnable()
    {
        if(SettingsController.Instance != null)
        {
            vsyncToggle.isOn = SettingsController.settings.vsync;
            showFrameToggle.isOn = SettingsController.settings.showFrames;
            frameCapToggle.isOn = SettingsController.settings.useFrameLimit;
            fullscreenToggle.isOn = SettingsController.settings.fullscreen;

            frameSlide.value = SettingsController.settings.frameLimit;
            fpsCapDisplay.text = $"{frameSlide.value} FPS";
            sensitivitySlide.value = SettingsController.settings.sensitivity;
            sensDisplay.text = $"{sensitivitySlide.value}";
            gameVolumeSlide.value = SettingsController.settings.gameVolume;
            musicVolumeSlide.value = SettingsController.settings.musicVolume;
            gameAudioDisplay.text = $"{(SettingsController.settings.gameVolume * 100):0.0}%";
            musicAudioDisplay.text = $"{(SettingsController.settings.musicVolume * 100):0.0}%";
            renderScaleSlide.value = SettingsController.settings.renderScale;
            renderScaleDisplay.text = $"x{SettingsController.settings.renderScale:0.00}";
        }
    }
    public void SetSensitivity(float value)
    {
        SettingsController.settings.sensitivity = value;
        SettingsController.Instance.ApplySettings();
        sensDisplay.text = $"{sensitivitySlide.value}";
    }
    public void SetFullscreen(bool value)
    {
        SettingsController.settings.fullscreen = value;
        SettingsController.Instance.ApplySettings();
    }
    public void SetVsync(bool value)
    {
        SettingsController.settings.vsync = value;
        SettingsController.Instance.ApplySettings();
    }
    public void SetFrameCapOn(bool value)
    {
        SettingsController.settings.useFrameLimit = value;
        SettingsController.Instance.ApplySettings();
    }
    public void SetShowFrames(bool value)
    {
        SettingsController.settings.showFrames = value;
        SettingsController.Instance.ApplySettings();
    }
    public void SetFPSCap(float value)
    {
        SettingsController.settings.frameLimit = Mathf.RoundToInt(value);
        fpsCapDisplay.text = $"{value} FPS";
        SettingsController.Instance.ApplySettings();
    }
    public void SetResolutionIndex(int index)
    {
        SettingsController.Instance.currentResolutionIndex = index;
        SettingsController.Instance.ApplySettings();
    }
    public void SetRenderScale(float value)
    {
        SettingsController.settings.renderScale = value;
        renderScaleDisplay.text = $"x{SettingsController.settings.renderScale:0.0}";
        SettingsController.Instance.ApplySettings();
    }
    public void SetGameAudio(float value)
    {
        SettingsController.settings.gameVolume = value;
        gameAudioDisplay.text = $"{(SettingsController.settings.gameVolume * 100):0.0}%";
        SettingsController.Instance.ApplySettings();
    }
    public void SetMusicAudio(float value)
    {
        SettingsController.settings.musicVolume = value;
        musicAudioDisplay.text = $"{(SettingsController.settings.musicVolume * 100):0.0}%";
        SettingsController.Instance.ApplySettings();
    }

    void BuildResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        if (SettingsController.Instance == null)
            return;
        SettingsController.Instance.filteredResolutions = new List<Resolution>();
        resolutionDropdown.ClearOptions();
        float currentRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;
        SettingsController.Instance.filteredResolutions.AddRange(resolutions.Where(v => v.refreshRateRatio.value == currentRefreshRate));
        List<string> options = new();
        SettingsController.Instance.currentResolutionIndex = -1;
        for (int i = 0; i < SettingsController.Instance.filteredResolutions.Count; i++)
        {
            Resolution r = SettingsController.Instance.filteredResolutions[i];
            string option = $"{r.width}x{r.height}";
            options.Add(option);
            Debug.Log($"{r.width}x{r.height} - {Screen.width}x{Screen.height} == {r.width == Screen.width}x{r.height == Screen.height}");
            if (r.width == Screen.width && r.height == Screen.height)
            {
                SettingsController.Instance.currentResolutionIndex = i;
            }
        }
        if(SettingsController.Instance.currentResolutionIndex == -1)
        {
            SettingsController.Instance.currentResolutionIndex = SettingsController.Instance.filteredResolutions.Count - 1;
        }


        SettingsController.settings.resolutionIndex = SettingsController.Instance.currentResolutionIndex;
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = SettingsController.Instance.currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void SaveFile()
    {
        //Then we can flip this updated flag at the end to make sure we only write the settings ONCE
        if (SettingsController.settings.updated)
        {
            if (!File.Exists(SettingsController.SettingsPath))
            {
                Debug.Log($"Created save file at {SettingsController.SettingsPath}");
                File.Create(SettingsController.SettingsPath).Close();
            }
            File.WriteAllText(SettingsController.SettingsPath, JsonUtility.ToJson(SettingsController.settings, true));
            Debug.Log("Wrote settings to file at end.");
            SettingsController.settings.updated = false;
        }
    }
    void CreateNewSettings()
    {
        SettingsController.settings = new()
        {
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
        if (File.Exists(SettingsController.SettingsPath))
        {
            SettingsController.settings = JsonUtility.FromJson<SettingsController.Settings>(File.ReadAllText(SettingsController.SettingsPath));
            Debug.Log($"Loaded settings from {SettingsController.SettingsPath}");
        }
        else
        {
            Debug.LogWarning("Failed to load file: settings Does Not Exist!");
        }
    }
}
