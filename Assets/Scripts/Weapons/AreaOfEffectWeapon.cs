using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaOfEffectWeapon : Weapon
{
    public float areaRadius;
    public float areaDamage;
    public GameObject areaParticles;
    public float areaParticleDestroyTime;
    public LayerMask areaLayermask, damageLayermask;
    public AnimationCurve damageFalloff;
    public override void HitEffects(RaycastHit hit)
    {
        base.HitEffects(hit);
        var g = Instantiate(areaParticles, hit.point, Quaternion.identity);
        g.transform.up = hit.normal;
        Destroy(g, areaParticleDestroyTime);
        Collider[] array = Physics.OverlapSphere(hit.point, areaRadius, damageLayermask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < array.Length; i++)
        {
            Collider item = array[i];
            if (!item.attachedRigidbody)
                continue;

            if (Physics.Linecast(hit.point, item.attachedRigidbody.centerOfMass, out RaycastHit hit2, areaLayermask, QueryTriggerInteraction.Ignore)
                || Physics.Linecast(hit.point, item.attachedRigidbody.centerOfMass + (Vector3.up * 0.3f), out hit2, areaLayermask, QueryTriggerInteraction.Ignore)
                || Physics.Linecast(hit.point, item.attachedRigidbody.centerOfMass + (Vector3.up * -0.3f), out hit2, areaLayermask, QueryTriggerInteraction.Ignore))
            {
                if (item.attachedRigidbody.TryGetComponent(out Character c))
                {
                    float d = areaDamage * damageFalloff.Evaluate(Mathf.InverseLerp(0, areaRadius, hit2.distance));
                    c.UpdateHealth(Mathf.FloorToInt(-d), hit.point);
                }
            }
        }
    }
}
