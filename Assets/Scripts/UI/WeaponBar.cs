using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponBar : MonoBehaviour
{
    public List<GameObject> weaponIcons;
    public RectTransform root;
    public int currentWeaponIndex;
    public RectTransform selectedWeaponHighlight;
    public static WeaponBar Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            enabled = false;
            return;
        }
    }

    public GameObject weaponBarIconPrefab;

    public void UpdateWeaponBar()
    {
        for (int i = weaponIcons.Count - 1; i >= 0; i--)
        {
            Destroy(weaponIcons[i]);
        }
        weaponIcons.Clear();
        for (int i = 0; i < GameManager.instance.playerRef.weaponManager.weapons.Count; i++)
        {
            var baricon = Instantiate(weaponBarIconPrefab, root).GetComponent<WeaponBarIcon>();
            baricon.Initialise(GameManager.instance.playerRef.weaponManager.weapons[i]);
            weaponIcons.Add(baricon.gameObject);
            if(i == GameManager.instance.playerRef.weaponManager.weaponIndex)
            {
                selectedWeaponHighlight.position = weaponIcons[i].transform.position;
            }
        }
    }

    public void UpdateWeaponHighlight()
    {
        StartCoroutine(WeaponHighlight());
    }
    IEnumerator WeaponHighlight()
    {
        yield return null;
        selectedWeaponHighlight.position = weaponIcons[GameManager.instance.playerRef.weaponManager.weaponIndex].transform.position;
    }
}
