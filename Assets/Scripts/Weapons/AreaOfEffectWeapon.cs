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

    RaycastHit[] hits;
    HashSet<Collider> validColliderHits = new();
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
            if (!item.attachedRigidbody.TryGetComponent(out Character c))
                continue;

            Debug.DrawLine(hit.point, item.attachedRigidbody.worldCenterOfMass, Color.red, .5f);
            Debug.DrawLine(hit.point, item.attachedRigidbody.worldCenterOfMass + (Vector3.up * 0.3f), Color.red, .5f);
            Debug.DrawLine(hit.point, item.attachedRigidbody.worldCenterOfMass + (Vector3.up * -0.3f), Color.red, .5f);

            float dist = Vector3.Distance(hit.point, item.ClosestPoint(hit.point));
            float dmg = areaDamage * damageFalloff.Evaluate(Mathf.InverseLerp(0, areaRadius, dist));
            c.UpdateHealth(-dmg, hit.point);

        }
        validColliderHits.Clear();
    }
}
