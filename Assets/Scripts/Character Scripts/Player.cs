using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Player Controller script, written by Lunar :p
/// Handles the player's input, movement, all that jazz
/// </summary>
public class Player : Character
{
    [SerializeField] Vector2 moveInput, lookInput, lookAngle, oldLookAngle, deltaLookAngle;

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
    protected override void Start()
    {
        base.Start();
        if(!weaponManager)
            weaponManager = GetComponent<WeaponManager>();

    }
    private void Aim()
    {
        //Rotate the player based on the delta time
        //If no aim transform is specified, the player is incorrectly set up and will not rotate.
        if (!aimTransform)
            return;
        //Add the look input to the look angle
        if (!GameManager.instance.weaponWheelOpen)
        {
            lookAngle += lookInput * GameManager.instance.lookSpeed * Time.fixedDeltaTime;
            //modulo the look yaw by 360
            lookAngle.y = Mathf.Clamp(lookAngle.y, -85, 85);
        }
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
    }
    public override void Move()
    {
        //We want to move the player in the direction they're looking
        Vector3 movevec = transform.rotation * new Vector3(moveInput.x, 0, moveInput.y) * MoveSpeed;
        rb.AddForce(movevec);
    }
    [SerializeField] Purchasable targeted;
    public void InteractCheck()
    {
        if (Physics.Raycast(worldCamera.position, worldCamera.forward, out RaycastHit hit, interactDistance, interactLayermask))
        {
            if(hit.collider.TryGetComponent(out Purchasable p))
            {
                targeted = p;
            }
        }
    }
    public void InteractConfirm()
    {
        if (targeted && targeted.cost <= GameManager.instance.currencyOwned)
        {
            targeted.Purchase();
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
        if (context.performed)
            GameManager.instance.PauseGame(!GameManager.instance.paused);
    }
    public void InteractInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InteractConfirm();
        }
    }

    #endregion

    public override void UpdateHealth(int healthChange)
    {
        base.UpdateHealth(healthChange);
        GameManager.instance.damageVolume.weight = Mathf.InverseLerp(maxHealth, 0, health);
    }
    public override void Die()
    {
        GameManager.instance.respawnScreen.SetActive(true);
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
}
