using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuTabs : MonoBehaviour
{
    public CanvasGroup[] tabs;
    public int startingTab;

    public void SwitchToTab(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetGroupActive(i == index);
        }
    }
}
