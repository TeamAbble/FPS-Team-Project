using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponWheelButton : MonoBehaviour, IPointerClickHandler
{
    public int weaponIndex;
    public string weaponName;
    public Sprite icon;
    public Image iconRender;
    public TextMeshProUGUI nameDisplay;
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.instance.playerRef.GetComponent<WeaponManager>().SwitchWeapon(weaponIndex);
        GameManager.instance.UseWeaponWheel(false);
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

        if(nameDisplay)
            nameDisplay.text = weaponName;
        if(icon && iconRender)
            iconRender.sprite = icon;
    }

}
