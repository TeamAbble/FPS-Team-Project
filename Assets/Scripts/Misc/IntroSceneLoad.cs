using Eflatun.SceneReference;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneLoad : MonoBehaviour
{
    public SceneReference menuScene;
    public void LoadMenuScene()
    {
        SceneManager.LoadScene(menuScene.Name);
    }
}
