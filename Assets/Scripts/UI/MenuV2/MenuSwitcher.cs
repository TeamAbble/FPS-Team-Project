using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuSwitcher : MonoBehaviour
{
    public string menuKey;
    private void Start()
    {
        parent = GetComponentInParent<CanvasGroup>();

    }

    CanvasGroup parent;
    public void SwitchToMenu()
    {
        var a = MenuCore.Instance.menuGroups.First(x => x.panel == parent);
        if(a.panel != null)
        {
            MenuCore.Instance.SwitchToMenuByKey(a, menuKey);
        }

    }
}
