using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionDoor : Purchasable
{
    bool opened;
    public Animator animator;
    private void Start()
    {
        if(!animator)
            animator = GetComponent<Animator>();
    }
    public override void Purchase()
    {
        if (!opened && animator)
        {
            base.Purchase();
            opened = true;
            animator.SetTrigger("Open");
        }
    }
}
