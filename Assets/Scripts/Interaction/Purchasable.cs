using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Purchasable : Interactable
{
    public int cost;
    public virtual int Cost => cost;
    public override void Interact()
    {
        if((GameManager.cheatsEnabled && GameManager.ch_babyNoMoney) || Cost <= GameManager.instance.score)
        {
            Purchase();
        }
    }
    public virtual void Purchase()
    {
        if (GameManager.cheatsEnabled && GameManager.ch_babyNoMoney)
            return;
        GameManager.instance.score -= Cost;
    }
}
