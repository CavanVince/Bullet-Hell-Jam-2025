using System;
using UnityEngine;

public abstract class BaseBullet : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Func<float, float> moveFunc;

    public int baseDamage = 1;
    public int damage;


    protected virtual void Start()
    {
        damage = baseDamage;
        rb = GetComponent<Rigidbody2D>();
    }
}