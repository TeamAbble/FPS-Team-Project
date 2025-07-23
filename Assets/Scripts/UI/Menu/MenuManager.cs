using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public CanvasGroup mainMenu;
    public CanvasGroup settingsMenu;
    public CanvasGroup titleMenu;
    public CanvasGroup creditsMenu;

    public InputAction ia;

    public enum menus
    {
        START,MAIN
    }
    public menus menu;

    void Start()
    {
        menu = menus.START;
        titleMenu.SetGroupActive(true);
        mainMenu.SetGroupActive(false);
        settingsMenu.SetGroupActive(false);
        creditsMenu.SetGroupActive(false);

        
        ia.Enable();
        ia.performed += Ia_performed;
    }

    private void Ia_performed(InputAction.CallbackContext obj)
    {
        if (menu == menus.START && obj.ReadValueAsButton())
        {
            ia.Disable();
            ia.performed -= Ia_performed;
            menu = menus.MAIN;
            mainMenu.SetGroupActive(true);
            titleMenu.SetGroupActive(false);
        }
    }
}
