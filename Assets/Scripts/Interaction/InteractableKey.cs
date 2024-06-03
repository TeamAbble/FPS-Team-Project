using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableKey : Interactable
{
    public string keyID;
    public override void Interact()
    {
        GameManager.instance.heldKeysIDs.Add(keyID);
        Destroy(gameObject);
    }
}
