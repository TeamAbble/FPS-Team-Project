using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Weapon : MonoBehaviour
{
    [SerializeField, Tooltip("The maximum ammunition held by a weapon at one time. If zero, this weapon does not consume ammo.")] protected float maxAmmo;
    [SerializeField, Tooltip("How much ammunition we currently have.")] protected float currentAmmo;
    [SerializeField, Tooltip("The maximum damage dealt to an enemy.")] protected float damage;
    [SerializeField, Tooltip("How many 'Projectiles' a weapon will fire at an enemy.")] protected int projectilesPerShot;
    [SerializeField, Tooltip("The time, in seconds, between each shot")] protected float fireInterval;
    [SerializeField, Tooltip("The remaining fire interval. Useful for interpolating visuals on weapons.")] protected float fireIntervalRemaining;
    [SerializeField, Tooltip("If true, the weapon will always fire once when clicked, regardless of the windup time.\nIf false, the weapon will only fire when the [CurrentWindup] reaches [FireWindup]")] protected bool forceFirstShot;
    [SerializeField, Tooltip("The wait time for the weapon to first be fired")] protected float fireWindup;
    [SerializeField, Tooltip("The progress of the weapon's windup. Useful for interpolating visuals.")] protected float currentWindup;
    [SerializeField, Tooltip("")] protected float windupDecay;
    [SerializeField, Tooltip("If true, this weapon's windup will be reset after [FireIntervalRemaining] reaches zero.")] bool resetWindupAfterFiring;
    [SerializeField, Tooltip("The maximum range of the weapon. Weapons will not do damage beyond their maximum range")] protected float maxRange;
    [SerializeField, Tooltip("Should the spread be distributed evenly for every fire iteration? If false, spread will be randomised.")] protected bool unifiedSpread;
    [SerializeField, Tooltip("Bounds between which to generate a circular random spread value")] protected Vector2 minSpread, maxSpread;
    [SerializeField, Tooltip("How many times we'll fire. If greater than zero, the weapon will fire n times and then disallow firing.\nIf zero, the weapon will fire until the fire input is released.")] protected int burstCount;
    protected int currentBurstCount;
    [SerializeField, Tooltip("The time, in seconds, after which the weapon can fire another burst")] protected float burstCooldown;
    [SerializeField, Tooltip("If true, the weapon will only finish the burst when fire input is held for the duration of the burst.")] protected bool canInterruptBurst;
    [SerializeField, Tooltip("If true, the weapon will automatically fire another burst.")] protected bool canAutoBurst;
    protected bool burstFiring;
    protected bool fireInput;
    /// <summary>
    /// Firing is blocked for one reason or another - typically through animations
    /// </summary>
    protected bool fireBlocked;
    /// <summary>
    /// This weapon is currently performing windup when ForceFirstShot is true.
    /// </summary>
    protected bool windupInProgress;

    [SerializeField] protected ParticleSystem fireParticles;
    [SerializeField] AudioSource fireAudioSource;
    [SerializeField] AudioClip fireAudioClip, lastShotAudioClip;
    [SerializeField] AudioClip windupAudio;
    [SerializeField] float minWindupPitch, maxWindupPitch;
    protected virtual bool CanFire()
    {
        return (fireIntervalRemaining <= 0) && 
            !fireBlocked && 
            (burstCount <= 0 || !burstFiring);
    }
    public void SetFireInput(bool fireInput)
    {
        this.fireInput = fireInput;
    }

    private void FixedUpdate()
    {
        //We can't fire if we're not pressing the fire button
        if (fireInput)
        {
            if(fireWindup > 0)
            {
                //If forceFirstShot is enabled, and we're not already winding up a shot, we'll start the windup
                if (forceFirstShot && !windupInProgress)
                    StartCoroutine(ForcedWindup());
                //Otherwise, we'll increment the windup by FixedDeltaTime
                else if(!burstFiring)
                    currentWindup += Time.fixedDeltaTime;
                //if current windup is done and we're able to fire, then we'll fire
                if(currentWindup >= fireWindup && CanFire())
                {
                    TryFire();
                }
            }
            else
            {
                if (CanFire())
                    TryFire();
            }
        }
        else if(!windupInProgress)
        {
            currentWindup -= Time.fixedDeltaTime * windupDecay;
        }

        if(fireIntervalRemaining > 0)
        {
            fireIntervalRemaining -= Time.fixedDeltaTime;
        }
        currentWindup = Mathf.Clamp(currentWindup, 0, fireWindup);
    }
    void TryFire()
    {
        if(burstCount > 0)
        {
            StartCoroutine(BurstFire());
        }
        else
        {
            FireWeapon();
        }
    }
    void FireWeapon()
    {
        Debug.Log($"Fired {name} @ {System.DateTime.Now}");
        fireIntervalRemaining = fireInterval;
        if (fireParticles)
            fireParticles.Play();
        if(fireAudioSource)
        {
            if (fireAudioClip)
            {
                fireAudioSource.PlayOneShot(fireAudioClip);
            }
        }
        if (resetWindupAfterFiring)
            currentWindup = 0;
    }
    IEnumerator BurstFire()
    {
        burstFiring = true;
        var wu = new WaitUntil(() => fireIntervalRemaining <= 0);
        while (((canInterruptBurst && fireInput) || !canInterruptBurst) && currentBurstCount < burstCount)
        {
            FireWeapon();
            currentBurstCount++;
            yield return wu;
        }
        yield return new WaitForSeconds(burstCooldown);
        if (!canAutoBurst)
        {
            yield return new WaitUntil(() => fireInput == false);
        }
        currentBurstCount = 0;
        burstFiring = false;
        yield break;
    }
    IEnumerator ForcedWindup()
    {
        windupInProgress = true;
        while (currentWindup < fireWindup)
        {
            currentWindup += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        FireWeapon();
        windupInProgress = false;
    }
}
