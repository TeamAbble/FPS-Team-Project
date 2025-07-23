using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnableSelection : MonoBehaviour
{
    public Selectable selectable;

    private void OnEnable()
    {
        if (selectable)
        {
            selectable.Select();
        }
    }
    private void OnValidate()
    {
        selectable = GetComponent<Selectable>();
    }
}
