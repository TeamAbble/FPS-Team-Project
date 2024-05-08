using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] protected int maxHealth;
    protected int health;
    [SerializeField] protected float MoveSpeed;
    [SerializeField] Animator animator;
    [SerializeField] Vector3 meleeBounds;
    [SerializeField] Vector3 meleeOffset;
    [SerializeField] LayerMask meleeLayermask;
    [SerializeField] int meleeDamage;
    protected virtual void Start()
    {
        if(!animator)
            animator = GetComponent<Animator>();
        health = maxHealth;
    }
    public bool IsAlive => health > 0;
    public virtual void UpdateHealth(int healthChange)//Adds the change in health to the health variable and clamps it to min 0 max maxHealth
    {
        health = Mathf.Clamp(health+healthChange,0,maxHealth);
        if (health <= 0)
        {
            Die();
        }
        
    }
    public virtual void MeleeAttack()
    {
        Collider[] cols = Physics.OverlapBox(transform.TransformPoint(meleeOffset), meleeBounds / 2, transform.rotation, meleeLayermask);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].TryGetComponent(out Character c))
            {
                c.UpdateHealth(-meleeDamage);
            }
        }
    }
    abstract public void Move();

    abstract public void Die();
    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(meleeOffset, meleeBounds);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
