using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponWheelButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int weaponIndex;
    public string weaponName;
    public Sprite icon;
    public Image iconRender;
    public TextMeshProUGUI nameDisplay;
    WeaponWheelController controller;
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.instance.playerRef.weaponManager.SwitchWeapon(weaponIndex);
        GameManager.instance.UseWeaponWheel(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        controller.SetDescription(GameManager.instance.playerRef.weaponManager.CurrentWeapon.description);
        controller.lastHoveredIndex = weaponIndex;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        controller.SetDescription("Select a weapon!");
        controller.lastHoveredIndex = GameManager.instance.playerRef.weaponManager.weapons.FindIndex(x => x == GameManager.instance.playerRef.weaponManager.CurrentWeapon);
    }

    private void OnEnable()
    {
        GetComponent<Button>().interactable = weaponIndex < GameManager.instance.playerRef.GetComponent<WeaponManager>().WeaponCount;
        print("Checked weapon is valid!");

    }
    private void Start()
    {
        weaponName = GameManager.instance.playerRef.weaponManager.weapons[weaponIndex].name;
        icon = GameManager.instance.playerRef.weaponManager.weapons[weaponIndex].icon;
        controller = GetComponentInParent<WeaponWheelController>();
        if(nameDisplay)
            nameDisplay.text = weaponName;
        if(icon && iconRender)
            iconRender.sprite = icon;
    }

}
