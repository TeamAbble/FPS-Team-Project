using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Character
{
    private NavMeshAgent agent;
    public Transform target;
    enum States
    {
        PATROL,
        CHASE,
        ATTACK
    }
    States state = States.PATROL;
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
    private void FixedUpdate()
    {
        switch (state)
        {
            case States.PATROL:
                break;
            case States.CHASE:
                break;
            case States.ATTACK:
                break;
        } 
    }
    public override void Move()
    {
        agent.SetDestination(target.position);
    }
}
