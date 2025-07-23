using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
    public Button creditsButton;
    public MenuManager menuManagerRef;
    public string sceneName;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(Settings);
        quitButton.onClick.AddListener(Quit);
        creditsButton.onClick.AddListener(Credits);
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void StartGame()
    {
        GameManager.instance.StartGame();
    }
    public void Settings()
    {
        menuManagerRef.settingsMenu.SetGroupActive(true);
        menuManagerRef.mainMenu.SetGroupActive(false);
    }
    public void Credits()
    {
        menuManagerRef.creditsMenu.SetGroupActive(true);
        menuManagerRef.mainMenu.SetGroupActive(false);
    }
}
