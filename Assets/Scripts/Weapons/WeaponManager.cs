using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] Weapon[] weapons;
    [SerializeField] int weaponIndex;
    [SerializeField] bool fireInput;
    public void OnFire(InputAction.CallbackContext context)
    {
        fireInput = context.ReadValueAsButton();
        weapons[weaponIndex].SetFireInput(fireInput);
    }
}
