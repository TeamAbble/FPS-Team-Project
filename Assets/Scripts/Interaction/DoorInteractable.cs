using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractable : Purchasable
{
    public Animator animator;
    public string keyID;
    private void Start()
    {
        if(!animator)
            animator = GetComponent<Animator>();
    }
    public override void Purchase()
    {
        base.Purchase();
        //If we have no keyID OR we have this key
        if (string.IsNullOrEmpty(keyID) || GameManager.instance.heldKeysIDs.Contains(keyID))
        {
            animator.SetTrigger("Open");
            GameManager.instance.heldKeysIDs.Remove(keyID);
        }
    }
}
