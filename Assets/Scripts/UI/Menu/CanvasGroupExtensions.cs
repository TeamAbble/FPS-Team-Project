using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CanvasGroupExtensions
{
    public static void SetGroupActive(this CanvasGroup group, bool active)
    {
        group.alpha = active ? 1 : 0;
        group.interactable = active;
        group.blocksRaycasts = active;
        group.gameObject.SetActive(active);
    }
}
