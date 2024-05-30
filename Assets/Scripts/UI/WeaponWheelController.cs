using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponWheelController : MonoBehaviour
{
    public static WeaponWheelController Instance { get; private set; }
    public GameObject weaponWheelPrefab;
    List<WeaponWheelButton> weaponWheelButtons = new();
    public Vector3 offsetFromCentre;
    public Transform weaponWheelParent;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        gameObject.SetActive(false);
    }

    public void UpdateWeaponWheel()
    {
        for (int i = 0; i < weaponWheelButtons.Count; i++)
        {
            Destroy(weaponWheelButtons[i]);
        }
        weaponWheelButtons.Clear();
        float angle = 360 / (float)GameManager.instance.playerRef.weaponManager.WeaponCount;
        for (int i = 0; i < GameManager.instance.playerRef.weaponManager.WeaponCount; i++)
        {
            var b = Instantiate(weaponWheelPrefab, weaponWheelParent);
            b.GetComponent<WeaponWheelButton>().weaponIndex = i;
            b.transform.localPosition = Quaternion.Euler(0, 0, angle * i) * offsetFromCentre;
        }
    }

    //private void OnValidate()
    //{
    //    if (Application.isPlaying)
    //    {
    //        UpdateWeaponWheel();
    //    }
    //}
}
