using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Character
{
    private NavMeshAgent agent;
    public Transform target;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = MoveSpeed;
        Move();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Move()
    {
        agent.SetDestination(target.position);
    }
}
