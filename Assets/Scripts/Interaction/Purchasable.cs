using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Purchasable : Interactable
{
    public int cost;
    public override void Interact()
    {
        if(cost <= GameManager.instance.score)
        {
            Purchase();
        }
    }
    public virtual void Purchase()
    {

    }
}
