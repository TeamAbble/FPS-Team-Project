using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCloser : MonoBehaviour
{
    public CanvasGroup lastMenu, thisMenu;

    public void CloseMenu()
    {
        lastMenu.SetGroupActive(true);
        thisMenu.SetGroupActive(false);
    }
}
