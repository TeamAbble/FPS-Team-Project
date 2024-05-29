using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public class Settings
    {
        public float volume = 0;
        public Vector2 sensitivity = new Vector2(2, 2);

    }
    string SettingsPath => (Application.isEditor ? Application.dataPath : Application.persistentDataPath) + "/settings.json";
    public Slider VolumeSlider;
    public Slider sensitivitySlider;
    public AudioMixer mixer;
    public Settings settings = new();
    void Start()
    {
        LoadSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplySettings()
    {
        SaveSettings();
    }
    void LoadSettings()
    {
        //Check if the file exists, otherwise we try to save it
        if(!File.Exists(SettingsPath))
        {
            SaveSettings();
        }
        //We've made sure there actually IS a settings file, so we can continue
        string json = File.ReadAllText(SettingsPath);
        JsonUtility.FromJsonOverwrite(json, settings);

        GameManager.instance.lookSpeed = settings.sensitivity;
        mixer.SetFloat("Volume", settings.volume);
    }
    void SaveSettings()
    {
        if (!File.Exists(SettingsPath))
        {
            File.Create(SettingsPath);
        }
        string json = JsonUtility.ToJson(settings);
        File.WriteAllText(SettingsPath, json);
        
    }
}
