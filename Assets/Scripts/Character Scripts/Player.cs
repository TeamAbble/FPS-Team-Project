using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// Player Controller script, written by Lunar :p
/// Handles the player's input, movement, all that jazz
/// </summary>
public class Player : Character
{
    [SerializeField] Vector2 moveInput, lookInput, lookAngle, oldLookAngle, deltaLookAngle;
    public Vector2 LookInput => lookInput;
    PlayerInput p;
    public bool IsUsingGamepad => p.currentControlScheme.Equals(gamepadString);
        public string gamepadString = "Gamepad";
    [SerializeField] float aimPitchOffset;
    [SerializeField] Transform aimTransform;
    [SerializeField] float drag;
    [SerializeField] bool movingCamera;
    [Tooltip("The transform that directly holds the weapon, NOT the transform for the weapon"), Header("Weapon Sway")] public Transform weaponTransform;
    [SerializeField] Vector3 weaponSwayPositionScalar, weaponSwayRotationScalar;
    [SerializeField] AnimationCurve swayPositionBounceCurve, swayRotationBounceCurve;
    [SerializeField] float swayPositionReturnSpeed, swayRotationReturnSpeed, swayPositionDamping, swayRotationDamping, aimingSwayPositionDamping, aimingSwayRotationDamping, swayPositionMultiplier, swayRotationMultiplier;
    Vector3 weaponSwayPositionTarget, weaponSwayRotationTarget, maxWeaponSwayPosition, maxWeaponSwayRotation, weaponSwayPos, weaponSwayRot;
    Vector3 swayPosDampVelocity;
    float swayPositionReturn, swayRotationReturn;
    Vector3 compositePosition, compositeRotation;
    public RecoilProfile currentRecoilProfile;
    [SerializeField] Vector3 recoilPositionScalar, recoilRotationScalar;
    [SerializeField] float recoilPosReturn, recoilRotReturn;
    Vector3 recoilPosTarget, recoilRotTarget, maxRecoilPos, maxRecoilRot, recoilPosDampVelocity, recoilRotDampVelocity, recoilPos, recoilRot;
    [SerializeField] Quaternion recoilOrientation, cameraRecoilOrientation;
    public WeaponManager weaponManager;
    [SerializeField] Vector3 temporaryAimAngleTarget, temporaryAimAngle;
    [SerializeField] float permanentAimAngle;
    [SerializeField] float permanentAimAngleMultiplier;
    float tempAimAngleLerp;
    Vector3 maxTempAimAngle;
    bool firing;
    public Transform viewCamera, worldCamera;
    public float interactDistance;
    public LayerMask interactLayermask;
    public float passiveHealPerSec, passiveHealDelay;
    float currentHealDelay;
    public float iFrameTime, dodgeDelay, dodgeForce;
    public float current_iFrameTime, currentDodgeDelay;
    public bool iFrame;
    public UnityEvent dodgeEvents;
    public Transform dodgeParticleTransform;
    public float dodgeDamage;
    public float dodgeKnockback;

    

    protected override void Start()
    {
        base.Start();
        if(!weaponManager)
            weaponManager = GetComponent<WeaponManager>();
        UpdateHealth(0, Vector3.zero);
        GameManager.instance.healthbar.maxValue = maxHealth;
        GameManager.instance.healthbar.value = maxHealth;
        GameManager.instance.dodgeBar.maxValue = dodgeDelay;
        p = GetComponent<PlayerInput>();
        transform.localRotation = Quaternion.Euler(0, lookAngle.x, 0);


    }
    private void Aim()
    {
        //Rotate the player based on the delta time
        //If no aim transform is specified, the player is incorrectly set up and will not rotate.
        if (!aimTransform)
            return;
        //Add the look input to the look angle
        
            lookAngle += lookInput * GameManager.instance.lookSpeed * Time.fixedDeltaTime;
            //modulo the look yaw by 360
            lookAngle.y = Mathf.Clamp(lookAngle.y, -85, 85);
        deltaLookAngle = oldLookAngle - lookAngle;
        lookAngle.x %= 360;
        aimTransform.localRotation = Quaternion.Euler(-lookAngle.y + aimPitchOffset, 0, 0);
        transform.localRotation = Quaternion.Euler(0, lookAngle.x, 0);
        oldLookAngle = lookAngle;
    }
    private void Update()
    {
        if (permanentAimAngle > 0)
            permanentAimAngle -= Time.unscaledDeltaTime * currentRecoilProfile.permAimAngleDamp;
        lookAngle.y += Mathf.Max(0, permanentAimAngle) * permanentAimAngleMultiplier;
        temporaryAimAngle = Vector3.Lerp(temporaryAimAngle, temporaryAimAngleTarget, Time.deltaTime * currentRecoilProfile.tempAimAngleDamp);
        aimTransform.localRotation = Quaternion.Euler(temporaryAimAngle + new Vector3(Mathf.Clamp(-lookAngle.y, -90, 90) + aimPitchOffset, 0, 0));

        weaponTransform.SetLocalPositionAndRotation(weaponSwayPos + (recoilPos.ScaleReturn(recoilPositionScalar) * currentRecoilProfile.recoilPosMultiplier),
   Quaternion.Euler(weaponSwayRot) * Quaternion.Euler(recoilRot.ScaleReturn(recoilRotationScalar) * currentRecoilProfile.recoilRotMultiplier));
    }
    void WeaponSwayVisuals()
    {
        weaponSwayPos = Vector3.SmoothDamp(weaponSwayPos, (weaponSwayPositionTarget * swayPositionMultiplier),
            ref swayPosDampVelocity, swayPositionDamping);
        weaponSwayRot = Vector3.LerpUnclamped(weaponSwayRot, weaponSwayRotationTarget * swayRotationMultiplier, Time.smoothDeltaTime * swayRotationDamping);


        viewCamera.SetLocalPositionAndRotation((recoilPos * currentRecoilProfile.viewmodelCameraInfluence).ScaleReturn(currentRecoilProfile.viewPositionScalar), 
            Quaternion.Euler((recoilRot * currentRecoilProfile.viewmodelCameraInfluence).ScaleReturn(currentRecoilProfile.viewRotationScalar)));
        worldCamera.SetLocalPositionAndRotation((recoilPos * currentRecoilProfile.worldCameraInfluence).ScaleReturn(currentRecoilProfile.worldPositionScalar),
            Quaternion.Euler((recoilRot * currentRecoilProfile.worldCameraInfluence).ScaleReturn(currentRecoilProfile.worldRotationScalar)));
    }

    void RecoilMaths()
    {
        if (firing)
        {
            recoilPosTarget -= currentRecoilProfile.firingRecoilPosDamping * Time.fixedDeltaTime * recoilPosTarget;
            recoilRotTarget -= currentRecoilProfile.firingRecoilRotDamping * Time.fixedDeltaTime * recoilRotTarget;

            maxRecoilPos = recoilPosTarget;
            maxRecoilRot = recoilRotTarget;

            tempAimAngleLerp = 0;

            temporaryAimAngleTarget = Vector3.Lerp(temporaryAimAngleTarget, Vector3.zero, currentRecoilProfile.tempAimAngleDecay * Time.fixedDeltaTime);
            maxTempAimAngle = temporaryAimAngleTarget;
        }
        else
        {

            if (recoilPosReturn < 1)
            {
                recoilPosReturn += Time.fixedDeltaTime * currentRecoilProfile.recoilPosReturnSpeed;
            }
            if (recoilRotReturn < 1)
            {
                recoilRotReturn += Time.fixedDeltaTime * currentRecoilProfile.recoilRotReturnSpeed;
            }
            recoilPosTarget = Vector3.LerpUnclamped(maxRecoilPos, Vector3.zero, currentRecoilProfile.recoilPosBounceCurve.Evaluate(recoilPosReturn));
            recoilRotTarget = Vector3.LerpUnclamped(maxRecoilRot, Vector3.zero, currentRecoilProfile.recoilRotBounceCurve.Evaluate(recoilRotReturn));

            if (tempAimAngleLerp < 1)
            {
                tempAimAngleLerp += Time.fixedDeltaTime * currentRecoilProfile.tempAimAngleReturnSpeed;
            }

            temporaryAimAngleTarget = Vector3.LerpUnclamped(maxTempAimAngle, Vector3.zero, currentRecoilProfile.temporaryAimBounceCurve.Evaluate(tempAimAngleLerp));
        }
        firing = false;

        recoilPos = Vector3.SmoothDamp(recoilPos, recoilPosTarget, ref recoilPosDampVelocity, currentRecoilProfile.recoilPosDamping);
        recoilRot = Vector3.SmoothDamp(recoilRot, recoilRotTarget, ref recoilRotDampVelocity, currentRecoilProfile.recoilRotDamping);


    }
    void WeaponSwayMaths()
    {
        if (movingCamera)
        {

            weaponSwayPositionTarget += Time.fixedDeltaTime * (new Vector3(deltaLookAngle.x, 0, deltaLookAngle.y).ScaleReturn(weaponSwayPositionScalar));

            weaponSwayRotationTarget += Time.fixedDeltaTime * (new Vector3(deltaLookAngle.y, deltaLookAngle.x, deltaLookAngle.x).ScaleReturn(weaponSwayRotationScalar));

            maxWeaponSwayPosition = weaponSwayPositionTarget;
            maxWeaponSwayRotation = weaponSwayRotationTarget;


            swayPositionReturn = 0;
            swayRotationReturn = 0;

            weaponSwayPositionTarget -= aimingSwayPositionDamping * Time.fixedDeltaTime * weaponSwayPositionTarget;
            weaponSwayRotationTarget -= aimingSwayRotationDamping * Time.fixedDeltaTime * weaponSwayRotationTarget;
        }
        else
        {
            if (swayPositionReturn < 1)
            {
                swayPositionReturn += Time.fixedDeltaTime * swayPositionReturnSpeed;
                weaponSwayPositionTarget = Vector3.LerpUnclamped(maxWeaponSwayPosition, Vector3.zero, swayPositionBounceCurve.Evaluate(swayPositionReturn));
            }
            if (swayRotationReturn < 1)
            {
                swayRotationReturn += Time.fixedDeltaTime * swayRotationReturnSpeed;
                weaponSwayRotationTarget = Vector3.LerpUnclamped(maxWeaponSwayRotation, Vector3.zero, swayRotationBounceCurve.Evaluate(swayRotationReturn));
            }
        }
    }
    private void FixedUpdate()
    {
        

        if (!IsAlive)
            return;
        rb.drag = drag;
        WeaponSwayMaths();
        WeaponSwayVisuals();

        Move();
        RecoilMaths();

        InteractCheck();

        if(currentHealDelay < passiveHealDelay)
        {
            currentHealDelay += Time.fixedDeltaTime;
        }
        else
        {
            UpdateHealth(passiveHealPerSec * Time.fixedDeltaTime, transform.position);
        }

        iFrame = current_iFrameTime < iFrameTime;
        current_iFrameTime += Time.fixedDeltaTime;
        current_iFrameTime = Mathf.Clamp(current_iFrameTime, 0, iFrameTime);

        currentDodgeDelay += Time.fixedDeltaTime;
        currentDodgeDelay = Mathf.Clamp(currentDodgeDelay, 0, dodgeDelay);
        GameManager.instance.dodgeBar.value = currentDodgeDelay;
    }
    public override void Move()
    {
        //We want to move the player in the direction they're looking
        Vector3 movevec = transform.rotation * new Vector3(moveInput.x, 0, moveInput.y) * MoveSpeed;
        rb.AddForce(movevec);
    }
    [SerializeField] Interactable targeted;
    public void InteractCheck()
    {
        if (Physics.Raycast(worldCamera.position, worldCamera.forward, out RaycastHit hit, interactDistance, interactLayermask))
        {
            if(hit.collider.TryGetComponent(out Interactable i))
            {
                if (!targeted || targeted != i) 
                {
                    GameManager.instance.interactTextBG.SetActive(true);
                    if (i is Purchasable)
                    {
                        var p = i as Purchasable;
                        if (p.Cost >= GameManager.instance.score)
                        {
                            GameManager.instance.interactText.text = $"{p.interactText}\nCan't Afford: ${p.Cost}";
                        }
                        else
                        {
                            GameManager.instance.interactText.text = $"{p.interactText}{(p.cost > 0 ? $"\n ${p.Cost}" : "")}";
                        }
                    }
                    else
                    {
                        GameManager.instance.interactText.text = i.interactText;
                    } 
                }
                targeted = i;
            }
            else
            {
                if (GameManager.instance.interactTextBG.activeInHierarchy)
                    GameManager.instance.interactTextBG.SetActive(false);
                targeted = null;
            }
        }
        else
        {
            if(GameManager.instance.interactTextBG.activeInHierarchy)
                GameManager.instance.interactTextBG.SetActive(false);
            targeted = null;
        }
    }
    public void InteractConfirm()
    {
        if (targeted)
        {
            targeted.Interact();
        }
    }

    #region InputCallbacks
    public void GetMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void GetLookInput(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        movingCamera = lookInput != Vector2.zero;
        if (IsAlive && !GameManager.instance.paused)
            Aim();
    }
    public void GetPauseInput(InputAction.CallbackContext context)
    {
        if (context.performed && IsAlive)
            GameManager.instance.PauseGame(!GameManager.instance.paused);
    }
    public void InteractInput(InputAction.CallbackContext context)
    {
        if (context.performed && !GameManager.instance.paused)
        {
            InteractConfirm();
        }
    }
    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.performed && currentDodgeDelay >= dodgeDelay && moveInput != Vector2.zero && !GameManager.instance.paused)
        {
            Vector3 movevec = transform.rotation * new Vector3(moveInput.x, 0, moveInput.y) * dodgeForce;
            rb.AddForce(movevec, ForceMode.Impulse);
            dodgeParticleTransform.localEulerAngles = new Vector3(0, Mathf.Atan2(moveInput.y, moveInput.x), 0);
            currentDodgeDelay = 0;
            dodgeEvents.Invoke();
            current_iFrameTime = 0;
        }
    }

    #endregion

    public override void UpdateHealth(float healthChange, Vector3 damagePosition)
    {
        if (!iFrame)
        {
            base.UpdateHealth(healthChange, damagePosition);
            GameManager.instance.damageVolume.weight = Mathf.InverseLerp(maxHealth, 0, health);
            GameManager.instance.healthbar.value = health;
            if (healthChange < 0)
            {
                DamageRingManager.Instance.AddRing(damagePosition);
                currentHealDelay = 0;
            }
        }
    }
    public override void Die()
    {
        GameManager.instance.respawnScreen.SetGroupActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ReceiveRecoilImpulse(Vector3 pos, Vector3 rot)
    {
        recoilPosTarget += pos;
        recoilRotTarget += rot;

        recoilPosReturn = 0;
        recoilRotReturn = 0;
        firing = true;

        permanentAimAngle = currentRecoilProfile.permanentAimAnglePerShot;
        
        Vector2 randomCircleVal = Random.insideUnitCircle;
        temporaryAimAngleTarget += new Vector3(currentRecoilProfile.temporaryAimAnglePerShot.x, randomCircleVal.x * currentRecoilProfile.temporaryAimAnglePerShot.y, randomCircleVal.y * currentRecoilProfile.temporaryAimAnglePerShot.z);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (iFrame && collision.rigidbody && collision.rigidbody.TryGetComponent(out Character c))
        {
            c.UpdateHealth(-dodgeDamage, transform.position);
            collision.rigidbody.AddForce(collision.impulse * dodgeKnockback);
        }
    }
    public override void MeleeAttack()
    {
        base.MeleeAttack();
        print("player melee");
    }
}
