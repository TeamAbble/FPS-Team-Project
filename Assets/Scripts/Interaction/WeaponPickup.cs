using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : Interactable
{
    public WeaponPrinter owningPrinter;
    Weapon w;
    private void Start()
    {
        w = GetComponent<Weapon>();
        interactText = "Pick up " +  w.displayName;
    }
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
        if(owningPrinter)
            owningPrinter.spawnedWeapon = null;
        GameManager.instance.playerRef.weaponManager.SwitchWeapon(GameManager.instance.playerRef.weaponManager.weapons.FindIndex(x => x.gameObject == gameObject));
        //Destroy the purchasable
        foreach (var item in GetComponentsInChildren<Renderer>(true))
        {
            if(item is not ParticleSystemRenderer)
                item.gameObject.layer = LayerMask.NameToLayer("PlayerRender");
        }
        Destroy(this);
    }
}
