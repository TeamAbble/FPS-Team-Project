using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] Weapon[] weapons;
    [SerializeField] int weaponIndex;
    [SerializeField] bool fireInput;
    Player p;
    public bool IsAlive => p.IsAlive;
    public Weapon CurrentWeapon => weapons[weaponIndex];
    public int weaponLayer;
    private void Start()
    {
        p = GetComponent<Player>();
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(false);
        }
        weapons[weaponIndex].gameObject.SetActive(true);
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].UpdateTracers();
        }
        CurrentWeapon.fireBlocked = p.Animator.GetCurrentAnimatorStateInfo(weaponLayer).IsTag("Block");
    }

    public void SwitchWeapon()
    {
        weapons[weaponIndex].SetFireInput(false);
        weapons[weaponIndex].gameObject.SetActive(false);
        weaponIndex++;
        weaponIndex %= weapons.Length;
        weapons[weaponIndex].gameObject.SetActive(true);
    }
    public void SwitchWeapon(int newWeaponIndex)
    {
        weapons[weaponIndex].SetFireInput(false);
        weapons[weaponIndex].gameObject.SetActive(false);
        weaponIndex = newWeaponIndex;
        weaponIndex %= weapons.Length;
        weapons[weaponIndex].gameObject.SetActive(true);
    }
    public void SwitchInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameManager.instance.UseWeaponWheel(true);
        }

        if (context.canceled)
        {
            GameManager.instance.UseWeaponWheel(false);
        }
    }
    public void OnFire(InputAction.CallbackContext context)
    {
        if (GameManager.instance.paused || GameManager.instance.weaponWheelOpen)
        {
            fireInput = false;
            CurrentWeapon.SetFireInput(false);
            return;
        }
        fireInput = context.ReadValueAsButton();
        CurrentWeapon.SetFireInput(fireInput);
    }
    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
            p.Animator.SetTrigger("Reload");
    }
}
