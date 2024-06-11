using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : Character
{
    protected NavMeshAgent agent;
    [SerializeField] protected Slider healthBarRef;
    protected bool firing;
    public GameObject target;
    public Vector2 randomNoiseTimes;
    public AudioClip[] randomNoises;
    public AudioClip[] painSounds;
    public AudioSource source;
    public AudioClip[] deathsounds;
    public int killValue = 1;
    public enum States
    {
        PATROL,
        CHASE,
        ATTACK,
        RETREAT
    }
    public States state = States.PATROL;
    protected override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = MoveSpeed;
        healthBarRef.maxValue = maxHealth;
        healthBarRef.value = maxHealth;
        Invoke(nameof(PlayRandomSound), Random.Range(randomNoiseTimes.x, randomNoiseTimes.y));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    protected virtual void FixedUpdate()
    {
        EnemyBehaviour();
    }
    public override void Move()
    {
        if(target)
            agent.SetDestination(target.transform.position);
    }
    void PlayRandomSound()
    {
        source.PlayOneShot(randomNoises[Random.Range(0, randomNoises.Length)]);
        Invoke(nameof(PlayRandomSound), Random.Range(randomNoiseTimes.x, randomNoiseTimes.y));
    }
    public override void Die()
    {
        agent.enabled = false;
        rb.isKinematic = false;
        GameManager.instance.EnemyDeath(killValue);
        rb.AddRelativeTorque(Random.onUnitSphere * 50);
        Destroy(gameObject, 2);
        if(animator)
            animator.enabled = false;
        rb.angularDrag = 0;
        source.PlayOneShot(deathsounds[Random.Range(0, deathsounds.Length)]);
        healthBarRef.value = 0;
        healthBarRef.gameObject.SetActive(false);
    }

    protected virtual void EnemyBehaviour()
    {
        
    }
    public override void UpdateHealth(float healthChange, Vector3 damagePosition)
    {
        base.UpdateHealth(healthChange, damagePosition);
        healthBarRef.value = CurrentHealth;
        source.PlayOneShot(painSounds[Random.Range(0, painSounds.Length)]);
        
    }
}
