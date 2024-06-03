using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : Interactable
{
    public WeaponPrinter owningPrinter;
    public override void Interact()
    {
        //Give the player this weapon
        GameManager.instance.playerRef.weaponManager.weapons.Add(GetComponent<Weapon>());
        //Parent it to the weapon transform on the player
        transform.SetParent(GameManager.instance.playerRef.weaponTransform, false);
        //reset the local pose
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        //callback to the weapon wheel to update that
        WeaponWheelController.Instance.UpdateWeaponWheel();
        GetComponent<Weapon>().GiveToEntity();
        owningPrinter.spawnedWeapon = null;
        GameManager.instance.playerRef.weaponManager.SwitchWeapon(GameManager.instance.playerRef.weaponManager.WeaponCount -1);
        //Destroy the purchasable
        Destroy(this);
    }
}
