using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPrinter : Purchasable
{

    public Transform topPart;
    public Vector3 closedRotation, openRotation;
    public float purchaseTime, closeTime, openTime;
    public AudioSource audioSource;
    public AudioClip startClip, processingClip, doneClip;
    public ParticleSystem processingSystem, doneSystem;
    public float processingNoisePower;
    public Vector3 startPosition;
    public Transform weaponSpawnpoint;
    public float costMultiplier;
    public GameObject spawnedWeapon;
    public bool printing;
    private void Start()
    {
        startPosition = transform.position;
        topPart.localEulerAngles = openRotation;
    }

    public override void Purchase()
    {
        if (!spawnedWeapon && !printing && GameManager.instance.unownedWeapons.Count > 0)
        {
            GameManager.instance.currencyOwned -= GameManager.instance.weaponPrintCost;
            GameManager.instance.weaponPrintCost = Mathf.FloorToInt(GameManager.instance.weaponPrintCost * costMultiplier);
            StartCoroutine(PurchaseAnimation());
        }
    }
    IEnumerator PurchaseAnimation()
    {
        printing = true;
        float t = 0;
        float speed = Time.fixedDeltaTime * Quaternion.Angle(Quaternion.Euler(openRotation), Quaternion.Euler(closedRotation)) * closeTime;
        while (t < 1)
        {
            t += speed;
            topPart.localRotation = Quaternion.Euler(Vector3.Lerp(openRotation, closedRotation, t));
            yield return new WaitForFixedUpdate();
        }
        if(processingSystem)
            processingSystem.Play();
        audioSource.PlayOneShot(startClip);
        t = 0;
        audioSource.clip = processingClip;
        audioSource.loop = true;
        audioSource.Play();
        while (t < purchaseTime)
        {
            t += Time.fixedDeltaTime;
            transform.position = startPosition + (Random.insideUnitSphere * processingNoisePower);
            yield return new WaitForFixedUpdate();
        }
        t = 0;
        speed = Time.fixedDeltaTime * Quaternion.Angle(Quaternion.Euler(openRotation), Quaternion.Euler(closedRotation)) * openTime;
        if (processingSystem)
            processingSystem.Stop();

        if (doneSystem)
            doneSystem.Play();
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = doneClip;
        audioSource.Play();

        //Spawn the weapon in
        int random = Random.Range(0, GameManager.instance.unownedWeapons.Count);
        GameObject w = Instantiate(GameManager.instance.unownedWeapons[random], weaponSpawnpoint.position, weaponSpawnpoint.rotation);
        GameManager.instance.unownedWeapons.RemoveAt(random);
        var p = w.AddComponent<WeaponPurchasable>();
        p.cost = 0;
        spawnedWeapon = p.gameObject;
        p.owningPrinter = this;
        while (t < 1)
        {
            t += speed;
            topPart.localRotation = Quaternion.Euler(Vector3.Lerp(closedRotation, openRotation, t));
            yield return new WaitForFixedUpdate();
        }
        printing = false;
    }
}
