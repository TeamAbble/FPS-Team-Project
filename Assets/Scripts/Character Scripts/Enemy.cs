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
    public AudioSource noiseSource, ambientSource;
    public AudioClip[] deathsounds;
    public AudioClip movementAudioClip;
    public float healthMultiplier = 1;
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
        maxHealth = GameManager.currentEnemyHealth * healthMultiplier;
        health = maxHealth;
        agent.speed = MoveSpeed;
        healthBarRef.maxValue = maxHealth;
        healthBarRef.value = maxHealth;
        Invoke(nameof(PlayRandomSound), Random.Range(randomNoiseTimes.x, randomNoiseTimes.y));
        ambientSource.clip = movementAudioClip;
        ambientSource.loop = true;
        ambientSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    protected virtual void FixedUpdate()
    {
        EnemyBehaviour();
        if (IsAlive)
        {
            ambientSource.volume = Mathf.InverseLerp(0, MoveSpeed, agent.velocity.magnitude);
        }
        else
        {
            if (ambientSource.isPlaying)
            {
                ambientSource.volume = Mathf.InverseLerp(0, MoveSpeed * 2, rb.velocity.magnitude);
            }
        }
    }
    public override void Move()
    {
        if(target)
            agent.SetDestination(target.transform.position);
    }
    void PlayRandomSound()
    {
        if (IsAlive)
        {
            noiseSource.PlayOneShot(randomNoises[Random.Range(0, randomNoises.Length)]);
            Invoke(nameof(PlayRandomSound), Random.Range(randomNoiseTimes.x, randomNoiseTimes.y));
        }
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
        noiseSource.PlayOneShot(deathsounds[Random.Range(0, deathsounds.Length)]);
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
        noiseSource.PlayOneShot(painSounds[Random.Range(0, painSounds.Length)]);
        
    }
}
