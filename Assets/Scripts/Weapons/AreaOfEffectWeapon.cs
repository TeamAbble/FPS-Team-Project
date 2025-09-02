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

            Debug.DrawLine(hit.point, item.attachedRigidbody.worldCenterOfMass, Color.red, .5f);
            Debug.DrawLine(hit.point, item.attachedRigidbody.worldCenterOfMass + (Vector3.up * 0.3f), Color.red, .5f);
            Debug.DrawLine(hit.point, item.attachedRigidbody.worldCenterOfMass + (Vector3.up * -0.3f), Color.red, .5f);

            if (Physics.Linecast(hit.point, item.ClosestPoint(hit.point), out RaycastHit hit2, damageLayermask, QueryTriggerInteraction.Ignore))
            {
                //Check if we've hit something, check if we've hit the thing we tried to hit
                //Also prevent self-damage. We don't want players blowing themselves up in cqc.
                if(hit2.rigidbody && hit2.rigidbody == item.attachedRigidbody && hit2.rigidbody != wm.p.RB && item.attachedRigidbody.TryGetComponent(out Character c))
                {
                    float d = areaDamage * damageFalloff.Evaluate(Mathf.InverseLerp(0, areaRadius, hit2.distance));
                    print($"{d}, {hit2.distance}");
                    c.UpdateHealth(Mathf.FloorToInt(-d), hit.point);
                }
            }

        }
        validColliderHits.Clear();
    }
}
