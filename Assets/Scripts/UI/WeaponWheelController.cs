using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponWheelController : MonoBehaviour
{
    public static WeaponWheelController Instance { get; private set; }
    public GameObject weaponWheelPrefab;
    List<GameObject> weaponWheelButtons = new();
    public Vector3 offsetFromCentre;
    public float controllerCursorRadius;
    public Transform weaponWheelParent;
    public TextMeshProUGUI descriptionText;
    public int lastHoveredIndex;
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
        SetDescription("Select a weapon!");
    }
    private void Update()
    {
        if (GameManager.instance.playerRef.IsUsingGamepad)
        {
            Vector2 l = GameManager.instance.playerRef.LookInput.normalized;
            Vector2 screenPos = new Vector2(Screen.width / 2 + l.x, Screen.height / 2 + l.y) * controllerCursorRadius;
            Mouse.current.WarpCursorPosition(screenPos);
        }
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
            weaponWheelButtons.Add(b);
        }
    }
    private void OnEnable()
    {
        SetDescription("Select a weapon!");
    }
    public void SetDescription(string desc)
    {
        descriptionText.text = desc;
    }
    //private void OnValidate()
    //{
    //    if (Application.isPlaying)
    //    {
    //        UpdateWeaponWheel();
    //    }
    //}
}
