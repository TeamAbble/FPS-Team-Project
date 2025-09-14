using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;

public class SettingsController : MonoBehaviour
{
    public static SettingsController Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            return;
        }

    }
    private void Start()
    {
        ApplySettings();
    }

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


    internal static string SettingsPath => (!Application.isEditor ? Application.dataPath : Application.persistentDataPath) + "/settings.json";
    public static Settings settings;

    public int currentResolutionIndex;
    

    public UniversalRenderer urpRenderer;
    public UniversalRendererData urpData;

    public string musicVolKey, gameVolKey;
    public AudioMixer mixer;

    public UniversalRenderPipelineAsset urpAsset, overrideAsset;

    public List<Resolution> filteredResolutions;

    [System.Serializable]
    public struct Settings
    {
        [System.NonSerialized]
        public bool updated;

        public float sensitivity, gameVolume, musicVolume, renderScale;
        public int resolutionIndex;

        public int frameLimit;
        public bool vsync, useFrameLimit, showFrames;
        public bool fullscreen;
    }
    
    public void ApplySettings()
    {
        Screen.fullScreenMode = settings.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        if (currentResolutionIndex >= 0 && filteredResolutions != null && filteredResolutions.Count > 0)
        {
            if (filteredResolutions[currentResolutionIndex].height != Screen.height || filteredResolutions[currentResolutionIndex].height != Screen.height)
            {
                Screen.SetResolution(filteredResolutions[currentResolutionIndex].width, filteredResolutions[currentResolutionIndex].height, settings.fullscreen);
            }
        }
        QualitySettings.vSyncCount = settings.vsync ? 1 : 0;
        if (GameManager.instance != null)
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
        mixer.SetFloat(musicVolKey, Mathf.Lerp(-80, 0, settings.musicVolume));
        mixer.SetFloat(gameVolKey, Mathf.Lerp(-80, 0, settings.gameVolume));
    }
}
