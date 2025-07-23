using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponBarIcon : MonoBehaviour
{
    public Image icon;
    public Sprite weaponSprite;
    public Weapon assignedWeapon;

    public void Initialise(Weapon weapon)
    {
        assignedWeapon = weapon;
        weaponSprite = weapon.icon;
        icon.sprite = weaponSprite;
    }
}
