using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject titleMenu;

    public enum menus
    {
        START,MAIN
    }
    public menus menu;

    void Start()
    {
        menu = menus.START;
        titleMenu.SetActive(true);
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }

    // Update is called once per frame
    void OnGUI()
    {
        if (Event.current.isKey && Event.current.type == EventType.KeyDown&&menu==menus.START) {
            menu = menus.MAIN;
            mainMenu.SetActive(true);
            titleMenu.SetActive(false);
        }
    }
}
