using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [SerializeField] LayerMask layermask;
    float shotTimer = 2;
    public Weapon weapon;
    [SerializeField] float viewDistance = 30;
    public Transform aimTransform;

    // Update is called once per frame
    void Update()
    {

    }
    protected override void EnemyBehaviour()
    {
        if(weapon)
            weapon.UpdateTracers();

        CheckLineOfSight();
        switch (state)
        {
            case States.CHASE:
                Move();
                aimTransform.localEulerAngles = Vector3.zero;
                break;
            case States.ATTACK:
                if (IsAlive)
                {
                    transform.rotation = Quaternion.LookRotation(target.transform.position - (transform.position + Vector3.down), Vector3.up);
                    aimTransform.forward = target.transform.position - (transform.position + Vector3.down);
                }

                break;
            case States.PATROL:
                break;
            case States.RETREAT:
                break;
            default:
                break;
        }
        if (weapon)
            weapon.SetFireInput(IsAlive && state == States.ATTACK);
    }

    void CheckLineOfSight()
    {
        if (!weapon || !target)
            return;
        if (Physics.Linecast(transform.position, target.transform.position,out RaycastHit hit, layermask, QueryTriggerInteraction.Ignore))
        {
            if (hit.rigidbody && hit.rigidbody.TryGetComponent(out Character player)&&hit.distance<=viewDistance)
            {
                Debug.Log("found");
                agent.enabled = false;
                state = States.ATTACK;
            }
            else
            {
                agent.enabled = true;
                state = States.CHASE;
            }
        }
    }
}
