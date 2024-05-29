using Eclipse.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public List<Weapon> weapons = new();
    [SerializeField] int weaponIndex;
    [SerializeField] bool fireInput;
    Player p;
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
        weapons[weaponIndex].gameObject.SetActive(true);
        ChangeAnimations();

        WeaponWheelController.Instance.UpdateWeaponWheel();
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < WeaponCount; i++)
        {
            weapons[i].UpdateTracers();
        }
        CurrentWeapon.fireBlocked = p.Animator.GetCurrentAnimatorStateInfo(weaponLayer).IsTag("Block");
    }

    public void SwitchWeapon(bool increment)
    {
        if (CurrentWeapon.newMag.magazine)
            animationHelper.ReleaseNewMagInDefault();
        if(CurrentWeapon.oldMag.magazine)
            animationHelper.ReleaseOldMagInDefault();
        weapons[weaponIndex].SetFireInput(false);
        weapons[weaponIndex].gameObject.SetActive(false);
        weaponIndex += increment ? 1 : -1;
        weaponIndex %= WeaponCount;
        weapons[weaponIndex].gameObject.SetActive(true);
        p.Animator.Play("Equip");
        ChangeAnimations();
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
        if (context.performed && CurrentWeapon.CanReload)
            p.Animator.SetTrigger("Reload");
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
    }
    public void ReceiveRecoilImpulse(Vector3 pos, Vector3 rot)
    {
        p.ReceiveRecoilImpulse(pos, rot);
    }
}
