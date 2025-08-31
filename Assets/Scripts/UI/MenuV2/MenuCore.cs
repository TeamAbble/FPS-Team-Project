using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuCore : MonoBehaviour
{
    public static MenuCore Instance;
    public float fadeTime; public float fadeWait;

    public float menuPanelWaitTime;
    public InputAction startScreenInput;

    public CinemachineVirtualCamera startCam, menuCam;

    public void SwitchToMenuByKey(MenuGroup a, string b)
    {
        SwitchToMenu(a, GroupByKey(b));
    }

    public void SwitchToMenu(MenuGroup a, MenuGroup b)
    {
        _ = StartCoroutine(MenuSwitchCoroutine(a, b));
    }
    IEnumerator MenuSwitchCoroutine(MenuGroup a, MenuGroup b)
    {
        yield return StartCoroutine(Fade(0, 1));
        a.panel.alpha = 0;
        a.panel.blocksRaycasts = false;
        a.panel.interactable = false;
        a.cam.enabled = false;
        b.panel.alpha = 1;
        b.panel.blocksRaycasts = true;
        b.panel.interactable = true;
        b.cam.enabled = true;
        yield return new WaitForSeconds(fadeWait);
        yield return StartCoroutine(Fade(1, 0));
    }
    IEnumerator Fade(float start, float end)
    {
        float fade = start; float fadeInc = (1 / fadeTime) * Time.fixedDeltaTime;
        var wff = new WaitForFixedUpdate();
        
        while (fade != end)
        {
            fade = Mathf.MoveTowards(fade, end, fadeInc);
            menuBlackout.alpha = fade;
            yield return wff;
        }
        menuBlackout.blocksRaycasts = end > 0;
    }
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            return;
        }
        StartScreenListener[] objects = FindObjectsByType<StartScreenListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var item in objects)
        {
            item.Register();
        }
        Debug.Log($"Registered {objects.Length} start screen listeners");
        startScreenInput.performed += StartScreenInput_performed;
        startScreenInput.Enable();

    }

    private void StartScreenInput_performed(InputAction.CallbackContext obj)
    {
        startScreenInput.Disable();
        passedStartScreen?.Invoke();
        StartCoroutine(MoveToMenu(GroupByKey(mainMenuKey)));
    }

    IEnumerator MoveToMenu(MenuGroup menu)
    {
        yield return new WaitForSeconds(menuPanelWaitTime * 2);
        startCam.enabled = false;
        menuCam.enabled = true;
        yield return new WaitForSeconds(menuPanelWaitTime);
        float fade = 0; float fadeInc = (1 / fadeTime) * Time.fixedDeltaTime;
        var wff = new WaitForFixedUpdate();
        while (fade != 1)
        {
            fade = Mathf.MoveTowards(fade, 1, fadeInc);
            menu.panel.alpha = fade;
            yield return wff;
        }
    }
    
    /// <summary>
    /// This delegate should only be called once, when we get past the start screen.
    /// </summary>
    public delegate void PassedStartScreen();
    public PassedStartScreen passedStartScreen;
    public CanvasGroup menuBlackout;
    public string mainMenuKey;
    [System.Serializable]
    public struct MenuGroup
    {
        public CinemachineVirtualCamera cam;
        public CanvasGroup panel;
        public string key;
    }
    public MenuGroup[] menuGroups;
    public MenuGroup GroupByKey(string key) => menuGroups.First(x => x.key == key);

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.StartGame();
        }
    }
}
