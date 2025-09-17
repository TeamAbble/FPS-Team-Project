using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthVendor : Vendor
{
    public int health;
    public override void Purchase()
    {
        if (GameManager.instance.playerRef.CurrentHealth < GameManager.instance.playerRef.MaxHealth)
        {
            base.Purchase();
            GameManager.instance.playerRef.UpdateHealth(health, transform.position);
            StatsManager.heals += health;
        }
    }
}
