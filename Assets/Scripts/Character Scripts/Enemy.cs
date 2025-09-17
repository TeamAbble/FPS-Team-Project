using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.API;
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
    bool ignorecash = false;

    public GameObject deathEffect;
    public float deathEffectKillTime;

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


        GameManager.onWaveSkipped += WaveSkipped;

    }
    void WaveSkipped(int waves)
    {
        if (IsAlive)
        {
            ignorecash = true;
            Die();
        }
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
    float targetUpdateTime;
    public override void Move()
    {
        if (target)
        {
            targetUpdateTime += Time.fixedDeltaTime;
            if(targetUpdateTime >= 0.5f)
            {
                targetUpdateTime = 0;
                agent.SetDestination(target.transform.position);
            }
        }
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

        GameManager.instance.EnemyDeath(killValue, ignorecash);
        rb.AddRelativeTorque(Random.onUnitSphere * 50);
        Destroy(gameObject, 4);
        if(animator)
            animator.enabled = false;
        rb.angularDrag = 0;
        noiseSource.PlayOneShot(deathsounds[Random.Range(0, deathsounds.Length)], 0.9f);
        healthBarRef.value = 0;
        healthBarRef.gameObject.SetActive(false);

        try
        {
            //StatsAndAchievements.Client.SetStat("elims", );
            StatsManager.Instance.UpdateElims(1);
        }
        catch (System.Exception)
        {

            throw;
        }
    }
    private void OnDestroy()
    {
        GameObject go = Instantiate(deathEffect, transform.position, transform.rotation);
        Destroy(go, deathEffectKillTime);
    }

    protected virtual void EnemyBehaviour()
    {
        
    }
    public override void UpdateHealth(float healthChange, Vector3 damagePosition)
    {
        healthChange *= GameManager.cheatsEnabled ? GameManager.ch_damageMultPlayer : 1;
        base.UpdateHealth(healthChange, damagePosition);
        healthBarRef.value = CurrentHealth;
        noiseSource.PlayOneShot(painSounds[Random.Range(0, painSounds.Length)]);
        
        if(healthChange < 0)
        {
            GameManager.instance.OnHitFeedback();
        }
    }
}
