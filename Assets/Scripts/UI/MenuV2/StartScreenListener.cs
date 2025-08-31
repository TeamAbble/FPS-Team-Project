using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StartScreenListener : MonoBehaviour
{
    


    public UnityEvent events;
    internal void Register()
    {
        if (MenuCore.Instance != null)
        {
            MenuCore.Instance.passedStartScreen += PlayEvents;
        }
    }

    private void OnDestroy()
    {
        if(MenuCore.Instance != null)
        {
            MenuCore.Instance.passedStartScreen -= PlayEvents;
        }
    }
    void PlayEvents()
    {
        events?.Invoke();
    }
}
