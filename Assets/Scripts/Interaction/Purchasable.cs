using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Purchasable : Interactable
{
    public int cost;
    public virtual int Cost => cost;
    public override void Interact()
    {
        if(Cost <= GameManager.instance.score)
        {
            Purchase();
        }
    }
    public virtual void Purchase()
    {
        GameManager.instance.score -= Cost;
    }
}
