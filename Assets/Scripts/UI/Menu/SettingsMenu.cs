using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    string SettingsPath => (!Application.isEditor ? Application.dataPath : Application.persistentDataPath) + "/settings.json";
    public Slider VolumeSlider;
    public Slider sensitivitySlider;
    public AudioMixer mixer;
    public Settings settings = new();
    public Button CloseButton;
    public Button ApplyButton;
    public CanvasGroup previousMenu;
    public CanvasGroup thisMenu;
    void Start()
    {
        LoadSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        if(CloseButton)
        CloseButton.onClick.AddListener(ExitMenu);
    }

    public void ApplySettings()
    {
        settings.sensitivity = Vector2.one * sensitivitySlider.value;
        settings.volume = VolumeSlider.value;

        GameManager.instance.lookSpeed = settings.sensitivity;
        AudioListener.volume = Mathf.InverseLerp(-80, 0, settings.volume);
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
        string json = File.ReadAllText(SettingsPath, Encoding.UTF8);
        JsonUtility.FromJsonOverwrite(json, settings);

        GameManager.instance.lookSpeed = settings.sensitivity;
        AudioListener.volume = Mathf.InverseLerp(-80, 0, settings.volume);

        VolumeSlider.value = settings.volume;
        sensitivitySlider.value = settings.sensitivity.x;
        
    }
    void SaveSettings()
    {
        using FileStream file = File.Open(SettingsPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        string json = JsonUtility.ToJson(settings);
        file.SetLength(0);
        file.Flush();
        file.Write(Encoding.UTF8.GetBytes(json));
        file.Close();
        print($"{json} saved to {SettingsPath}");

    }
    void ExitMenu()
    {
        previousMenu.SetGroupActive(true);
        thisMenu.SetGroupActive(false);
    }
}
