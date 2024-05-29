using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public class settings
    {
        public float volume = 0;
        public Vector2 sensitivity;

    }

    public Slider VolumeSlider;
    public Slider sensitivitySlider;
    public Button applyButton;

    void Start()
    {
        
    }

    private void OnEnable()
    {
        applyButton.onClick.AddListener(ApplySettings);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplySettings()
    {
        settings setting = new settings();
        setting.volume = VolumeSlider.value;

        string json = JsonUtility.ToJson(setting);
        string path = Application.dataPath + "/save.txt";

        
    }
    public void ChangeVolume()
    {
        float volume = VolumeSlider.value;
    }
}
