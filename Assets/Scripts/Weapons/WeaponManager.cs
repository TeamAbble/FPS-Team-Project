using Eclipse.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public List<Weapon> weapons = new();
    [SerializeField] internal int weaponIndex;
    [SerializeField] bool fireInput;
    internal Player p;
    public bool IsAlive => p.IsAlive;
    public Weapon CurrentWeapon => weapons[weaponIndex];
    public int weaponLayer;
    public int WeaponCount => weapons.Count;
    public AnimationHelper animationHelper;
    private void Start()
    {
        p = GetComponent<Player>();
        for (int i = 0; i < WeaponCount; i++)
        {
            weapons[i].gameObject.SetActive(false);
            weapons[i].GiveToEntity();
        }
        weaponIndex = 0;
        weapons[weaponIndex].gameObject.SetActive(true);
        WeaponBar.Instance.UpdateWeaponBar();
        SwitchWeapon(0);
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < WeaponCount; i++)
        {
            weapons[i].UpdateTracers();
        }
        CurrentWeapon.fireBlocked = p.Animator.GetCurrentAnimatorStateInfo(weaponLayer).IsTag("Block");
        if(CurrentWeapon.CanReload && !CurrentWeapon.fireBlocked && CurrentWeapon.Ammo.current == 0)
        {
            p.Animator.SetTrigger("Reload");
            CurrentWeapon.PlayReloadSound();
        }
    }


    

    public void SwitchWeapon(bool increment)
    {
        if(WeaponCount > 1)
        {
            CurrentWeapon.StopReloadSound();
            if (CurrentWeapon.newMag.magazine)
                animationHelper.ReleaseNewMagInDefault();
            if(CurrentWeapon.oldMag.magazine)
                animationHelper.ReleaseOldMagInDefault();
            weapons[weaponIndex].SetFireInput(false);
            weapons[weaponIndex].gameObject.SetActive(false);
            weaponIndex += increment ? 1 : -1;
            if(weaponIndex < 0)
            {
                weaponIndex = weapons.Count + weaponIndex;
            }
            weaponIndex %= WeaponCount;
            weapons[weaponIndex].gameObject.SetActive(true);
            p.Animator.Play("Equip");
            ChangeAnimations();
        }
        WeaponBar.Instance.UpdateWeaponHighlight();
    }
    public void SwitchWeapon(int newWeaponIndex)
    {
        if (CurrentWeapon.newMag.magazine)
            animationHelper.ReleaseNewMagInDefault();
        if (CurrentWeapon.oldMag.magazine)
            animationHelper.ReleaseOldMagInDefault();
        weapons[weaponIndex].SetFireInput(false);
        weapons[weaponIndex].gameObject.SetActive(false);
        weaponIndex = newWeaponIndex;
        weaponIndex %= WeaponCount;
        weapons[weaponIndex].gameObject.SetActive(true);
        p.Animator.Play("Equip");
        ChangeAnimations();
        WeaponBar.Instance.UpdateWeaponHighlight();
    }
    //public void SwitchInput(InputAction.CallbackContext context)
    //{
    //if (GameManager.instance.paused)
    //return;
    //if (context.performed)
    //{
    //GameManager.instance.UseWeaponWheel(true);
    //}

    //if (context.canceled)
    //{
    //GameManager.instance.UseWeaponWheel(false);
    //}
    //}
    public void OnFire(InputAction.CallbackContext context)
    {
        if (GameManager.instance.paused)
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
        if (context.performed && CurrentWeapon.CanReload && !GameManager.instance.paused)
            p.Animator.SetTrigger("Reload");
    }
    public void OnCycleWeaponLeft(InputAction.CallbackContext context)
    {
        if(context.performed)
            SwitchWeapon(false);
    }
    public void OnCycleWeaponRight(InputAction.CallbackContext context)
    {
        if(context.performed)
            SwitchWeapon(true);
    }


    AnimatorOverrideController aoc;
    AnimationClipOverrides overrideclips;
    public void ChangeAnimations()
    {
        if (!aoc)
        {
            aoc = new(p.Animator.runtimeAnimatorController);
            p.Animator.runtimeAnimatorController = aoc;
            overrideclips = new(aoc.overridesCount);
            aoc.GetOverrides(overrideclips);
        }
        if (CurrentWeapon.animationSet)
        {
            for (int i = 0; i < CurrentWeapon.animationSet.overrides.Count; i++)
            {
                AnimationOverrides a = CurrentWeapon.animationSet.overrides[i];
                overrideclips[a.name] = a.overrideClip;
            }
            aoc.ApplyOverrides(overrideclips);
        }
        p.currentRecoilProfile = CurrentWeapon.recoilProfile;
        if (CurrentWeapon.meleeWeapon)
            p.meleeDamage = CurrentWeapon.Damage;
    }
    public void ReceiveRecoilImpulse(Vector3 pos, Vector3 rot)
    {
        p.ReceiveRecoilImpulse(pos, rot);
    }
}
