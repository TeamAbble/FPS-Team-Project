using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [System.Serializable]
    public class TracerObject
    {
        public GameObject tracer;
        public Vector3 end;
        public Vector3 start;
        public float lerp;
        public float timeIncrement;
    }
    [System.Serializable]
    public class Magazine
    {
        public Transform magazine;
        public Vector3 startPos;
        public Quaternion startRot;
    }

    List<TracerObject> tracers = new List<TracerObject>();
    [SerializeField, Tooltip("The maximum ammunition held by a weapon at one time. If zero, this weapon does not consume ammo.")] protected float maxAmmo;
    [SerializeField, Tooltip("How much ammunition we currently have.")] protected float currentAmmo;
    [SerializeField, Tooltip("The maximum damage dealt to an enemy.")] protected int damage;
    [SerializeField, Tooltip("How many 'Projectiles' a weapon will fire at an enemy.")] protected int projectilesPerShot;
    [SerializeField, Tooltip("The time, in seconds, between each shot")] protected float fireInterval;
    [SerializeField, Tooltip("The remaining fire interval. Useful for interpolating visuals on weapons.")] protected float fireIntervalRemaining;
    [SerializeField, Tooltip("If true, the weapon will always fire once when clicked, regardless of the windup time.\nIf false, the weapon will only fire when the [CurrentWindup] reaches [FireWindup]")] protected bool forceFirstShot;
    [SerializeField, Tooltip("The wait time for the weapon to first be fired")] protected float fireWindup;
    [SerializeField, Tooltip("The progress of the weapon's windup. Useful for interpolating visuals.")] protected float currentWindup;
    float lastWindup;
    [SerializeField, Tooltip("How quickly the Windup decays when not holding the fire button")] protected float windupDecay;
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
    [SerializeField] protected bool fireInput;
    /// <summary>
    /// Firing is blocked for one reason or another - typically through animations
    /// </summary>
    public bool fireBlocked;

    /// <summary>
    /// This weapon is currently performing windup when ForceFirstShot is true.
    /// </summary>
    protected bool windupInProgress;
    [SerializeField] protected int timesFired;
    [SerializeField] protected ParticleSystem fireParticles;
    [SerializeField] protected AudioSource fireAudioSource;
    [SerializeField] protected AudioClip fireAudioClip, lastShotAudioClip, firstShotAudioClip;
    [SerializeField] protected AudioClip windupAudio;
    [SerializeField] protected float minWindupPitch, maxWindupPitch;
    [SerializeField] protected Transform firePosition;
    [SerializeField] protected GameObject shotEffect;
    [SerializeField] protected float tracerSpeed;
    [SerializeField] protected LayerMask layermask;
    WeaponManager wm;
    Animator animator;
    [SerializeField] bool useLoopedSound;
    public Magazine oldMag, newMag;
    [SerializeField] CinemachineImpulseSource recoilSource;
    [SerializeField] float recoilForce;
    
    private void Start()
    {
        wm = GetComponentInParent<WeaponManager>();
        animator = GetComponentInParent<Character>().Animator;
    }
    bool IsOwnerAlive => (wm && wm.IsAlive);
    public bool isEnemyWeapon;
    protected virtual bool CanFire()
    {
        return (isEnemyWeapon || IsOwnerAlive) && (fireIntervalRemaining <= 0) && 
            !fireBlocked && 
            (burstCount <= 0 || currentBurstCount == 0);
    }
    public void SetFireInput(bool fireInput)
    {
        this.fireInput = fireInput;
        if(useLoopedSound && fireAudioSource)
        {
            fireAudioSource.loop = fireInput;
        }
    }
    bool canfire;
    [SerializeField, Tooltip("If true, this weapon will play a firing animation when fired")] bool useFireAnimation;
    [SerializeField, Tooltip("If true, this weapon will keep playing the same firing animation over and over until the weapon stops being fired.")] bool loopFireAnimation;
    [SerializeField, Tooltip("If true, the fire animation will be played when the windup starts")] bool playAnimationOnWindup;
    [SerializeField, Tooltip("If true, use a different trigger for the windup, thus playing a different animation")] bool windupAnimationIsNotFireAnimation;
    private void OnDisable()
    {
        //Cleanup - Some weapons are non-functional after swapping to another weapon before coroutine-controlled CanFire conditions are reset.
        StopAllCoroutines();
        //Fix for burst fire weapons being non-functional after swapping during burst cooldown.
        burstFiring = false;
        fireBlocked = false;
        currentBurstCount = 0;
        //Fix for weapons using ForceFirstShot being non-functional after swapping to another weapon during forced windup.
        windupInProgress = false;
        currentWindup = 0;
        //Fix for weapons potentially playing looped sounds after switching weapons
        fireAudioSource.loop = false;
        //Ensure weapons cannot fire upon swapping back to this weapon
        fireInput = false;

    }
    private void FixedUpdate()
    {
        //Cache our ability to fire at the start of the fixed update
        canfire = CanFire();
        //We can't fire if we're not pressing the fire button
        if (fireInput)
        {
            if (canfire)
            {
                if (fireWindup > 0)
                {
                    //If forceFirstShot is enabled, and we're not already winding up a shot, we'll start the windup
                    if (forceFirstShot)
                    {
                        if(!windupInProgress && !burstFiring)
                        {
                            StartCoroutine(ForcedWindup());
                        }
                        //If ForceFirstShot is enabled, we don't want to evaluate the Windup every fixed update
                        return;
                    }
                    //Otherwise, we'll increment the windup by FixedDeltaTime
                    if(!burstFiring)
                        currentWindup += Time.fixedDeltaTime;
                    //if current windup is done and we're able to fire, then we'll fire
                    if (currentWindup >= fireWindup)
                    {
                        TryFire();
                    }
                }
                else
                {
                    TryFire();
                }
            }
        }
        else
        {
            //If this weapon does not use ForceFirstShot, then the windup needs to be decremented.
            if (!windupInProgress)
                currentWindup -= Time.fixedDeltaTime * windupDecay;
            //Reset the number of times we've fired to 0
            timesFired = 0;
        }
        //If we're waiting to fire again, continue the timer
        if(fireIntervalRemaining > 0)
        {
            fireIntervalRemaining -= Time.fixedDeltaTime;
        }

        //Clamp the windup so it doesn't get too large and allow the player to "over-charge" a weapon and fire with no windup after holding the button for a while
        currentWindup = Mathf.Clamp(currentWindup, 0, fireWindup);
        //We only want to do all this stuff down here if this weapon is NOT configured to charge up 
        if (!forceFirstShot)
        {
            //If this is the first frame we're winding up for, then we want to do some stuff relating to animations
            if (lastWindup == 0 && currentWindup > 0)
            {
                if (playAnimationOnWindup)
                {
                    animator.SetTrigger(windupAnimationIsNotFireAnimation ? "Windup" : "Fire");
                }
                fireAudioSource.clip = windupAudio;
                fireAudioSource.Play();
            }
            //If the below is true then the windup has ended and we should stop doing windup stuff
            else if (lastWindup > 0 && currentWindup <= 0)
            {
                fireAudioSource.Stop();
                animator.SetTrigger("WindupCancel");
            }
            lastWindup = currentWindup;
        }
    }
    /// <summary>
    /// Moved Tracer Update from MonoBehaviour.FixedUpdate() to be controlled by the WeaponManager/RangedEnemy script, so tracers for disabled weapons are still processed.
    /// </summary>
    public void UpdateTracers()
    {
        //Progress all the tracers for this weapon
        for (int i = tracers.Count - 1; i >= 0; i--)
        {
            if (tracers[i].tracer)
                tracers[i].tracer.transform.position = Vector3.Lerp(tracers[i].start, tracers[i].end, tracers[i].lerp);
            else
            {
                tracers.RemoveAt(i);
                i = Mathf.Min(i + 1, tracers.Count - 1);
                continue;
            }
            tracers[i].lerp += tracers[i].timeIncrement * Time.fixedDeltaTime;
        }
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
        if (useFireAnimation)
            animator.SetTrigger("Fire");
        //Debug.Log($"Fired {name} @ {System.DateTime.Now}");
        fireIntervalRemaining = fireInterval;
        if (fireParticles)
            fireParticles.Play();
        if(fireAudioSource)
        {
            if(timesFired == 0 && firstShotAudioClip)
            {
                fireAudioSource.PlayOneShot(firstShotAudioClip);
                fireAudioSource.clip = fireAudioClip;
                fireAudioSource.Play();
            }
            else if (fireAudioClip && !useLoopedSound)
            {
                fireAudioSource.clip = fireAudioClip;
                fireAudioSource.Play();
            }
        }
        timesFired++;
        if (resetWindupAfterFiring)
            currentWindup = 0;

        Vector3 randomDirection;
        for (int i = 0; i < projectilesPerShot; i++)
        {
            var vec = Random.insideUnitCircle;
            randomDirection = new Vector3()
            {
                x = Mathf.Lerp(minSpread.x, maxSpread.x, vec.x),
                y = Mathf.Lerp(minSpread.y, maxSpread.y, vec.y)
            } + Vector3.forward * maxRange;

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(randomDirection), out RaycastHit hit, maxRange, layermask, QueryTriggerInteraction.Ignore))
            {
                if (hit.rigidbody && hit.rigidbody.TryGetComponent(out Character c))
                {
                    c.UpdateHealth(-damage);
                    print("hit an enemy");
                }
                else
                {
                    print("did not hit enemy");
                }
                Debug.DrawLine(Camera.main.transform.position, hit.point, Color.green, 0.25f);
            }
            else
            {
                print("Did not hit anything");
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.TransformDirection(randomDirection), Color.red, 0.25f);
            }

            if (shotEffect)
            {
                GameObject shotObject = Instantiate(shotEffect, firePosition.position, firePosition.rotation);
                var t = new TracerObject()
                {
                    tracer = shotObject,
                    start = firePosition.position,
                    end = hit.collider ? hit.point : (firePosition.TransformDirection(randomDirection) + firePosition.position),
                    lerp = 0,
                };
                t.timeIncrement = tracerSpeed / Vector3.Distance(t.start, t.end);
                tracers.Add(t);

            }
        }
        if (recoilSource)
            recoilSource.GenerateImpulse(recoilForce);

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
        if (!canAutoBurst)
        {
            fireInput = false;
        }
        yield return new WaitForSeconds(burstCooldown);
        currentBurstCount = 0;
        burstFiring = false;
        yield break;
    }
    IEnumerator ForcedWindup()
    {
        windupInProgress = true;
        fireAudioSource.clip = windupAudio;
        fireAudioSource.Play();
        while (currentWindup < fireWindup)
        {
            currentWindup += Time.fixedDeltaTime;
            fireAudioSource.pitch = Mathf.Lerp(minWindupPitch, maxWindupPitch, Mathf.InverseLerp(0, fireWindup, currentWindup));
            yield return new WaitForFixedUpdate();
        }
        fireAudioSource.pitch = 1;
        TryFire();
        yield return new WaitForFixedUpdate();
        windupInProgress = false;
        yield break;
    }

    private void OnDrawGizmosSelected()
    {
        if (firePosition) {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = firePosition.localToWorldMatrix;
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * maxRange);
            if(maxSpread.x != 0)
                Gizmos.DrawLine(Vector3.zero, Vector3.forward * maxRange + (Vector3.right * maxSpread.x));
            if(minSpread.x != 0)
                Gizmos.DrawLine(Vector3.zero, Vector3.forward * maxRange + (Vector3.right * minSpread.x));
            if(maxSpread.y != 0)
                Gizmos.DrawLine(Vector3.zero, Vector3.forward * maxRange + (Vector3.up * maxSpread.y));
            if (minSpread.y != 0)
                Gizmos.DrawLine(Vector3.zero, Vector3.forward * maxRange + (Vector3.up * minSpread.y));
            Gizmos.matrix = Matrix4x4.identity;
        }
    }

}
