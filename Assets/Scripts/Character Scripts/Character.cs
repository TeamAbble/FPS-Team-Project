using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [SerializeField] protected int maxHealth;
    protected int health;
    [SerializeField] protected float MoveSpeed;

    public void UpdateHealth(int healthChange)//Adds the change in health to the health variable and clamps it to min 0 max maxHealth
    {
        health = Mathf.Clamp(health+healthChange,0,maxHealth);
    }

    abstract public void Move();
}
