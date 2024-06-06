using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoVendor : Vendor
{
    public int ammo;
    public override void Purchase()
    {
        var a = GameManager.instance.playerRef.weaponManager.CurrentWeapon.Ammo;
        if (a.max > 0)
        {
            GameManager.instance.playerRef.weaponManager.CurrentWeapon.GiveReserveAmmo(ammo);
            base.Purchase();
        }
    }
}
