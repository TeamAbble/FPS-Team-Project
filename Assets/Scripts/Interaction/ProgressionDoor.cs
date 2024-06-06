using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionDoor : Purchasable
{
    bool opened;
    public override int Cost => GameManager.instance.areaUnlockCost;
    public ParticleSystem particle;
    public override void Purchase()
    {
        base.Purchase();
        particle.transform.SetParent(null, true);
        particle.Play();
        Destroy(particle, 10f);
        GameManager.instance.areaUnlockCost *= GameManager.instance.areaCostMultiplier;
        Destroy(gameObject);
    }
}
