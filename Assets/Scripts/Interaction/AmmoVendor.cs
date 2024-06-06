using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoVendor : Vendor
{
    public int ammo;
    public override void Purchase()
    {
        if (GameManager.instance.playerRef.CurrentHealth < GameManager.instance.playerRef.MaxHealth)
        {
            base.Purchase();
            GameManager.instance.playerRef.weaponManager.CurrentWeapon.GiveReserveAmmo(ammo);
            if (particle)
                particle.Emit(1);
            if (audioSource && clip)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
